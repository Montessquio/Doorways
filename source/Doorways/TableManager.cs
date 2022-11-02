using SecretHistories.Spheres;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static sh.monty.doorways.MonoBehaviours.ExtraTables;

namespace sh.monty.doorways
{
    class TableManager
    {
        private static Dictionary<(int, int), Table> tables;

        static TableManager()
        {

        }

        /*
        public static Table Create()
        {
            var _span = Logger.Span();

            var master = GameObject.Find("MasterCanvas");
            var other = Instantiate(master);
            other.name = "OtherCanvas";
            var other_trans = other.GetComponent<Transform>();
            other_trans.position = new Vector3(6000.0f, 0.0f, 0.0f);
            _span.Info("Created new Table " + GameObject.Find("OtherCanvas").name + " at " + other_trans.position.ToString());

            CamOperatorPatch.bounds_master = master
                .transform.Find("CameraDragRect")
                .transform.Find("Content")
                .transform.Find("CameraNavBounds")
                .GetComponent<RectTransform>();
            CamOperatorPatch.bounds_master.gameObject.name = "NavBoundsMaster";
            CamOperatorPatch.bounds_other = other
                .transform.Find("CameraDragRect")
                .transform.Find("Content")
                .transform.Find("CameraNavBounds")
                .GetComponent<RectTransform>();
            CamOperatorPatch.bounds_other.gameObject.name = "NavBoundsOther";
            CamOperatorPatch.bounds_other.transform
                .parent
                .Find("TabletopBackground")
                .GetComponent<Image>().sprite = Sprite.Create(new Texture2D(2, 2), Rect.zero, Vector2.zero);

        }
        */
    }

    /// <summary>
    /// A script that instantiated on all Root Canvases,
    /// one per table. 
    /// </summary>
    class Table : MonoBehaviour
    {
        static readonly float X_TRANSLATION_FACTOR = 6000.0f;
        static readonly float Y_TRANSLATION_FACTOR = 4000.0f;

        public int table_index_x { get; private set; }
        public int table_index_y { get; private set; }

        GameObject root;
        GameObject bounds;
        GameObject tabletop;

        TabletopSphere sphere;
        WindowsSphere win_sphere;
        PermanentRootSphereSpec permanent_root_spec;

        public Table(int x, int y)
        {
            var master_table = GameObject.Find("MasterCanvas");
            root = Instantiate(master_table);

            bounds = root
                .transform.Find("CameraDragRect")
                .transform.Find("Content")
                .transform.Find("CameraNavBounds")
                .gameObject;

            tabletop = bounds.transform
                .parent
                .Find("TabletopBackground")
                .gameObject;

            DisconnectSpheres(root);
        }

        private static void DisconnectSpheres(GameObject root)
        {
            // Disconnect this table from the main table.
        }
    }
}
