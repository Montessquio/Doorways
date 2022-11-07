using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace DoorwaysFramework.Internals.Patches
{
    /// <summary>
    /// Contains the initialization routine that
    /// in turn initializes all other Patches.
    /// </summary>
    internal class _mod
    {
        private static Harmony globalPatcher = new Harmony("DoorwaysFramework");
        internal static Harmony GlobalPatcher { get { return globalPatcher; } }

        public static void Initialize()
        {
            var assembly = Assembly.GetExecutingAssembly();
            GlobalPatcher.PatchAll(assembly);
        }
    }
}
