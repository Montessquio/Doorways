using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

using Logger = sh.monty.doorways.logging.Logger;

namespace sh.monty.doorways
{
    class ResourceLoader
    {
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static string ModDirectory
        {
            get
            {
                return Directory.GetParent(AssemblyDirectory).FullName;
            }
        }
        public static string AssetDirectory
        {
            get
            {
                return Path.Combine(ModDirectory, "images");
            }
        }

        public static string LogPath
        {
            get
            {
                return Path.Combine(AssemblyDirectory, "doorways.log");
            }
        }

        /// <summary>
        /// Load an image with a path relative to the mod's `images` directory.
        /// <para />
        /// Assign this to a UI Image with
        /// <c>
        /// Sprite s = ResourceLoader.LoadImage("path/to.png");
        /// myGameObject.GetComponent<Image>().sprite = s;
        /// </c>
        /// </summary>
        public static Sprite LoadImage(string imagePath)
        {
            Logger.Info("Loading dynamic image from path: " + Path.Combine(AssetDirectory, imagePath));
            byte[] bytes = File.ReadAllBytes(Path.Combine(AssetDirectory, imagePath));

            // Texture dimensions will be overridden on load
            var tex = new Texture2D(1, 1);
            tex.LoadImage(bytes);
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2, tex.height / 2));
        }
    }
}
