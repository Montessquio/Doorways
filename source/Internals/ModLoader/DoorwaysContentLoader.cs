using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doorways.Internals.ModLoader
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
            var _span = Logger.Instance.Span();
            IEnumerator<LoadedDataFile> iterator = LoadContentInner().GetEnumerator();

            // Manually unwrap the enumerator so we can use a try..catch
            // in an IEnumerable method.
            bool more = true;
            while(more)
            {
                try
                {
                    more = iterator.MoveNext();
                }
                catch(Exception e)
                {
                    _span.Error($"Failed loading content file: {e}");
                    continue;
                }

                if(more)
                {
                    LoadedDataFile ldf = IDCanonicalizer.CanonicalizeId(mod.ModPrefix, iterator.Current);


                    yield return ldf;
                }
            }
        }

        /// <summary>
        /// Any key in a supported object of the format
        /// "@originalKey": "scriptName.lua" will be run as
        /// a runtime dynamic value. Every time core would
        /// attempt to get that value for an instance of
        /// that item on the table, Doorways will instead
        /// run "scriptName.Lua::originalKey()". The returned
        /// data will be merged with "originalKey" in that
        /// item (if any) and returned to Core for that
        /// instance.
        /// <para/>
        /// For example, the following recipe fragment will
        /// run "myDynamicEffect.lua::effects()". The returned
        /// table, if valid, will be converted to JSON and
        /// merge-overwrite the existing effects field.
        /// <code>
        /// "effects": {
        ///     funds: 1
        /// },
        /// "@effects": "myDynamicEffect.lua"
        /// </code>
        /// </summary>
        private static LoadedDataFile InitializeDynamicValues(LoadedDataFile ldf)
        {
            throw new NotImplementedException();
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
