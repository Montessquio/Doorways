using HarmonyLib;
using SecretHistories.Constants.Modding;
using SecretHistories.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace sh.monty.doorways.Patches
{
    internal class RoostCompat
    {
        private static bool DidPatchAlready = false;
        public static void Patch(Harmony patcher)
        {
            var _span = Logger.Span();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if(assembly.GetName().Name == "TheRoostMachine")
                {
                    _span.Info("Detected The Roost Machine!");
                    if (DidPatchAlready == true)
                    {
                        _span.Error("Trying to patch the Roost Machine for the second time (don't do that!)");
                    }
                    else
                    {
                        _span.Info("Patching The Roost Machine...");
                        try
                        {
                            Type type = assembly.GetType("TheRoostMachine");
                            MethodInfo target = type.GetMethod("Initialise");

                            var prefix = typeof(DoorwaysInjections).GetMethod(nameof(DoorwaysInjections.InitialisePrefix));
                            var postfix = typeof(DoorwaysInjections).GetMethod(nameof(DoorwaysInjections.InitialisePostfix));
                            patcher.Patch(
                                target,
                                prefix: new HarmonyMethod(prefix),
                                postfix: new HarmonyMethod(postfix)
                            );
                        }
                        catch (Exception e)
                        {
                            _span.Error($"Failure patching Roost: {e}");
                        }
                    }
                    break;
                }
            }
        }

        private class DoorwaysInjections
        {
            private static Span _span = Logger.Span("Initialise", "Roost");
            public  static void InitialisePrefix(ref Stopwatch __state)
            {
                __state = new Stopwatch();
                __state.Start();
                // Perform just-in-time patches to The Roost Machine.
            }

            public static void InitialisePostfix(ref Stopwatch __state)
            {
                __state.Stop();
                _span.Info($"Initialized in {__state.ElapsedMilliseconds}ms");
            }
        }

        [HarmonyPatch]
        private class RoostOptionsRemover
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(OptionsPanelTab), nameof(OptionsPanelTab.Activate))]
            public static void OverrideOptionsList(OptionsPanelTab __instance)
            {
                // Remove the roost verbosity settings element,
                // we're overriding it with ours.
                if (__instance.TabId == "UI_ROOST_SETTINGS")
                {
                    var option = __instance.gameObject.transform
                        .parent
                        .parent
                        .Find("ScrollContent")
                        .Find("Viewport")
                        .Find("Content")
                        .Find("SettingsPanel")
                        .Find("SliderSetting_verbosity");
                    option.gameObject.SetActive(false);
                }
            }
        }
    }
}
