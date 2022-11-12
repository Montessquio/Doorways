using Doorways.Entities;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using SecretHistories.Fucine;
using SecretHistories.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UniverseLib;
using OpCodes = System.Reflection.Emit.OpCodes;

namespace Doorways.Internals.Patches
{
    /// <summary>
    /// Patches that modify the behavior of the game's
    /// mod loader mechanism.
    /// </summary>
    [HarmonyPatch]
    class ModLoader
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(DataFileLoader), nameof(DataFileLoader.LoadFilesFromAssignedFolder))]
        private static void DataLoadHook(DataFileLoader __instance)
        {
            var _span = Logger.Span();

            var mod_root = Directory.GetParent(__instance.ContentFolder).FullName;
            var mod_id = new DirectoryInfo(mod_root).Name;

            var synopsis = parseDoorwaysMod(mod_root);

            if (synopsis != null)
            {
                JObject dmeta = synopsis["doorways"] as JObject;
                if (dmeta.ContainsKey("dll"))
                {
                    _span.Info($"Detected Doorways Mod {mod_id}");
                    string dll_path;
                    try
                    {
                        dll_path = Path.Combine(mod_root, dmeta["dll"].Value<string>());
                    }
                    catch (Exception)
                    {
                        _span.Error($"Mod {mod_id} had an invalid manifest key: The key 'dll' must be a string.");
                        return;
                    }

                    if (File.Exists(dll_path))
                    {
                        try
                        {
                            Assembly mod = Assembly.LoadFrom(dll_path);
                            _span.Info($"Loading Doorways Plug-in Mod: '{mod.FullName}'");
                            loadDoorwaysMod(mod, mod_id);
                        }
                        catch (Exception e)
                        {
                            _span.Error($"Encountered an error loading Doorways DLL for {mod_id}: {e}");
                        }
                        return;
                    }
                    _span.Error($"Mod {mod_id} specified a DLL path that did not exist: {dll_path}");
                }
            }
        }

        /// <summary>
        /// Attempts to parse the synopsis.json file at a mod folder's root.
        /// If it has a "doorways" top-level key that has a dictionary value,
        /// it returns the whole parsed JSON file. Otherwise, returns null.
        /// </summary>
        private static JObject parseDoorwaysMod(string mod_root)
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

        private static Compendium compendium = null;
        private static FieldInfo entityStores = AccessTools.Field(typeof(Compendium), "entityStores");

        /// <summary>
        /// Loads a Doorways assembly's mod content into the Compendium.
        /// </summary>
        private static void loadDoorwaysMod(Assembly mod, string mod_id)
        {
            // Cache the compendium only once
            if (compendium == null)
            {
                compendium = Watchman.Get<Compendium>();
            }
            var _span = Logger.Span();

            // If the entity store isn't set up yet, then set it up.
            if (entityStores.GetValue(compendium) == null)
            {
                entityStores.SetValue(compendium, new Dictionary<Type, EntityStore>());
            }

            var customNamespace = mod.GetCustomAttribute<DoorwaysNamespaceAttribute>();
            string base_id = mod_id;
            if (customNamespace != null)
            {
                base_id = customNamespace.Id;
            }
            _span.Info($"Determined mod baseid to be '{base_id}' (Attribute was {(customNamespace == null ? "not present" : "present")}");

            // get all [DoorwaysObject]s in the assembly
            IEnumerable<Type> types = mod.GetTypes();
            foreach (Type t in types)
            {
                if (t.IsClass)
                {
                    _span.Debug($"Inspecting type '{t.Name}'");
                    bool hasDoorwaysAttr = t.GetCustomAttribute<DoorwaysObjectAttribute>() != null;
                    _span.Debug($"  Checking for Doorways attribute... {hasDoorwaysAttr}");

                    bool isRegisterable = typeof(IEntityWithId).IsAssignableFrom(t);
                    _span.Debug($"  Checking for registrant status... {isRegisterable}");

                    bool isFactory = typeof(DoorwaysFactory).IsAssignableFrom(t);
                    _span.Debug($"  Checking for factory status... {isFactory}");

                    if (hasDoorwaysAttr && isRegisterable)
                    {
                        TryRegisterStaticEntity(t, base_id, mod_id);
                    }
                    else if (hasDoorwaysAttr && isFactory)
                    {
                        DoorwaysFactory factory = Activator.CreateInstance(t) as DoorwaysFactory;
                        foreach (IEntityWithId entity in factory.GetAll())
                        {
                            Type underlyingType = entity.GetUnderlyingElement();
                            TryRegisterInstancedEntity(entity, t, underlyingType, base_id, mod_id, true);
                        }
                    }
                    else
                    {
                        _span.Error($"Marked entity '{t.FullName}' does not derive from IEntityWithId or DoorwaysFactory and/or is not marked with the [DoorwaysObject] attribute, so it cannot be registered as game data.");
                    }
                }
            }
        }

        /// <summary>
        /// If our derived classes shadow variables in the base class, then those
        /// shadowed values will not persist when core casts them into the base
        /// element class. This sets all overridden (with "new") properties (but 
        /// not fields) from the superclass into the underlying type ancestor.
        /// </summary>
        private static void ApplySuperClassToBaseClass(ref IEntityWithId obj, Type superType, Type underlyingType)
        {
            var _span = Logger.Span();
            Type masterType = obj.GetActualType();
            foreach (PropertyInfo property in masterType.GetProperties(/*BindingFlags.DeclaredOnly | */BindingFlags.Instance | BindingFlags.Public))
            {
                PropertyInfo underlyingProperty = underlyingType.GetProperty(property.Name, BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public);
                if (property.IsDeclaredMember())
                {
                    if (property.CanRead && underlyingProperty != null && underlyingProperty.CanWrite)
                    {
                        underlyingProperty.SetValue(obj, property.GetValue(obj));
                    }
                }
            }
        }

        /// <summary>
        /// Registeres a new entity (card, verb, etc)
        /// created at runtime.
        /// </summary>
        public static void TryRegisterInstancedEntity(IEntityWithId entity, Type superType, Type underlyingType, string base_id, string mod_id = "@anonymous", bool is_factory = false)
        {
            var _span = Logger.Span();

            // If we're dealing with a Factory, then we need
            // to change Id generation semantics to support
            // dynamically generated entities.
            if(is_factory)
            {
                // preserge original behavior
                if(entity.Id == null || entity.Id == "id")
                {
                    _span.Error($"Entities produced by a Factory must have IDs! Caused by: {mod_id}");
                    throw new InvalidDataException("Entities produced by a Factory must have IDs!");
                }
                else if(entity.Id.StartsWith("."))
                {
                    // A . preceding the entity id means that it should
                    // be interpreted as a literal, so remove the dot and proceed.
                    entity.SetId(entity.Id.Remove(0, 1));
                }
                else
                {
                    // If it doesn't start with a dot and it's not
                    // empty, prepend the base id.
                    entity.SetId(base_id + "." + entity.Id);
                }
            }
            else
            {
                // Non-factory behavior. Simply prepend the id and move on.
                if (entity.Id == null || entity.Id == "id")
                {
                    string id = base_id + "." + entity.GetActualType().Name;
                    entity.SetId(id);
                    _span.Warn($"{id}");
                }
            }

            // Set the ID to lowercase, since the game expects all IDs to be
            // lowercase-converted.
            entity.SetId(entity.Id.ToLower());

            // Add the type of the underlying store if it doesnt yet exist
            Dictionary<Type, EntityStore> ecs = entityStores.GetValue(compendium) as Dictionary<Type, EntityStore>;
            if (!ecs.ContainsKey(underlyingType))
            {
                ecs.Add(underlyingType, new EntityStore());
            }

            // Apply the entity's overrides to the underlying type.
            ApplySuperClassToBaseClass(ref entity, superType, underlyingType);

            // Add the new entity to that store
            EntityStore entityStore = ecs[underlyingType];
            if (!entityStore.TryAddEntity(entity))
            {
                _span.Error($"Can't add entity {entity.Id} of type {entity.GetType()}\". Does it already Exist?");
                entity.OnPostImport(new ContentImportLog(), compendium);
            }

            // Verify the entity was added properly
            var e = typeof(Compendium)
                .GetMethod(nameof(compendium.GetEntityById))
                .MakeGenericMethod(underlyingType)
                .Invoke(compendium, new object[] { entity.Id });
            _span.Debug($"Loaded Entity '{entity.GetType().Name}'/'{entity.Id}' for mod '{mod_id}' as '{entity.GetUnderlyingElement().Name}'");

        }

        /// <summary>
        /// Registers some type that derives from AbstractEntity
        /// into the compendium.
        /// </summary>
        private static void TryRegisterStaticEntity(Type t, string base_id, string mod_id = "@anonymous")
        {
            var _span = Logger.Span();
            _span.Debug("  Status OK.");
            try
            {
                object obj = Activator.CreateInstance(t);
                /*obj.GetActualType()
                .InvokeMember(
                    "SetDefaultValues",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod,
                    null,
                    obj,
                    new object[] { }
                );*/
                _span.Debug("  Instantiated entity");
                IEntityWithId o = obj as IEntityWithId;
                compendium.InitialiseForEntityTypes(new Type[] { t });
                _span.Debug("  Initializing new Type in compendium");
                Type underlyingType = o.GetUnderlyingElement();
                string name = underlyingType.Name ?? "null";
                _span.Debug($"    Underlying Compendium type resolved to '{name}'");

                if (underlyingType != null)
                {
                    TryRegisterInstancedEntity(o, t, underlyingType, base_id, mod_id);
                }
                else
                {
                    _span.Debug($"Marked entity '{t.FullName}' does not derive from AbstractEntity, so it cannot be registered as game data.");
                }
            }
            catch (Exception e)
            {
                _span.Error($"Exception occurred constructing and registering '{t.FullName}': {e}");
            }
        }

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

        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CompendiumNullObjectStore), nameof(CompendiumNullObjectStore.GetNullObjectForType))]
        private static object OverrideForSubTypes(object __result, Type forType, string entityId, CompendiumNullObjectStore __instance)
        {
            if(__result == null)
            {
                var _span = Logger.Span($"Attempting to get null object for underlying element type '{forType.GetUnderlyingElementType()}'");
                return __instance.GetNullObjectForType(forType.GetUnderlyingElementType(), entityId);
            }
            return __result;
        }
        */


    }
}
