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
        private static int MAX_LOGS { get; } = 256;

        /// <summary>
        /// Any time the Core engine would request the console
        /// log level, replace it with Doorways' setting.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPriority(int.MaxValue)]
        [HarmonyPatch(typeof(SecretHistory), nameof(SecretHistory.Sensitivity))]
        private static bool ConsoleSensitivity(ref VerbosityLevel __result)
        {
            __result = Logger.ClampedLogLevel();
            return false;
        }

        /// <summary>
        /// Each log message is its own gameobject.
        /// This patch limits the number of log messages
        /// that are shown in the debug window (but not the log file)
        /// so the game doesn't get immensely laggy with the window
        /// open as time goes on.
        /// <para/>
        /// Technically, the best solution to this would be to
        /// append the *text* into a single GameObject,
        /// but that'd require a lot of logic to preserve
        /// existing behavior semantics. So we're just capping
        /// the max gameobjects in the container for simplicity
        /// and calling it a day.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SecretHistory), nameof(SecretHistory.AddMessage))]
        private static void AddMessageToDebugLog(ref List<SecretHistoryLogMessageEntry> ___entries, ILogMessage message)
        {
            if (___entries.Count > MAX_LOGS)
            {
                foreach (SecretHistoryLogMessageEntry entry in ___entries)
                {
                    if (entry.TryMatchMessage(message))
                    {
                        return;
                    }
                }

                UnityEngine.Object.Destroy(___entries[0].gameObject);
                ___entries.RemoveAt(0);
            }
        }
    }
}
