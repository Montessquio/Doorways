using Doorways.Events;
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
            var _span = Logger.Instance.Span();

            _span.Debug("Initializing UniverseLib...");
            UniverseLib.Universe.Init(logHandler: Logger.Instance.GetUnityExplorerListener());

            _span.Debug("Initializing patch modules...");
            Patches._mod.Initialize();

            _span.Debug("Initializing miscellaneous modules...");
            SceneEvent.UhOSceneInit += UhOScene.OnUhOSceneInit;

            _span.Debug("Internals initialization complete.");
        }
    }
}
