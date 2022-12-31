using Hjson;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doorways.Internals.ModLoader
{
    internal class HJSONLoader : DoorwaysContentLoader
    {
        public HJSONLoader(DoorwaysMod mod, string contentFolder) : base(mod, contentFolder)
        {
        }

        protected override IEnumerable<LoadedDataFile> LoadContentInner()
        {
            var _span = Logger.Instance.Span();

            var contentFilePaths = GetContentFilesRecursive(contentFolder, ".hjson");
            if (contentFilePaths.Any())
            {
                contentFilePaths.Sort();
            }

            foreach (string contentFilePath in contentFilePaths)
            {
                if (new FileInfo(contentFilePath).Length < 8)
                {
                    continue;
                }

                LoadedDataFile item = null;
                try
                {
                    // Load HJson from the file, and then convert it to JSON with ToString();
                    using (StreamReader reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(HjsonValue.Load(contentFilePath).ToString()))))
                    {
                        using (JsonTextReader reader2 = new JsonTextReader(reader))
                        {
                            JProperty jProperty = ((JObject)JToken.ReadFrom(reader2)).Properties().First();
                            item = new LoadedDataFile(contentFilePath, jProperty, jProperty.Name);
                            _span.Debug($"Loaded '{contentFilePath}' as '{item.EntityTag}' with {((JArray)item.EntityContainer.Value).Count} members");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _span.Error("Problem parsing HJSON file at " + contentFilePath + ": " + ex.Message);
                }

                if (item == null)
                {
                    continue;
                }
                yield return item;
            }
        }
    }
}
