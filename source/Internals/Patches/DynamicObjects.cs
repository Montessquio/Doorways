using HarmonyLib;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using UniverseLib;

namespace Doorways.Internals.Patches
{
    // Hooks into the core engine to provide
    // per-manifestation dynamic entity behavior

    /// <summary>
    /// Hooks into the end-of-situation function to
    /// allow recipes to perform custom actions before
    /// they resolve.
    /// </summary>
    [HarmonyPatch]
    internal static class DynamicObjects
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Situation), nameof(Situation.TransitionToState))]
        private static void TransitionPrefix(SituationState newState, Situation __instance)
        {
            var _span = Logger.Instance.Span();

            SituationState originalState = __instance.State;
            _span.Info($"Situation {__instance.CurrentRecipe.Id} is transitioning from {originalState.Identifier} to {newState.Identifier}");
            if(originalState.Identifier == StateEnum.Ongoing)
            {
                switch (newState.Identifier)
                {
                    // Going from Ongoing to RequiringExecution means
                    // a link may or may not be present.
                    case (StateEnum.RequiringExecution):
                    // Going from Ongoing to Complete means this
                    // is a single-step recipe.
                    case (StateEnum.Complete):
                        _span.Info($"Concrete type of current recipe is: {__instance.CurrentRecipe.GetActualType()}");
                        // First, determine if this is a Doorways Dynamic Recipe.
                        if(typeof(IDynamicRecipe).IsAssignableFrom(__instance.CurrentRecipe.GetActualType()))
                        {
                            // Then, execute its dynamic resolution.
                            IDynamicRecipe recipe = (IDynamicRecipe)__instance.CurrentRecipe;
                            recipe.OnPostExecute(__instance);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
