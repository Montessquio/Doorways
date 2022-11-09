using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doorways.Internals
{
    /// <summary>
    /// Contains the initialization routine that
    /// in turn initializes all the other Internals modules.
    /// </summary>
    internal class _mod
    {
        public static void Initialize()
        {
            var _span = Logger.Span();

            _span.Debug("Initializing UniverseLib...");
            UniverseLib.Universe.Init(logHandler: Logger.LogUnityExplorer);

            _span.Debug("Initializing patch modules...");
            Patches._mod.Initialize();

            _span.Debug("Internals initialization complete.");
        }
    }
}
