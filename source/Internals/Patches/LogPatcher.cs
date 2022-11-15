using SecretHistories.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using Core = SecretHistories;
using SecretHistories.Fucine;
using SecretHistories.Entities;
using SecretHistories.UI;

namespace sh.monty.doorways.Patches.SecretHistories
{
    /// <summary>
    /// Patches the core game logs to absolutely,
    /// positively, certainly use our Log Level.
    /// Also does some other things (see individual patch methods).
    /// </summary>
    internal class LogPatcher
    {
        /// <summary>
        /// Any time the Core engine would request the console
        /// log level, replace it with Doorways' setting.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPriority(int.MaxValue)]
        [HarmonyPatch(typeof(SecretHistory), nameof(SecretHistory.Sensitivity))]
        private static bool ConsoleSensitivity(ref VerbosityLevel __result)
        {
            //NoonUtility.Log();
            __result = Logger.ClampedLogLevel();
            return false;
        }

        /// <summary>
        /// Any time the Core Engine would create a new GameObject
        /// for each log message, simply append the log text to an existing gameobject instead.
        /// </summary>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SecretHistory), nameof(SecretHistory.AddMessage))]
        private static bool AddMessageToDebugLog()
        {
            // TODO
            return true;
        }
    }
}
