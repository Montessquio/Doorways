using HarmonyLib;
using SecretHistories.UI;
using sh.monty.doorways.logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using Logger = sh.monty.doorways.logging.Logger;

namespace sh.monty.doorways.MonoBehaviours
{
    class ExtraTables : MonoBehaviour
    {
        /*
        [HarmonyPatch(typeof(CamOperator))]
        public class CamOperatorPatch
        {
            public static GameObject instance;
            public static RectTransform bounds_master;
            public static RectTransform bounds_other;
            public static bool is_master = true;
            private static Span _span = Logger.Span("CamOperator.Update");

            private static int count = 0;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CamOperator), nameof(CamOperator.Update))]
            static void Update(ref RectTransform ___navigationLimits, ref Vector3 ___smoothTargetPosition, ref Vector3 ____storedCameraPosition)
            {
                //count += 1;
                if (count > 300)
                {
                    count = 0;
                    if (is_master)
                    {   
                        is_master = false;
                        ___navigationLimits = bounds_other;
                    }
                    else
                    {
                        is_master = true;
                        ___navigationLimits = bounds_master;
                    }
                    _span.Info("Setting view to '{0}': {1}", ___navigationLimits.gameObject.name, ___navigationLimits.transform.position);
                    CamOperator cammy = instance.GetComponent<CamOperator>();

                    var nav_pos = ___navigationLimits.transform.position; 
                    var new_camera_pos = new Vector3(nav_pos.x, nav_pos.y, instance.GetComponent<Transform>().position.z);
                    ___smoothTargetPosition = new_camera_pos;
                    ____storedCameraPosition = new_camera_pos;
                    SetCameraPosition(cammy, new_camera_pos);
                    _span.Info("Set Camera Position to (" + new_camera_pos.x + ", " + new_camera_pos.y + ")");
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CamOperator), nameof(CamOperator.Awake))]
            static void Awake()
            {
                _span.Info("CamOperator Hook has awoken!");
            }

            [HarmonyReversePatch]
            [HarmonyPatch(typeof(CamOperator), "SetCameraPosition")]
            static void SetCameraPosition(CamOperator instance, Vector3 position)
            {
                throw new NotImplementedException("Stub");
            }
        }

        public void Start()
        {
            var _span = Logger.Span();
            _span.Info("Doorways ExtraTables experiment runner started!");

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
            
            CamOperatorPatch.instance = GameObject.Find("Main Camera");
        }

        private Span _span = Logger.Span("Update");
        public void Update()
        {
            
        }

        T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy as T;
        }
        
        */
    }
}
