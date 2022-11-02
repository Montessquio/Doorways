using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Infrastructure;
using SecretHistories.UI;
using sh.monty.doorways.logging;
using UnityEngine.UIElements;

namespace sh.monty.doorways.Patches.SecretHistories
{
    [HarmonyPatch]
    public class CoreEvents
    {
        public delegate void DEventHandler();

        public static event DEventHandler onQuoteSceneInit;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SplashScreen), "Start")]
        private static void RaiseSplashScreenStartEvent()
        {
            if (onQuoteSceneInit != null)
            {
                onQuoteSceneInit.Invoke();
            }
        }

        public static event DEventHandler onMenuSceneInit;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuScreenController), "InitialiseServices")]
        private static void RaiseInitialiseServicesEvent()
        {
            if (onMenuSceneInit != null)
            {
                onMenuSceneInit.Invoke();
            }
        }

        public static event DEventHandler onTabletopSceneInit;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameGateway), "PopulateEnvironment")]
        private static void RaisePopulateEnvironmentEvent()
        {
            if (onTabletopSceneInit != null)
            {
                onTabletopSceneInit.Invoke();
            }
        }

        public static event DEventHandler onGameOverSceneInit;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameOverScreenController), "OnEnable")]
        private static void RaiseOnEnableEvent()
        {
            if (onGameOverSceneInit != null)
            {
                onGameOverSceneInit.Invoke();
            }
        }

        public static event DEventHandler onNewGameSceneInit;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NewGameScreenController), "Start")]
        private static void RaiseNewGameScreenControllerStartEvent()
        {
            if (onNewGameSceneInit != null)
            {
                onNewGameSceneInit.Invoke();
            }
        }

        public static event DEventHandler onNewGame;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuScreenController), "BeginNewSaveWithSpecifiedLegacy")]
        private static void RaiseBeginNewSaveWithSpecifiedLegacyEvent()
        {
            if (onNewGame != null)
            {
                onNewGame.Invoke();
            }
        }

        public static event DEventHandler onRecipeRequirementsCheck;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Recipe), "RequirementsSatisfiedBy")]
        private static void RaiseRequirementsSatisfiedByEvent()
        {
            if (onRecipeRequirementsCheck != null)
            {
                onRecipeRequirementsCheck.Invoke();
            }
        }

        public static event DEventHandler onCompendiumLoad;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CompendiumLoader), "PopulateCompendium")]
        private static void RaisePopulateCompendiumEvent()
        {
            if (onCompendiumLoad != null)
            {
                onCompendiumLoad.Invoke();
            }
        }

        public static void Initialize()
        {
            // stubbed
        }
    }
}
