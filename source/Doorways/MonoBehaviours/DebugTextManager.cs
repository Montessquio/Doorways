using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;

namespace sh.monty.doorways.MonoBehaviours
{
    class DebugTextManager : MonoBehaviour
    {
        TextMeshProUGUI text;
        StringBuilder sb = new StringBuilder();
        static string lastMouseEnterName = "None";

        public void Start()
        {
            this.text = GetComponent<TextMeshProUGUI>();

            AddTracersToAllObjects();
        }

        public static readonly List<String> TracerBlacklist = new List<String> { 
            "MasterCanvas",
        };

        private void AddTracersToAllObjects()
        {
            UnityEngine.Object[] gos = FindObjectsOfType(typeof(GameObject));
            foreach (UnityEngine.Object go in gos)
            {
                GameObject g = go as GameObject;
                if (
                    (g.GetComponent<DebugPointerSubscriber>() == null) &&
                    (!TracerBlacklist.Contains(g.name.Trim()))
                )
                {
                    g.AddComponent<DebugPointerSubscriber>();
                }
            }
        }

        public void Update()
        {
            AddTracersToAllObjects();

            (float mouse_x, float mouse_y) = GetMousePosition();

            sb.Append("MousePos: ("); sb.Append(Math.Round(mouse_x, 4).ToString()); sb.Append(", "); sb.Append(Math.Round(mouse_y, 4).ToString()); sb.AppendLine(")");
            sb.Append("Last MouseTrace: "); sb.AppendLine(lastMouseEnterName.Trim());
            text.text = sb.ToString();
            sb.Clear();
        }

        private (float, float) GetMousePosition()
        {
            Vector2Control pos = Mouse.current.position;
            return (pos.x.ReadValue(), pos.y.ReadValue());
        }

        class DebugPointerSubscriber : MonoBehaviour, IPointerEnterHandler
        {
            public void OnPointerEnter(PointerEventData eventData)
            {
                if(!DebugTextManager.TracerBlacklist.Contains(gameObject.name.Trim())) {
                    lastMouseEnterName = gameObject.name;
                }
            }
        }
    }
}
