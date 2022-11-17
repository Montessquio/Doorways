using HarmonyLib;
using SecretHistories.Services;
using SecretHistories.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doorways.Events
{
    [HarmonyPatch]
    public static class SceneEvent
    {
        public static event DoorwaysEventHandler<object> MenuSceneInit;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuScreenController), "InitialiseServices")]
        private static void RaiseMenuSceneInit()
        {
            if (MenuSceneInit != null)
            {
                MenuSceneInit.Invoke(null);
            }
        }

        public static event DoorwaysEventHandler<StackTrace> UhOSceneInit;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StageHand), nameof(StageHand.LoadInfoScene))]
        private static void RaiseUhOSceneInit()
        {
            if (UhOSceneInit != null)
            {
                UhOSceneInit.Invoke(new StackTrace(true));
            }
        }
    }
}
