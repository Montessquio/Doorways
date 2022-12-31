using SecretHistories.Constants.Modding;
using System.IO;
using System;
using System.Diagnostics;
using Doorways.Events;
using Doorways.Internals.ModLoader;
using Doorways.Internals;
using HarmonyLib;
using MoonSharp.Interpreter;
using System.Runtime.CompilerServices;
using System.Reflection;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Doorways;

public static class DoorwaysFramework
{
    internal static Harmony GlobalPatcher { get; } = new Harmony("DoorwaysFramework");

    // Cultist Simulator runs this function on startup.
    // Everything else is up to us.
    public static void Initialise(ISecretHistoriesMod mod)
    {
        // Wrap all code in a `try` block so we can log exceptions
        // before re-emitting them.
        try
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Logger.Initialize(LogLevel.Trace);
            var _span = Logger.Instance.Span();
            _span.Info("Strike the Earth!");

            // Uncomment to create a harmony log file on your Desktop.
            // Harmony.DEBUG = true;

            _span.Info("Initializing Internals...");
            {
                _span.Debug("Initializing UniverseLib...");
                UniverseLib.Universe.Init(logHandler: Logger.Instance.GetUnityExplorerListener());

                _span.Debug("Initializing patch modules...");
                var assembly = Assembly.GetExecutingAssembly();
                GlobalPatcher.PatchAll(assembly);
                UserData.RegisterAssembly();

                _span.Debug("Initializing mod loader...");
                DFucineHandlerAttribute.Initialize();
                ModLoader.Initialize();

                _span.Debug("Initializing miscellaneous modules...");
                SceneEvent.UhOSceneInit += UhOScene.OnUhOSceneInit;
            }
            _span.Debug("Internals initialization complete.");

            // UnityExplorer is enabled with a filesystem gate.
            // It has a significant (>1000ms) startup time and
            // induces over two seconds of lag each time a new
            // scene is loaded, so it is off by default.
            InitUnityExplorer(_span);

            stopwatch.Stop();
            _span.Info($"Initialized in {stopwatch.ElapsedMilliseconds}ms");
        }
        catch (Exception e)
        {
            NoonUtility.LogWarning("Doorways encountered an internal exception: " + e.ToString());
            throw e;
        }
    }

    private static void InitUnityExplorer(Span _span)
    {
        if (File.Exists(Path.Combine(ResourceLoader.AssemblyDirectory, "ENABLE_EXPLORER")))
        {
            try
            {
                _span.Info("Initializing UnityExplorer");
                UnityExplorer.ExplorerStandalone.CreateInstance(Logger.Instance.GetUnityExplorerListener());
            }
            catch (Exception e)
            {
                _span.Error("Detected conflict between Harmony and UnityExplorer. UnityExplorer will not be fully loaded.");
                _span.Debug($"{e}");
            }
        }
    }
}