using SecretHistories.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using sh.monty.doorways.logging;
using Logger = sh.monty.doorways.logging.Logger;
using Core = SecretHistories;
using SecretHistories.Fucine;
using SecretHistories.Entities;
using SecretHistories.UI;

namespace sh.monty.doorways.Patches.SecretHistories
{
    /// <summary>
    /// Patches the core game logs to absolutely,
    /// positively, certainly use our Log Level.
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
    }
}
