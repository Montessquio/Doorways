using sh.monty.doorways.Patches.SecretHistories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Logger = sh.monty.doorways.logging.Logger;

namespace sh.monty.doorways.UIExtensions
{
    /// <summary>
    /// Utilities to get and set the current splash screen text.
    /// 
    /// This class becomes effectively useless after the game
    /// has started, but it can be used by other DLL mods to
    /// overwrite the default starting text.
    /// </summary>
    public static class GameSplash
    {
        public static string Quote;
        public static string Advice;

        public static void Initialize()
        {
            CoreEvents.onQuoteSceneInit += new CoreEvents.DEventHandler(() =>
            {
                var _span = Logger.Span("SplashOverride");
                Transform parent = GameObject
                    .Find("Canvas").transform
                    .Find("Text").transform;

                if (Quote != null)
                {
                    _span.Info($"Setting splash quote to \"{Quote}\"");
                    TextMeshProUGUI t = parent.Find("Quote").GetComponent<TextMeshProUGUI>();
                    t.text = Quote;
                }

                if (Advice != null)
                {
                    _span.Info($"Setting splash advice to \"{Advice}\"");
                    TextMeshProUGUI t = parent.Find("Advice").GetComponent<TextMeshProUGUI>();
                    t.text = Advice;
                }
            });
        }
    }
}
