using HarmonyLib;
using SecretHistories.Fucine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doorways.Events
{
    /// <summary>
    /// Events fired when the game loads various resources.
    /// </summary>
    [HarmonyPatch]
    public class LoadEvents
    {
        internal class PreLoadModEventData
        {
            public DataFileLoader Instance;
            public ContentImportLog Log;
            public bool RunOriginal;

            public PreLoadModEventData(DataFileLoader instance, ref ContentImportLog log)
            {
                Instance = instance;
                Log = log;
                RunOriginal = true;
            }
        }

        /// <summary>
        /// Runs once for each enabled mod.
        /// </summary>
        internal static event DoorwaysEventHandler<PreLoadModEventData> PreLoadMod;
        
        [HarmonyPrefix]
        [HarmonyPriority(int.MaxValue)] // We want to run absolutely, positively first.
        [HarmonyPatch(typeof(DataFileLoader), nameof(DataFileLoader.LoadFilesFromAssignedFolder))]
        private static bool _PreLoadMod(DataFileLoader __instance, ref ContentImportLog log)
        {
            PreLoadModEventData d = new PreLoadModEventData(__instance, ref log);
            PreLoadMod.Invoke(ref d);
            return d.RunOriginal;
        }

        internal class PostLoadCompendiumEventData
        {
            internal static bool PostLoadCompendium_HasRun = false;

            public ContentImportLog Log;

            public PostLoadCompendiumEventData(ContentImportLog Log)
            {
                this.Log = Log;
            }
        }

        /// <summary>
        /// Runs once after all mods have been loaded and after
        /// the compendium has been initialized.
        /// </summary>
        internal static event DoorwaysEventHandler<PostLoadCompendiumEventData> PostLoadCompendium;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CompendiumLoader), nameof(CompendiumLoader.PopulateCompendium))]
        private static ContentImportLog _PostLoadCompendium(ContentImportLog __result)
        {
            if (PostLoadCompendiumEventData.PostLoadCompendium_HasRun) { return __result; }
            PostLoadCompendiumEventData.PostLoadCompendium_HasRun = true;

            PostLoadCompendiumEventData d = new PostLoadCompendiumEventData(__result);
            PostLoadCompendium.Invoke(ref d);
            return d.Log;
        }
    }
}
