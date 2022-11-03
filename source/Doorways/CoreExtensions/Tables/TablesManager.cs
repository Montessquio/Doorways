using HarmonyLib.Tools;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Spheres;
using SecretHistories.UI;
using sh.monty.doorways.Patches.SecretHistories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = sh.monty.doorways.logging.Logger;

namespace sh.monty.doorways.CoreExtensions.Tables
{
    /// <summary>
    /// Create new game tables, which act as their own
    /// containers for cards and situations.
    /// <para />
    /// Tables have their own unique set of cards
    /// and verbs/situations, and in pure JSON it 
    /// is only possible to move game elements 
    /// between them using expulsion recipes.
    /// However, DLL mods do get more control
    /// over how their tables work.
    /// </summary>
    public class TablesManager
    {
        internal static GameObject drag_rect;
        internal static GameObject content;
        internal static GameObject container_template;

        internal static Dictionary<string, Table> tables = new Dictionary<string, Table>();

        public static void Initialize()
        {
            CoreEvents.onTabletopSceneInit += Start;
        }

        private static void Start()
        {
            var _span = Logger.Span();

            _span.Info("Hello from Tables!");

            _span.Debug("Clearing internal table cache");
            tables.Clear();

            try
            {
                drag_rect = GameObject.Find("MasterCanvas")
                    .transform.Find("CameraDragRect")
                    .gameObject;
                content = drag_rect
                    .transform.Find("Content")
                    .gameObject;
                GameObject nav_bounds = content
                    .transform.Find("CameraNavBounds")
                    .gameObject;
                GameObject interaction_blocker = content
                    .transform.Find("InteractionBlocker")
                    .gameObject;
                GameObject numa = content
                    .transform.Find("Numa")
                    .gameObject;
                GameObject enroute = content
                    .transform.Find("EnRouteSphere")
                    .gameObject;

                try
                {
                    // Move the camera navigation bounds outside of the
                    // game's content container.
                    // This allows us to enable/disable the content
                    // container without disabling camera navigation
                    // for all other tables.
                    nav_bounds.transform.SetParent(drag_rect.transform);
                    interaction_blocker.transform.SetParent(drag_rect.transform);
                    numa.transform.SetParent(drag_rect.transform);
                    enroute.transform.SetParent(drag_rect.transform);
                }
                catch (Exception e)
                {
                    _span.Error($"Exception while deparenting GameObjects: {e}");
                    throw e;
                }
            }
            catch(Exception e)
            {
                _span.Error($"Exception while marshalling GameObjects: {e}");
                throw e;
            }

            _span.Debug("Enumerating Spheres:");
            foreach (Sphere s in FucineRoot.Get().Spheres)
            {
                _span.Debug($"  {s.GetAbsolutePath().ToString()}");
            }

            try
            {
                Table experiment = new Table("experimental table");
                experiment.ExecuteTestColor();
            }
            catch (Exception e)
            {
                _span.Error($"Exception while creating test table: {e}");
                throw e;
            }

            _span.Debug("Enumerating Spheres:");
            foreach (Sphere s in FucineRoot.Get().Spheres)
            {
                _span.Debug($"  {s.GetAbsolutePath().ToString()}");
            }
        }
    }
}
