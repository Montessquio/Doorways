using SecretHistories.Entities;
using SecretHistories.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.IO.Compression;
using UnityEngine.Windows;

namespace Doorways.Internals
{
    /// <summary>
    /// Manipulates the UH O! Scene that comes
    /// up when the game encounters a fatal error.
    /// If doorways is loaded, it should say so
    /// on this screen and direct the user to create
    /// a doorways issue in addition to the existing
    /// directions.
    /// </summary>
    internal class UhOScene : MonoBehaviour
    {
        private static StackTrace lastStackTrace = null;
        public static void OnUhOSceneInit(ref StackTrace st)
        {
            var _span = Logger.Instance.Span();
            _span.Info("Detected core engine crash.");

            lastStackTrace = st;
            GameObject go = new GameObject("Doorways Crash Handler", new Type[] { typeof(UhOScene) });
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if(scene.name == "S7UhO")
            {
                ModifyScene();
                CreateCrashDump();
            }
        }
        private void ModifyScene()
        {
            GameObject textBox = GameObject.Find("CanvasContent")
                .transform.Find("TextBox").gameObject;
            RectTransform tb_rt = textBox.GetComponent<RectTransform>();
            tb_rt.sizeDelta = new Vector2(tb_rt.sizeDelta.x, tb_rt.sizeDelta.y + 180.0f);

            TextMeshProUGUI title = textBox
                .transform.Find("Title").GetComponent<TextMeshProUGUI>();
            //title.text += "<space=5em><size=30%>";

            TextMeshProUGUI description = textBox
                .transform.Find("Description").GetComponent<TextMeshProUGUI>();
            description.text = @"
<b>Something horrible has happened: a wakeful Worm, a loop-bound choice, a mangled Name, a wind from Nowhere.</b>

<b>It looks like you're running <color=""red"">Doorways</color>.</b>
Please don't send Doorways crash reports to Weather Factory. Instead, please send a bug report to me@monty.sh.

If you genuinely believe this crash is unrelated to Doorways, please disable it and try again.

You can find your log files by clicking the button below -  ""player.log"", ""save.json"", and ""doorways.ddf"" are the important ones.

Doorways has created an additional diagnostic file in the logs folder named . Please include it in any bug reports.

<i>(""It is a capital mistake to theorise before one has data.""  - Conan Doyle, 'A Scandal in Bohemia')</i>";
        }

        [JsonObject(MemberSerialization.Fields)]
        private class DoorwaysDumpFormat
        {
            public string[] UhOCallingStackTrace;
        }

        private void CreateCrashDump()
        {
            string path = Path.Combine(Watchman.Get<MetaInfo>().PersistentDataPath, "doorways.ddf");
            DoorwaysDumpFormat ddf = new DoorwaysDumpFormat
            {
                UhOCallingStackTrace = lastStackTrace.GetFrames().Select(x => x.ToString()).ToArray(),
            };
            byte[] bin = Compress(Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(ddf)
            ));
            using (FileStream fd = File.Create(path))
            {
                fd.Write(bin.ToArray(), 0, (int)bin.Length);
            }
        }

        private static byte[] Compress(byte[] input)
        {
            using (var result = new MemoryStream())
            {
                var lengthBytes = BitConverter.GetBytes(input.Length);
                result.Write(lengthBytes, 0, 4);

                using (var compressionStream = new GZipStream(result, CompressionMode.Compress))
                {
                    compressionStream.Write(input, 0, input.Length);
                    compressionStream.Flush();

                }
                return result.ToArray();
            }
        }
    }
}
