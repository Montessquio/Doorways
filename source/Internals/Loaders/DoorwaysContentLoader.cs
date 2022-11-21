using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doorways.Internals.Loaders
{
    /// <summary>
    /// Provides post-load finalizers that get applied
    /// to the results of all Content Loaders after they
    /// are translated to common JSON.
    /// </summary>
    internal abstract class DoorwaysContentLoader
    {
        protected DoorwaysMod mod;
        protected string contentFolder;

        protected abstract IEnumerable<LoadedDataFile> LoadContentInner();

        public IEnumerable<LoadedDataFile> LoadContent()
        {
            foreach(LoadedDataFile file in LoadContentInner())
            {
                // Currently just canonicalizes all ids.
                // TODO: Go into each fucine type and canonicalize subids
                // TODO: Resolve Doorways types - instead of yielding them,
                // just add them to the mod registry.
                yield return IDCanonicalizer.CanonicalizeId(mod.ModPrefix, file);
            }
        }

        protected static List<string> GetContentFilesRecursive(string path, string extension)
        {
            List<string> list = new List<string>();
            if (Directory.Exists(path))
            {
                list.AddRange(Directory.GetFiles(path).ToList().FindAll((string f) => f.EndsWith(extension)));
                string[] directories = Directory.GetDirectories(path);
                foreach (string path2 in directories)
                {
                    list.AddRange(GetContentFilesRecursive(path2, extension));
                }
            }

            return list;
        }

        public DoorwaysContentLoader(DoorwaysMod mod, string contentFolder)
        {
            this.mod = mod;
            this.contentFolder = contentFolder;
        }
    }
}
