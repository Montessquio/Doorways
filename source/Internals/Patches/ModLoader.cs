using HarmonyLib;
using SecretHistories.Constants.Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace sh.monty.doorways.Patches
{
    /// <summary>
    /// Patches that modify the behavior of the game's
    /// mod loader mechanism.
    /// </summary>
    [HarmonyPatch]
    class ModLoader
    {
        private static Span _span = Logger.Span();

    }
}
