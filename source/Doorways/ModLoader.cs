using Hjson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SecretHistories.Constants.Modding;
using SecretHistories.UI;
using sh.monty.doorways.logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIWidgets.Extensions;

namespace sh.monty.doorways
{
    public class ModLoader
    {
        /// <summary>
        /// A set of flags defining which hjson
        /// compiler features to enable or disable.
        /// For more information, see this struct's
        /// constructor.
        /// </summary>
        public readonly struct CompileParams
        {
            public readonly bool ChainSteps;

            /// <summary>
            /// Create a new set of flags describing
            /// which hjson compiler features to
            /// enable or disable.
            /// </summary>
            /// <param name="chainSteps">Enable Simple Recipe Chaining</param>
            public CompileParams
            (
                bool chainSteps = true
            )
            {
                this.ChainSteps = chainSteps;
            }

            public static CompileParams Parse(JsonObject json)
            {
                // Default options setting. Also acts as shorthand to enable
                // all compiler features.
                bool d = json.Qb("all", defaultValue: false);
                return new CompileParams (
                    chainSteps: json.Qb("ChainSteps", d)
                );
            }
        }

        // Get a list of all mod directory roots - both steam mods and local mods.
        private static List<Mod> EnumerateModDirectories()
        {
            var _span = Logger.Span();
            _span.Info("Enumerating Mods");
            List<Mod> enabledMods = Watchman.Get<ModManager>().GetEnabledModsInLoadOrder().ToList();
            if (enabledMods.First().LoadedAssembly.FullName == "DoorwaysFramework")
            {
                _span.Warn("Doorways was not the first mod to be loaded! The mod will still work, but mods loaded before doorways may behave strangely!");
            }
            enabledMods.RemoveAll(m =>
            {
                try { return m.LoadedAssembly.FullName == "DoorwaysFramework"; }
                catch(Exception e)
                {
                    _span.Warn($"Exception enumerating mods: {e}");
                    return false;
                }
            });
            return enabledMods;
        }

        /// <summary>
        /// Scans the local mods directory for any mods with a `doorways.hjson`
        /// file at the mod root, processes the manifest, and patches/compiles the mod.
        /// </summary>
        public static void InitializeDoorwaysMods()
        {
            var _span = Logger.Span();

            var mods = EnumerateModDirectories();
            foreach (Mod mod in mods)
            {
                string synopsisPath = Path.Combine(mod.ModRootFolder, "synopsis.json");
                if (File.Exists(synopsisPath)) // Ignore non-mods
                {
                    Dictionary<string, JValue> manifest = JsonConvert.DeserializeObject<Dictionary<string, JValue>>(File.ReadAllText(synopsisPath));
                    // Consider only Doorways Mods
                    if(manifest != null && manifest.ContainsKey("doorways"))
                    {
                        InitializeDoorwaysMod(mod, manifest);
                    }
                }
                
            }
        }

        /// <summary>
        /// Initialize a single Doorways mod.
        /// This function will compile any hjson source found in
        /// <c>src/content</c>. 
        /// <para />
        /// If the key <c>"crucible"</c> is
        /// present in the manifest, it will also compile it as
        /// a crucible mod.
        /// </summary>
        private static void InitializeDoorwaysMod(Mod mod, Dictionary<string, JValue> manifest)
        {
            // stub throw new NotImplementedException();
        }

        /// <summary>
        /// Recurses through a mod's <c>[Path]/doorways/</c>
        /// directory and translates any <c>.hjson</c> files into
        /// <c>.json</c> files, optionally desugaring the data
        /// based on provided compiler options.
        /// <para />
        /// The compiled <c>.json</c> file tree will be copied
        /// to the mod's root directory, preserving the original
        /// file tree. This method will fail if the directory 
        /// <c>[Path]/doorways/doorways</c> exists, to prevent
        /// Doorways from overwriting your mod source folder.
        /// <para />
        /// In order to be recognized as a valid mod, the mod folder
        /// must still have a regular <c>synopsis.json</c>
        /// </summary>
        /// <param name="Path">The root path of the mod to compile.</param>
        public static void Compile(string modRoot, CompileParams? options = null)
        {

        }
    }

    public class ModParams
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public ModLoader.CompileParams CompilerParams { get; private set; }
        
        private ModParams(string id, string name, ModLoader.CompileParams options)
        {
            this.Id = id;
            this.Name = name;
            this.CompilerParams = options;
        }

        public static ModParams Parse(string modroot, string synopsisPath, string manifestPath)
        {
            var manifestJson = HjsonValue.Load(manifestPath).Qo();
            var synopsisJson = HjsonValue.Load(synopsisPath).Qo();

            string id = new DirectoryInfo(modroot).Name;
            string name = synopsisJson.Qs("name");

            return new ModParams(
                id,
                name,
                ModLoader.CompileParams.Parse(manifestJson.Qo("compiler"))
            );
        }
    }
}
