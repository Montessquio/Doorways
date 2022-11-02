using HarmonyLib;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Services;
using SecretHistories.UI;
using sh.monty.doorways.Patches.SecretHistories;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = sh.monty.doorways.logging.Logger;
using LogLevel = sh.monty.doorways.logging.LogLevel;

namespace sh.monty.doorways
{
    /// <summary>
    /// Processes and applies values set in the "Doorways" section
    /// of the game options pane.
    /// </summary>
    class DoorwaysOptions
    {
        // All the option IDs defined in `content/settings.json`.
        public const string consoleVerbosityID = "doorways.consoleverbosity";

        public static void Initialize()
        {
            CoreEvents.onMenuSceneInit += ApplyConfigs;
        }

        private static void ApplyConfigs()
        {
            Logger.Info("Applying Config Settings");
            new ConsoleVerbosity();
        }

        internal class ConsoleVerbosity : ISettingSubscriber
        {
            public ConsoleVerbosity()
            {
                // Subscribe to the compendium singleton and let Core know that we're interested
                // in a specific setting.
                Watchman.Get<Compendium>().GetEntityById<Setting>(consoleVerbosityID).AddSubscriber(this);
            }

            public void WhenSettingUpdated(object newValue)
            {
                Logger.MinLogLevel = (LogLevel)((int)newValue);
                Logger.Info($"Set LogLevel to {Logger.MinLogLevel}");
            }

            public void BeforeSettingUpdated(object oldValue) { }
        }
    }
}
