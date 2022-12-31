using HarmonyLib;
using Newtonsoft.Json.Linq;
using SecretHistories.Fucine;
using SecretHistories.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UIWidgets.Extensions;
using UniverseLib;
using OpCodes = System.Reflection.Emit.OpCodes;

namespace Doorways.Internals.Patches
{
    /// <summary>
    /// Patches that modify the behavior of the game's
    /// mod loader mechanism. These patches are designed
    /// to support <see cref="ModLoader.ModLoader"/>
    /// </summary>
    [HarmonyPatch]
    internal class ModLoaderPatcher
    {
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
        internal static void TryRegisterInstancedEntity(IEntityWithId entity, string modName, ref Compendium compendium, ref ContentImportLog contentImportLog)
        {
            var _span = Logger.Instance.Span();

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
            var _span = Logger.Instance.Span();
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
