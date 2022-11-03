using HarmonyLib;
using Logger = sh.monty.doorways.logging.Logger;
using Newtonsoft.Json;
using SecretHistories.Assets.Scripts.Application.Entities;
using SecretHistories.Constants.Modding;
using SecretHistories.UI;
using SecretHistories;
using sh.monty.doorways.logging;
using sh.monty.doorways;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using System.Windows.Forms;
using System;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using UnityEngine;
using Patches = sh.monty.doorways.Patches;
using sh.monty.doorways.Patches.SecretHistories;
using System.Diagnostics;

public static class DoorwaysFramework
{
    private static Harmony globalPatcher = new Harmony("sh.monty.doorways");
    internal static Harmony GlobalPatcher { get { return globalPatcher; } }

    // Mod entry point
    public static void Initialise(ISecretHistoriesMod mod)
    {
        // Wrap all code in a `try` block so we can log exceptions
        // before re-emitting them.
        try
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Logger.Initialize(LogLevel.Trace);
            var log = Logger.Span();
            log.Info("Strike the Earth!");

            // Uncomment to create a harmony log file on your Desktop.
            // Harmony.DEBUG = true;

            log.Info("Patching Core Assembly");
            CoreEvents.Initialize();

            // This will emit a log on its own if roost is detected.
            Patches.RoostCompat.Patch(GlobalPatcher);

            log.Info("Initializing Mod Preferences");
            DoorwaysOptions.Initialize();

            try
            {
                // stubbed
            }
            catch(Exception e)
            {
                log.Error("An Exception occurred while initializing Doorways Mods. The mod itself will work, but no Doorways Mods will be enabled.");
                log.Debug($"{e}");
            }

            try
            {
                log.Info("Instantiating UnityExplorer");
                UnityExplorer.ExplorerStandalone.CreateInstance(Logger.LogUnityExplorer);
            }
            catch(Exception e)
            {
                log.Error("Detected conflict between Harmony and UnityExplorer. UnityExplorer will not be fully loaded.");
                log.Debug($"{e}");
            }

            log.Info("Performing miscellaneous patches");
            var assembly = Assembly.GetExecutingAssembly();
            GlobalPatcher.PatchAll(assembly);

            stopwatch.Stop();
            log.Info($"Initialized in {stopwatch.ElapsedMilliseconds}ms");
        }
        catch (Exception e)
        {
            NoonUtility.LogWarning("Doorways encountered an internal exception: " + e.ToString());
            throw e;
        }
    }


    /// <summary>
    /// Signals to Doorways that this class can be loaded
    /// as a Fucine Importable. It must be a child class of
    /// an existing Fucine class or a Doorways entity class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DoorwaysObjectAttribute : Attribute { }

    /// <summary>
    /// A shorthand signal to Doorways that this enum
    /// can be used as a Fucine DeckSpec.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public class DeckAttribute : Attribute { }

    /// <summary>
    /// Contains APIs for other mods to register their own
    /// mod content into the game.
    /// </summary>
    public class Doorways
    {
        
    }

}