using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Timeline;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace sh.monty.doorways.Unity
{
    /// <summary>
    /// Utilities for spawning new GameObjects
    /// into scenes.
    /// </summary>
    public class UnityObject
    {
        /// <summary>
        /// Runtime instantiation of prefabs from the disk in a variety
        /// of formats. Uses the ResourceLoader behind the scenes.
        /// </summary>
        public Prefab Load(string path)
        {
            return ResourceLoader.LoadPrefab(path);
        }
    }

    /// <summary>
    /// Represents either a UnityEngine internal prefab
    /// or an on-disk prefab that has been loaded by
    /// Doorways.
    /// </summary>
    public class Prefab
    {
        public string name { get; set; }

        public GameObject Instantiate()
        {
            return Instantiate(name);
        }

        public GameObject Instantiate(string name)
        {
            var go = new GameObject(name);
            // TODO: Apply prefab processing
            return go;
        }

        public GameObject Instantiate(Transform parent)
        {
            return Instantiate(this.name, parent);
        }

        public GameObject Instantiate(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            // TODO: Apply prefab processing
            return go;
        }
    }
}
