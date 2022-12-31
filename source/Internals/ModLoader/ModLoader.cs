using Doorways.Entities;
using Doorways.Events;
using Doorways.Internals.Patches;
using HarmonyLib;
using SecretHistories.Constants.Modding;
using SecretHistories.Fucine;
using SecretHistories.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UniverseLib;

namespace Doorways.Internals.ModLoader
{
    /// <summary>
    /// The root of all Doorways mod loader logic.
    /// </summary>
    internal class ModLoader
    {
        private static Dictionary<string, DoorwaysMod> registry= new Dictionary<string, DoorwaysMod>();

        internal static void Initialize()
        {
            LoadEvents.PreLoadMod += ParseMod;
            LoadEvents.PostLoadCompendium += WriteAllMods;
        }

        #region Event Handlers

        /// <summary>
        /// Runs once for every enabled mod. If it is a doorways mod,
        /// parses the mod and takes over mod handling for that mod.
        /// Otherwise, proceeds as usual.
        /// </summary>
        private static void ParseMod(ref LoadEvents.PreLoadModEventData data)
        {
            var _span = Logger.Instance.Span();
            var _timer = new Stopwatch();
            _timer.Start();

            try
            {
                // Load mod Synopsis
                DoorwaysMod mod;
                if (DoorwaysMod.TryFrom(data.Instance.ContentFolder, out mod) && mod != null)
                {
                    if(registry.ContainsKey(mod.ModName))
                    {
                        _timer.Stop();
                        _span.Error($"Could not load Mod '{mod.ModName}' from '{data.Instance.ContentFolder}': a mod with that name is already registered!");
                        return;
                    }

                    // Load Plugins
                    foreach(DoorwaysPlugin pl in LoadPlugins(data.Instance.ContentFolder, mod.ModName))
                    {
                        mod.RegisterPlugin(pl);
                    }

                    // Load Textual Content
                    foreach(IEntityWithId entity in LoadTextualContent(mod, data.Instance.ContentFolder))
                    {
                        if(typeof(INamespacedIDEntity).IsAssignableFrom(entity.GetActualType()))
                        {
                            ((INamespacedIDEntity)entity).CanonicalizeIds(mod.CanonicalizeId, mod.ModPrefix);
                        }
                        mod.RegisterEntity(entity);
                    }

                    // Save mod into registry
                    registry.Add(mod.ModName, mod);
                    data.RunOriginal = false;

                    _timer.Stop();
                    _span.Info($"Parsed Doorways mod {mod.ModName} in {_timer.ElapsedMilliseconds}ms");
                    return;
                }
                else
                {
                    _timer.Stop();
                    _span.Error("DoorwaysMod.TryFrom returned TRUE but mod was NULL!");
                    return;
                }
            }
            catch(Exception e)
            {
                _span.Error($"An exception occurred while loading from '{data.Instance.ContentFolder}': {e}. Aborting...");
            }

            _timer.Stop();
        }

        /// <summary>
        /// Runs once after all mods have been parsed, core has been loaded,
        /// and all non-doorways mods have been loaded.
        /// </summary>
        private static void WriteAllMods(ref LoadEvents.PostLoadCompendiumEventData data)
        {
            var _span = Logger.Instance.Span();
            var _timer = new Stopwatch();
            _timer.Start();

            Compendium compendium = Watchman.Get<Compendium>();

            foreach(DoorwaysMod mod in registry.Values)
            {
                try
                {
                    // Register mod-level defs. These are supplied by textual content
                    foreach (IEntityWithId entity in mod.registry.Values)
                    {
                        ModLoaderPatcher.TryRegisterInstancedEntity(entity, mod.ModName, ref compendium, ref data.Log);
                    }

                    // Register plugin-level defs. These are supplied by plugin DLLs
                    foreach (DoorwaysPlugin plugin in mod.plugins.Values)
                    {
                        foreach(IEntityWithId entity in plugin.registry.Values)
                        {
                            ModLoaderPatcher.TryRegisterInstancedEntity(entity, mod.ModName, ref compendium, ref data.Log);
                        }
                    }
                }
                catch(Exception e)
                {
                    // Misbehaving mods don't get to be in the mod registry.
                    registry.Remove(mod.ModName);
                    _span.Error($"Could not register content for Doorways mod '{mod.ModName}': {e}");
                }
            }

            _timer.Stop();
            _span.Info($"Registered all Doorways Mods in {_timer.ElapsedMilliseconds}ms");
        }

        #endregion

        #region Helpers
        /// <summary>
        /// Iterates through all the DLLs in the mod content
        /// folder and tries to load them as Doorways plug-ins for that mod.
        /// </summary>
        private static IEnumerable<DoorwaysPlugin> LoadPlugins(string contentPath, string mod_id)
        {
            var _span = Logger.Instance.Span();
            foreach (FileInfo file in new DirectoryInfo(contentPath).GetFiles("*.dll"))
            {
                DoorwaysPlugin p = null;
                try
                {
                    // The Plugin is processed here, but
                    // yielded at the bottom of the method
                    // due to the inability to yield from
                    // inside a `try` block.
                    Assembly mod = Assembly.LoadFrom(file.FullName);
                    if (mod.GetCustomAttribute<DoorwaysAttribute>() != null)
                    {
                        _span.Debug($"Loading Doorways Plug-in '{mod.GetName().Name}' for '{mod_id}'.");
                        p = new DoorwaysPlugin(mod);

                        #region Apply Patches
                        OverrideAttributePatches.ApplyPatches(p.PluginAssembly);
                        DFucineHandlerAttribute.Initialize(p);
                        // add more patches here...
                        #endregion

                        #region Run init methods
                        var methods = from t in mod.GetTypes()
                                    from m in t.GetMethods()
                                    where m.GetCustomAttribute<DoorwaysInitAttribute>() != null
                                    select m;

                        foreach (MethodInfo m in methods)
                        {
                            try
                            {
                                DoorwaysInitAttribute.Invoke(m, p);
                            }
                            catch(Exception e)
                            {
                                _span.Error($"An error occured while invoking init method {m.DeclaringType}::{m.Name} - {e}");
                            }
                        }
                        #endregion

                        #region Instantiate all Types and Factories
                        IEnumerable<Type> statics = from typ in mod.GetTypes()
                                                    where typ.GetCustomAttribute<DoorwaysObjectAttribute>() != null
                                                    select typ;

                        #region Single-Types
                        IEnumerable<Type> singles = from typ in statics
                                      where typeof(IEntityWithId).IsAssignableFrom(typ)
                                      select typ;

                        foreach(Type t in singles)
                        {
                            p.RegisterEntity(InstantiateStaticEntity(t));
                        }
                        #endregion

                        #region Factories
                        IEnumerable<MethodInfo> factories = from typ in statics
                                                      from m in typ.GetMethods()
                                                      where m.GetCustomAttribute<DoorwaysFactoryAttribute>() != null
                                                      select m;

                        foreach(MethodInfo m in factories)
                        {
                            IEnumerable<IEntityWithId> factoryOut;
                            try
                            {
                                factoryOut = DoorwaysFactoryAttribute.Invoke(m);
                            }
                            catch (Exception e)
                            {
                                _span.Error($"An error occured while invoking init method {m.DeclaringType}::{m.Name} - {e}");
                                continue;
                            }

                            foreach(IEntityWithId entity in factoryOut)
                            {
                                p.RegisterEntity(entity);
                            }
                        }
                        #endregion

                        #endregion
                    }
                }
                catch (Exception e)
                {
                    _span.Error($"Could not load plugin '{file.FullName}' for '{mod_id}'. Reason: {e}.");
                    continue;
                }
                yield return p;
            }
        }

        /// <summary>
        /// Attempts to load each content file in the content path using the
        /// appropriate loader for that file's format.
        /// </summary>
        private static IEnumerable<IEntityWithId> LoadTextualContent(DoorwaysMod mod, string contentPath)
        {
            var _span = Logger.Instance.Span();
            List<DoorwaysContentLoader> loaders = new List<DoorwaysContentLoader>
            {
                new JSONLoader(mod, contentPath),
                new HJSONLoader(mod, contentPath),
            };

            foreach (DoorwaysContentLoader loader in loaders)
            {
                foreach (LoadedDataFile file in loader.LoadContent())
                {
                    IEntityWithId o;
                    try
                    {
                        o = file.Construct(mod);
                    }
                    catch(Exception e)
                    {
                        _span.Error($"Failed to construct type '{file.EntityTag}' from '{file.Path}': {e}");
                        continue;
                    }
                    yield return o;
                }
            }
        }

        /// <summary>
        /// Instantiates a type into a concrete
        /// <see cref="IEntityWithId"/> that
        /// can be then registered in the game's
        /// compendium.
        /// </summary>
        internal static IEntityWithId InstantiateStaticEntity(Type t)
        {
            var _span = Logger.Instance.Span();
            try
            {
                object obj = Activator.CreateInstance(t);
                IEntityWithId o = obj as IEntityWithId;
                Type underlyingType = o.GetUnderlyingElement();
                if (underlyingType != null)
                {
                    _span.Debug($"  Underlying Compendium type resolved to '{underlyingType.Name ?? "null"}'");
                    return o;
                }
                else
                {
                    throw new ArgumentException($"Marked entity '{t.FullName}' does not derive from AbstractEntity, so it cannot be registered as game data.");
                }
            }
            catch (Exception e)
            {
                _span.Error($"Exception occurred constructing and registering '{t.FullName}': {e}");
                throw e;
            }
        }

        #endregion
    }
}
