using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.UI;

namespace sh.monty.doorways.Patches.SecretHistories
{
    /// <summary>
    /// Patches that allow Doorways to detect
    /// when a card would decay, and replace
    /// what it decays to.
    /// </summary>
    [HarmonyPatch]
    internal class DecayInterceptPatches
    {
        /// <summary>
        /// <c>oldElementID</c>: The element that just decayed.
        /// <para />
        /// <c>oldDecayTargetID</c>: The element that would be created.
        /// </summary>
        /// <param name="oldElementID">The element that just decayed.</param>
        /// <param name="oldDecayTargetID">The element that would be created.</param>
        /// <returns>The new element ID to decay into</returns>
        public delegate string ElementDecayOverrider(string oldElementID, string oldDecayTargetID);

        /// <summary>
        /// Elements inserted into this dictionary will be run when a card with the same ID
        /// as an existing key begins to decay. The original card and the card it wants to
        /// decay into will be passed to the caller (although the decay target ID may be empty
        /// or null).
        /// <para />
        /// For example, if the key is "restlessness", then the associated function will
        /// be called whenever a restlessness decays.
        /// </summary>
        public static Dictionary<string, ElementDecayOverrider> DecayOverrides = new Dictionary<string, ElementDecayOverrider>();

        /// <summary>
        /// Elements inserted into this dictionary will be run when a card would decay into
        /// another card with the same ID as an existing key.
        /// <para />
        /// For example, if the key is "dread", then the associated function will
        /// be called whenever any card decays into dread.
        /// </summary>
        public static Dictionary<string, ElementDecayOverrider> LateDecayOverrides = new Dictionary<string, ElementDecayOverrider>();

        private static Span _span = Logger.Instance.Span("InterceptDecay");

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Element), "get_DecayTo")]
        private static string InterceptDecay(string oldDecayTargetID, Element __instance)
        {
            string newTargetID = oldDecayTargetID;

            if (DecayOverrides.ContainsKey(__instance.Id))
            {
                newTargetID = DecayOverrides[__instance.Id](__instance.Id, oldDecayTargetID);
                _span.Trace($"Intercepted Decay: '{__instance.Id}' will decay to '{newTargetID}' instead of '{oldDecayTargetID}'");
            }
            // Check for a LateDecayOverride
            if (LateDecayOverrides.ContainsKey(newTargetID))
            {
                string lateTargetID = LateDecayOverrides[newTargetID](__instance.Id, newTargetID);
                _span.Trace($"Intercepted LateDecay: '{__instance.Id}' will decay to '{lateTargetID}' instead of '{newTargetID}'");
                return lateTargetID;
            }

            return newTargetID;
        }
    }
}
