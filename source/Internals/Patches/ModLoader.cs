using HarmonyLib;
using Newtonsoft.Json.Linq;
using SecretHistories.Fucine;
using SecretHistories.Services;
using SecretHistories.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UIWidgets.Extensions;
using UniverseLib;
using static UnityEngine.EventSystems.EventTrigger;
using OpCodes = System.Reflection.Emit.OpCodes;

namespace Doorways.Internals.Patches
{
    /// <summary>
    /// Patches that modify the behavior of the game's
    /// mod loader mechanism.
    /// </summary>
    [HarmonyPatch]
    internal class ModLoader
    {
        /// <summary>
        /// Filled by <see cref="DoorwaysMod"/> constructors. 
        /// Doorways' indirect attribute loading mechanism
        /// (see <see cref="LoadDoorwaysMod(Assembly, string)"/>) 
        /// creates <see cref="DoorwaysMod"/> instances which
        /// implicitly adds tagged mod content into this dictionary.
        /// <para/>
        /// Used by <see cref="PostLoadCompendium"/> to add
        /// mod content to the core engine.
        /// </summary>
        internal static Dictionary<string, DoorwaysMod> Mods = new Dictionary<string, DoorwaysMod>();

        /// <summary>
        /// Runs once for each enabled mod.
        /// <para/>
        /// Checks the mod synopsis to see if it's a Doorways
        /// mod; if it is, it reads all doorways data out of it
        /// and into the mods registry (<see cref="Mods"/>)
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(DataFileLoader), nameof(DataFileLoader.LoadFilesFromAssignedFolder))]
        private static void PostLoadMod(DataFileLoader __instance)
        {
            var _span = Logger.Span();

            var mod_root = Directory.GetParent(__instance.ContentFolder).FullName;
            var mod_id = new DirectoryInfo(mod_root).Name;

            var synopsis = ParseDoorwaysMod(mod_root);

            if (synopsis != null)
            {
                JObject dmeta = synopsis["doorways"] as JObject;
                if (dmeta.ContainsKey("dll"))
                {
                    string dll_path;
                    try
                    {
                        dll_path = Path.Combine(mod_root, dmeta["dll"].Value<string>());
                    }
                    catch (Exception)
                    {
                        _span.Error($"Could not load mod '{mod_id}'. Reason: Invalid manifest key: The key 'dll' must be a string.");
                        return;
                    }

                    if (File.Exists(dll_path))
                    {
                        try
                        {
                            Assembly mod = Assembly.LoadFrom(dll_path);
                            _span.Info($"Loading Doorways Plug-in Mod: '{mod.FullName}'");
                            LoadDoorwaysMod(mod, mod_id);
                        }
                        catch (Exception e)
                        {
                            _span.Error($"Could not load mod '{mod_id}'. Reason: {e}.");
                        }
                        return;
                    }
                    _span.Error($"Could not load mod '{mod_id}'. Reason: Specified a DLL path that did not exist, \"{dll_path}\"");
                }
            }
        }

        private static bool PostLoadCompendium_HasRun = false;
        /// <summary>
        /// Runs once after all mods have been loaded and after
        /// the compendium has been initialized.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CompendiumLoader), nameof(CompendiumLoader.PopulateCompendium))]
        private static ContentImportLog PostLoadCompendium(ContentImportLog __result)
        {
            if(PostLoadCompendium_HasRun) { return __result; }
            PostLoadCompendium_HasRun = true;

            Compendium compendium = Watchman.Get<Compendium>();
            Mods.Values.ForEach((mod) => RegisterDoorwaysMod(mod, ref compendium, ref __result));
            return __result;
        }

        /// <summary>
        /// Attempts to parse the synopsis.json file at a mod folder's root.
        /// If it has a "doorways" top-level key that has a dictionary value,
        /// it returns the whole parsed JSON file. Otherwise, returns null.
        /// </summary>
        private static JObject ParseDoorwaysMod(string mod_root)
        {
            var _span = Logger.Span();

            // fast-exit if this is the core directory
            if (Directory.GetParent(mod_root).Name == "StreamingAssets")
            {
                return null;
            }

            var syn_path = Path.Combine(mod_root, "synopsis.json");
            JObject synopsis = null;
            try
            {
                synopsis = JObject.Parse(File.ReadAllText(syn_path));
            }
            catch (Exception e) { _span.Debug($"Unable to load synopsis.json for path {syn_path}: {e}"); }
            if (synopsis != null && synopsis.ContainsKey("doorways"))
            {
                if (synopsis["doorways"] is JObject)
                {
                    return synopsis;
                }
            }
            return null;
        }

        /// <summary>
        /// Registers a Doorways mod assembly into <see cref="Mods"/>
        /// <para/>
        /// In order to present your DLL as a Doorways mod, add
        /// the [Doorways] attribute to its assembly properties.
        /// <para/>
        /// Doorways will automatically register any types that
        /// derive from <see cref="AbstractEntity{T}"/> and
        /// have the [<see cref="DoorwaysObjectAttribute"/>]
        /// attribute. It will automatically construct any
        /// types that inherit from <see cref="DoorwaysFactory"/>
        /// and have the [<see cref="DoorwaysObjectAttribute"/>]
        /// and register the Entities they produce.
        /// </summary>
        private static void LoadDoorwaysMod(Assembly mod, string mod_id)
        {
            var _span = Logger.Span();

            var doorwaysModAttr = mod.GetCustomAttribute<DoorwaysAttribute>();

            string ModName = doorwaysModAttr.Name ?? mod.FullName;
            string ModPrefix = doorwaysModAttr.Prefix ?? mod_id;

            // Instantiating this class automatically adds the instance
            // to the doorways mod registry.
            DoorwaysMod thisMod = new DoorwaysMod(ModName, ModPrefix);
            _span.Info($"Detected Doorways Mod {thisMod.ModName} with baseid {thisMod.ModId}");

            // Get all [DoorwaysObject]s in the assembly
            IEnumerable<Type> types = mod.GetTypes();
            foreach (Type t in types)
            {
                if (t.IsClass)
                {
                    // Determine if this is a valid entity we can register.
                    _span.Debug($"Inspecting type '{t.Name}'");
                    bool hasDoorwaysAttr = t.GetCustomAttribute<DoorwaysObjectAttribute>() != null;
                    if(hasDoorwaysAttr)
                    {
                        bool isRegisterable = typeof(IEntityWithId).IsAssignableFrom(t);
                        bool isFactory = typeof(DoorwaysFactory).IsAssignableFrom(t);
                        _span.Debug($"  {t.Name}: (Registerable: {(isRegisterable ? "true" : "false")}, Factory: {(isFactory ? "true" : "false")})");

                        if (isRegisterable)
                        {
                            thisMod.RegisterEntity(InstantiateStaticEntity(t));
                        }
                        else if (isFactory)
                        {
                            DoorwaysFactory factory = Activator.CreateInstance(t) as DoorwaysFactory;
                            factory.GetAll().ForEach((entity) => thisMod.RegisterEntity(entity));
                        }
                        else
                        {
                            _span.Error($"Marked entity '{t.FullName}' does not derive from IEntityWithId or DoorwaysFactory and/or is not marked with the [DoorwaysObject] attribute, so it cannot be registered as game data.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Registers all the content in a given doorways mod
        /// into the core game engine.
        /// <para />
        /// This method should <b>never</b> throw an exception,
        /// and instead simply emit an error in the log
        /// with exception information.
        /// </summary>
        private static void RegisterDoorwaysMod(DoorwaysMod mod, ref Compendium compendium, ref ContentImportLog log)
        {
            var _span = Logger.Span();
            try
            {
                foreach (IEntityWithId entity in mod.registry.Values)
                {
                    TryRegisterInstancedEntity(entity, mod.ModName, ref compendium, ref log);
                }
            }
            catch(Exception e)
            {
                // Misbehaving mods don't get to be in the mod registry.
                Mods.Remove(mod.ModId);
                _span.Error($"Could not register content for Doorways mod '{mod.ModName}': {e}");
            }
        }

        /// <summary>
        /// Instantiates a type into a concrete
        /// <see cref="IEntityWithId"/> that
        /// can be then registered in the game's
        /// compendium.
        /// </summary>
        private static IEntityWithId InstantiateStaticEntity(Type t)
        {
            var _span = Logger.Span();
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

        #region Compendium Manipulation
        /// <summary>
        /// Registeres a new entity (card, verb, etc)
        /// created at runtime into the core game
        /// engine's <see cref="Compendium"/>.
        /// <para/>
        /// This function does not check its input - 
        /// it is the responsibility of the caller to 
        /// ensure input data is correct!
        /// </summary>
        private static void TryRegisterInstancedEntity(IEntityWithId entity, string modName, ref Compendium compendium, ref ContentImportLog contentImportLog)
        {
            var _span = Logger.Span();

            // Set the ID to lowercase, since the game expects all IDs to be
            // lowercase-converted.
            entity.SetId(entity.Id.ToLower());

            // Ensure the compendium is ready to receive our types.
            // Realistically we only need to do this for one of the three,
            // but it costs very little to pre-initialize so we'll just
            // add them all at once.
            //
            // Remember that this method is patched - see the transpiler
            // method below for more details.
            compendium.InitialiseForEntityTypes(new Type[] { entity.GetType(), entity.GetActualType(), entity.GetUnderlyingElement() });

            // Add the type of the underlying store if it doesnt yet exist
            FieldInfo entityStores = AccessTools.Field(typeof(Compendium), "entityStores");
            Dictionary<Type, EntityStore> ecs = entityStores.GetValue(compendium) as Dictionary<Type, EntityStore>;
            if (!ecs.ContainsKey(entity.GetUnderlyingElement()))
            {
                ecs.Add(entity.GetUnderlyingElement(), new EntityStore());
            }

            // Apply the entity's overrides to the underlying type.
            //ApplySuperClassToBaseClass(ref entity, entity.GetActualType(), entity.GetUnderlyingElement());

            // Add the new entity to the entity type store
            EntityStore entityStore = ecs[entity.GetUnderlyingElement()];
            if (!entityStore.TryAddEntity(entity))
            {
                _span.Error($"Can't add entity {entity.Id} of type {entity.GetType()}\". Does it already Exist?");
            }

            // Initialize the newly added entity.
            entity.OnPostImport(contentImportLog, compendium);

            // Verify the entity was added properly
            var e = typeof(Compendium)
                .GetMethod(nameof(compendium.GetEntityById))
                .MakeGenericMethod(entity.GetUnderlyingElement())
                .Invoke(compendium, new object[] { entity.Id });
        }
        

        #endregion

        #region Core Compat Patches

        /// <summary>
        /// In the transpiler function below, we remove the
        /// construction of an empty EntityStore dictionary.
        /// We still need to make sure it's non-null by the
        /// time InitialiseForEntityTypes is called, so we'll
        /// construct it here in the prefix if and only if 
        /// it's not already constructed.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Compendium), nameof(Compendium.InitialiseForEntityTypes))]
        private static void EnsureEntityStoresIsNotNull(ref Dictionary<Type, EntityStore> ___entityStores)
        {
            if (___entityStores == null)
            {
                ___entityStores = new Dictionary<Type, EntityStore>();
            }
        }

        /// <summary>
        /// Doorways might register dynamic types from
        /// doorways submods before InitialiseForEntityTypes
        /// is called, in which case the default implementation
        /// will overwrite the entityStore member, thus
        /// destroying our changes. This removes the overwrite,
        /// and the prefix above ensures it's non-null.
        /// </summary>
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Compendium), nameof(Compendium.InitialiseForEntityTypes))]
        private static IEnumerable<CodeInstruction> DontOverwriteEntityStores(IEnumerable<CodeInstruction> instructions)
        {
            var _span = Logger.Span();
            FieldInfo entityStores = AccessTools.Field(typeof(Compendium), "entityStores");
            foreach (var instruction in instructions)
            {
                if (instruction.StoresField(entityStores))
                {
                    // Replace any lines that would cause EntityStore to be overwritten
                    // with No-Ops
                    yield return new CodeInstruction(OpCodes.Nop);
                }
                else if (instruction.Calls(AccessTools.Method(typeof(Dictionary<Type, EntityStore>), "Add")))
                {
                    // Replace the call to Add() with TryAdd()
                    MethodInfo fn = AccessTools.Method(typeof(Dictionary<Type, EntityStore>), "TryAdd", new Type[] { typeof(Type), typeof(EntityStore) });
                    yield return new CodeInstruction(OpCodes.Callvirt, fn);
                    yield return new CodeInstruction(OpCodes.Pop);
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        #endregion
    }
}
