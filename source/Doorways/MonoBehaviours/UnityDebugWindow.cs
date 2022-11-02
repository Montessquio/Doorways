using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace sh.monty.doorways.MonoBehaviours
{
    class UnityDebugWindow : MonoBehaviour
    {
        public static UnityDebugWindow Instantiate()
        {
            GameObject masterCanvas = GameObject.Find("CanvasScalableUI");

            GameObject me = new GameObject("Doorways.Debug Container");

            RectTransform me_trans = me.AddComponent<RectTransform>();
            MoveToUISpace(me_trans);
            me_trans.sizeDelta = new Vector2(400.0f, 200.0f);
            me_trans.localPosition = new Vector2(0.0f, -20.0f);
            me_trans.anchorMax = new Vector2(0.5f, 1);
            me_trans.anchorMin = new Vector2(0.5f, 1);
            me_trans.pivot = new Vector2(0.5f, 1.0f);
            me_trans.localScale = new Vector2(1.0f, 1.0f);

            me.AddComponent<CanvasRenderer>();
            Image me_image = me.AddComponent<Image>();
            me_image.color = new Color(1.0f, 1.0f, 255f, 0.5f);

            // Background
            /*{
                GameObject bg = new GameObject("Doorways.Debug Container.Background");
                RectTransform bg_trans = bg.AddComponent<RectTransform>();
                MoveToUISpace(bg_trans, me_trans);
                bg_trans.localPosition = new Vector2(0.0f, 0.0f);
                bg_trans.anchorMax = new Vector2(0.5f, 1);
                bg_trans.anchorMin = new Vector2(0.5f, 1);
                bg_trans.pivot = new Vector2(0.5f, 1.0f);
                bg_trans.localScale = new Vector2(1.0f, 1.0f);

                bg.AddComponent<CanvasRenderer>();
                Sprite bg_sprite = ResourceLoader.LoadImage(Path.Combine("ui", "textbox.png"));
                Image bg_img = bg.gameObject.AddComponent<Image>();
                bg_img.preserveAspect = true;
                bg_img.sprite = bg_sprite;
            }*/

            // Debug Text Items
            {
                GameObject mt = new GameObject("Doorways.DebuggerText");

                RectTransform mt_trans = mt.AddComponent<RectTransform>();
                MoveToUISpace(mt_trans);
                mt_trans.localPosition = new Vector2(-90.0f, -30.0f);
                mt_trans.anchorMax = new Vector2(0.5f, 1);
                mt_trans.anchorMin = new Vector2(0.5f, 1);
                mt_trans.pivot = new Vector2(0.5f, 1.0f);

                mt.AddComponent<CanvasRenderer>();
                TextMeshProUGUI text = mt.AddComponent<TextMeshProUGUI>();
                text.text = "Initializing...";
                text.color = Color.black;
                text.fontSize = 12;

                mt.AddComponent<DebugTextManager>();
            }

            // Finally, attach a new 'this' to the game object.
            return me.AddComponent<UnityDebugWindow>();
        }

        private static void MoveToUISpace(Transform go)
        {
            GameObject masterCanvas = GameObject.Find("CanvasScalableUI");
            MoveToUISpace(go, masterCanvas.transform);
        }

        private static void MoveToUISpace(Transform go, Transform parent)
        {
            GameObject masterCanvas = GameObject.Find("CanvasScalableUI");
            go.SetParent(parent, false);
            go.gameObject.layer = 5;

            // Set its position
            var pos = go.position;
            // A little hacky, but we're using the pause button's Z-index as
            // a known-good value for Z, since the default Z-index will be
            // outside the game's camera space.
            pos.z = GameObject.Find("Button_Pause").transform.position.z;
            go.position = pos;
            go.rotation = masterCanvas.transform.rotation;

        }
    }
}
