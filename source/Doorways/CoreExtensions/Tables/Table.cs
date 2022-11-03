using SecretHistories.Spheres;
using UnityEngine;
using SecretHistories.UI;
using Logger = sh.monty.doorways.logging.Logger;
using SecretHistories.Entities;
using System.Windows.Documents;
using System.Collections.Generic;
using SecretHistories.Constants;
using UnityEngine.UI;

namespace sh.monty.doorways.CoreExtensions.Tables
{
    public class Table
    {
        public string id { get; private set; }

        private GameObject container;
        private GameObject tableGraphic;

        private AuxTabletopSphere sphere;

        // Numa? Should probably be handled by its own module.
        // Numa works perfectly when extracted to its parent module
        // so we probably can extract it and let other Doorways modules
        // mess with it.

        public Table(string id)
        {
            var _span = Logger.Span();
            this.id = id;

            // Create the table container

            _span.Info($"Creating new Table '{id}'");
            _span.Debug("Instantiating Table Container");
            container = GameObject.Instantiate<GameObject>(TablesManager.content, TablesManager.drag_rect.transform);
            container.SetActive(false);
            container.name = "doorways-table-" + id;
            _span.Debug("Destroying Children");
            foreach (Transform child in container.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            #region Table Graphics

            _span.Debug("Instantiating Table Graphic");
            // Create the Table graphic
            tableGraphic = GameObject.Instantiate(
                TablesManager.content.transform.Find("TabletopBackground").gameObject,
                container.transform
            );
            tableGraphic.name = "TabletopBackground-" + id;

            #endregion

            #region Tabletop Card Sphere

            _span.Debug("Instantiating Table Sphere");
            var original_table = TablesManager.content.transform.Find("TabletopSphere").gameObject;
            // Instantiate a Sphere to hold cards on the table.
            var sphere_go = GameObject.Instantiate(
                original_table,
                container.transform
            );
            sphere_go.name = "TabletopSphere-" + id;

            // Clear old children
            _span.Debug("Stripping Template Children");
            foreach(Transform child in sphere_go.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            // Strip old values
            _span.Debug("Stripping Table Sphere");
            GameObject.Destroy(sphere_go.GetComponent<TabletopSphere>());
            GameObject.Destroy(sphere_go.GetComponent<PermanentRootSphereSpec>());
            // We keep the Choreographer, since it operates on whatever sphere is on
            // this gameObject.

            // Substitute our own values
            _span.Debug("Setting up Table Sphere");
            sphere = sphere_go.AddComponent<AuxTabletopSphere>();
            // Freeze time for this table.
            _span.Debug("  Setting Heartbeat Interval Multiplier");
            sphere.SetTokenHeartbeatIntervalMultiplier(0f);
            _span.Debug("  Setting Tabletop Graphic Reference");
            sphere.SetTabletopBackground(tableGraphic.GetComponent<TabletopBackground>());
            _span.Debug("  Setting Canvas Group Fader");
            sphere.SetCanvasGroupFader(sphere_go.GetComponent<CanvasGroupFader>());
            _span.Debug("  Setting En Route Sphere");
            sphere.SetEnRouteSphere(TablesManager.drag_rect.transform.Find("EnRouteSphere").GetComponent<EnRouteSphere>());
            _span.Debug("  Setting Sphere Coreographer");
            sphere.SetTabletopChoreographer(sphere_go.GetComponent<TabletopChoreographer>());

            _span.Debug("Setting up Sphere Drop Catcher");
            tableGraphic.GetComponent<SphereDropCatcher>().Sphere = sphere;

            // This statement attaches our AuxTabletopSphere to
            // FucineRoot's sphere list, and registers it with
            // HornedAxe via Watchman.
            _span.Debug("Registering Table Sphere into Fucine");
            var spec = sphere_go.AddComponent<AuxPermanentRootSphereSpec>();
            spec.EnRouteSpherePath = "~/enroute";
            spec.WindowsSpherePath = "~/windows";
            spec.Id = "doorways.table." + id;
            spec.ApplySpecToSphere(sphere.GetComponent<AuxTabletopSphere>());

            // Clean up
            // This script makes a second ~/tabletop
            // if anyone knows why it does this, please
            // create a GitHub issue.
            _span.Debug("Removing Duplicate spheres in root");
            HashSet<string> records = new HashSet<string>();
            List<Sphere> dupes = new List<Sphere>();
            foreach(Sphere s in FucineRoot.Get().Spheres)
            {
                // Remove only the last instance of ~/tabletop
                if(records.Contains(s.GetAbsolutePath().ToString()))
                {
                    dupes.Add(s);
                }
                else
                {
                    records.Add(s.GetAbsolutePath().ToString());
                }
            }

            foreach(Sphere dupe in dupes)
            {
                _span.Debug($"Removing duplicate sphere with path {dupe.GetAbsolutePath().ToString()}");
                FucineRoot.Get().DetachSphere(dupe);
            }

            _span.Debug("-----------");
            #endregion
            _span.Debug("Registering Table into Doorways");
            TablesManager.tables.Add(id, this);
        }

        public void ExecuteTestColor()
        {
            var img = this.tableGraphic.transform.Find("Leather").gameObject.GetComponent<Image>();
            var mat = new Material(Shader.Find("UI/Default"));
            mat.CopyPropertiesFromMaterial(img.material);
            mat.color = Color.red;
            img.material = mat;
        }
    }
}
