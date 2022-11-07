using HarmonyLib;
using SecretHistories.Constants.Modding;
using sh.monty.doorways;
using System.IO;
using System.Reflection;
using System;
using Patches = sh.monty.doorways.Patches;
using sh.monty.doorways.Patches.SecretHistories;
using System.Diagnostics;
using sh.monty.doorways.UIExtensions;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using SecretHistories.Fucine;

public static class Doorways
{
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
            var _span = Logger.Span();
            _span.Info("Strike the Earth!");

            // Uncomment to create a harmony log file on your Desktop.
            // Harmony.DEBUG = true;

            _span.Info("Initializing Internals...");
            DoorwaysFramework.Internals._mod.Initialize();

            // UnityExplorer is enabled with a filesystem gate.
            // It has a significant (>1000ms) startup time and
            // induces over two seconds of lag each time a new
            // scene is loaded, so it is off by default.
            if (File.Exists(Path.Combine(ResourceLoader.AssemblyDirectory, "ENABLE_EXPLORER")))
            {
                try
                {
                    _span.Info("Initializing UnityExplorer");
                    UnityExplorer.ExplorerStandalone.CreateInstance(Logger.LogUnityExplorer);
                }
                catch (Exception e)
                {
                    _span.Error("Detected conflict between Harmony and UnityExplorer. UnityExplorer will not be fully loaded.");
                    _span.Debug($"{e}");
                }
            }

            stopwatch.Stop();
            _span.Info($"Initialized in {stopwatch.ElapsedMilliseconds}ms");
        }
        catch (Exception e)
        {
            NoonUtility.LogWarning("Doorways encountered an internal exception: " + e.ToString());
            throw e;
        }
    }
}