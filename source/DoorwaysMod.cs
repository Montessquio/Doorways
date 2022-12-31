using Doorways.Entities;
using Doorways.Internals;
using Doorways.Internals.ModLoader;
using Doorways.Internals.Patches;
using HarmonyLib;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using SecretHistories.Constants.Modding;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UIWidgets.Extensions;
using UniverseLib;

namespace Doorways
{
    public interface IDoorwaysMod
    {
        string ModName { get; }
        string ModPrefix { get; }
    }

    // Mod metadata information passed to Doorways mod plugins.
    public interface IDoorwaysPlugin
    {
        /// <summary>
        /// The ID of the Mod this plugin is registered with.
        /// </summary>
        string ModName { get; }

        /// <summary>
        /// The mod prefix as set in the mod's synopsis.
        /// </summary>
        string ModPrefix { get; }

        /// <summary>
        /// Register a new game content entity with this plugin.
        /// This method does not pre-canonicalize the fields of the
        /// provided entity.
        /// </summary>
        void RegisterEntity(IEntityWithId entity);

        /// <summary>
        /// Register a new game content entity with this plugin.
        /// This method automatically canonicalizes the fields of 
        /// the provided entity.
        /// </summary>
        void RegisterEntity(INamespacedIDEntity entity);

        /// <summary>
        /// Informs Doorways that the caller would like to be the one in charge
        /// of converting static (JSON, etc) data into IEntityWithIDs. This is on
        /// a first come, first serve basis, so future callers will get an exception
        /// if they try to register a handler for the same entityTag.
        /// </summary>
        void RegisterEntityType(string entityTag, CustomEntity.EntityConstructor constructor);

        /// <summary>
        /// Manually canonicalize an ID with this plugin's metadata.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string CanonicalizeId(string id);
    }

    /// <summary>
    /// Contains information used by Doorways to load mods.
    /// </summary>
    public class DoorwaysMod : IDoorwaysMod
    {
        public string ModName { get; private set; }
        public string ModPrefix { get; private set; }

        // Holds the raw data from every plugin, as well as all their static defs.
        internal Dictionary<string, DoorwaysPlugin> plugins { get; private set; } = new Dictionary<string, DoorwaysPlugin>();

        // This registry holds top-level defs, i.e. raw JSON defs not produced by any plugin.
        internal CanonicalMap<string, IEntityWithId> registry;

        public void RegisterEntity(IEntityWithId entity)
        {
            var _span = Logger.Instance.Span();

            if (registry.ContainsKey(entity.Id))
            {
                throw new ArgumentException($"The mod {ModName} has already registered an entity with ID {entity.Id}");
            }
            registry.Add(entity.Id, entity);
            _span.Debug($"Loaded Entity '{entity.Id}'/'{entity.Id}' for mod '{ModName}' as '{entity.GetUnderlyingElement().Name}'");
        }

        public void RegisterPlugin(Assembly pluginAssembly)
        {
            RegisterPlugin(new DoorwaysPlugin(pluginAssembly));
        }

        public void RegisterPlugin(DoorwaysPlugin p)
        {
            var _span = Logger.Instance.Span();

            if (plugins.ContainsKey(p.Prefix))
            {
                throw new ArgumentException($"The mod {ModName} has already registered a plugin with ID {p.Prefix}");
            }

            plugins.Add(p.Prefix, p);
            p.ModName = this.ModName;
            p.ModPrefix = this.ModPrefix;

            _span.Debug($"Loaded Plugin '{p.Name}' for mod '{ModName}' as '{p.Prefix}'");
        }

        public string CanonicalizeId(string item)
        {
            return IDCanonicalizer.CanonicalizeId(ModPrefix, item);
        }

        public LoadedDataFile CanonicalizeId(LoadedDataFile item)
        {
            return IDCanonicalizer.CanonicalizeId(ModPrefix, item);
        }

        public DoorwaysMod(string modName, string prefix = null)
        {
            ModName = modName;
            ModPrefix = prefix ?? ModName.ToLower().Replace(" ", "");

            registry = new CanonicalMap<string, IEntityWithId>(CanonicalizeId, ModPrefix);
        }

        /// <summary>
        /// Attempt to load a Doorways Mod from a JObject containing the synopsis.
        /// Does not load plugins or content.
        /// </summary>
        public static bool TryFrom(JObject synopsis, out DoorwaysMod mod)
        {
            if (synopsis != null && synopsis.ContainsKey("doorways", JTokenType.Object))
            {
                string mod_prefix = null;
                JObject manifest = (JObject)synopsis["doorways"];
                if (manifest.ContainsKey("prefix", JTokenType.String))
                {
                    mod_prefix = ((JObject)synopsis["doorways"])["prefix"].ToString();
                }

                mod = new DoorwaysMod(mod_prefix);
                return true;
            }

            mod = null;
            return false;
        }

        /// <summary>
        /// Attempt to load a Doorways Mod object from the synopsis at its
        /// mod root directory. Does not load plugins or content.
        /// </summary>
        public static bool TryFrom(string mod_root, out DoorwaysMod mod)
        {
            var _span = Logger.Instance.Span();

            // fast-exit if this is the core directory
            if (Directory.GetParent(mod_root).Name == "StreamingAssets")
            {
                mod = null;
                return false;
            }

            string syn_path = Path.Combine(mod_root, "synopsis.json");
            JObject synopsis;
            try
            {
                synopsis = JObject.Parse(File.ReadAllText(syn_path));
            }
            catch (Exception e) 
            { 
                throw new Exception($"Unable to load synopsis.json for path {syn_path}", e); 
            }

            return TryFrom(synopsis, out mod);
        }
    }

    public class DoorwaysPlugin : IDoorwaysPlugin
    {
        internal Assembly PluginAssembly { get; set; }
        public string Name { get; internal set; } = "NONE";
        public string Prefix { get; internal set; } = null;
        public string ModName { get; internal set; } = "NONE";
        public string ModPrefix { get; internal set; } = null;

        // This registry holds pre-canonicalized defs produced by this plugin.
        internal CanonicalMap<string, IEntityWithId> registry;

        public DoorwaysPlugin(Assembly plugin)
        {
            PluginAssembly = plugin;

            var attr = PluginAssembly.GetCustomAttribute<DoorwaysAttribute>();
            if(attr != null)
            {
                Name = attr.Name;
                Prefix = attr.Prefix;
            }

            registry = new CanonicalMap<string, IEntityWithId>(CanonicalizeId, Prefix);
        }

        public string CanonicalizeId(string rawid)
        {
            var _span = Logger.Instance.Span();

            if (rawid == null) { return null; }

            // If the entity id starts with a dot, we need to
            // remove the dot and allow the remaining ID through as-is.
            if (rawid.StartsWith("."))
            {
                rawid = rawid.Substring(1);
            }
            // If it's not designated as a literal ID,
            // we need to prepend its mod's prefix.
            else
            {
                rawid = Prefix + "." + rawid;
            }

            // Lowercase the whole thing because
            // the core engine expects all IDs to be lowercase.
            return rawid.ToLower();
        }

        /// <summary>
        /// Registers game content with this plugin's
        /// internal registry.
        /// <para/>
        /// Note that while this method accepts any
        /// <see cref="IEntityWithId"/> for flexibility,
        /// you're much better off instantiating or
        /// extending one of the classes in the
        /// <see cref="Doorways.Entities"/> namespace.
        /// </summary>
        public void RegisterEntity(IEntityWithId entity)
        {
            var _span = Logger.Instance.Span();

            if(entity.Id == null || entity.Id == "id")
            {
                entity.SetId(entity.GetActualType().Name.ToLower());
            }

            entity.SetId(CanonicalizeId(entity.Id));
            if (registry.ContainsKey(entity.Id))
            {
                throw new ArgumentException($"The plugin {Name} has already registered an entity with ID {entity.Id}");
            }
            registry.Add(entity.Id, entity);
            _span.Debug($"Loaded Entity '{Name}'/'{entity.Id}' as '{entity.GetUnderlyingElement().Name}'");
        }

        public void RegisterEntity(INamespacedIDEntity entity)
        {
            var _span = Logger.Instance.Span();
            entity.CanonicalizeIds(CanonicalizeId, this.Prefix ?? this.ModPrefix);
            _span.Info($"Canonicaliized '{Name}'/'{entity.Id}'");
            RegisterEntity(entity);
        }

        public void RegisterEntityType(string entityTag, CustomEntity.EntityConstructor constructor)
        {
            CustomEntity.RegisterNewType(ModName, entityTag, constructor);
        }
    }
}
