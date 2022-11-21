using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doorways.Internals
{
    /// <summary>
    /// A helper class used by <see cref="DoorwaysMod"/>.
    /// It should only be used in that class, but is extracted
    /// to this file because the semantics for canonicalization
    /// could grow over time.
    /// </summary>
    internal static class IDCanonicalizer
    {
        public static string CanonicalizeId(string prefix, string item)
        {
            var _span = Logger.Instance.Span();

            if (item == null) { return null; }

            // If the entity id starts with a dot, we need to
            // remove the dot and allow the remaining ID through as-is.
            if (item.StartsWith("."))
            {
                item = item.Substring(1);
            }
            // If it's not designated as a literal ID,
            // we need to prepend its mod's prefix.
            else
            {
                item = prefix + "." + item;
            }

            // Lowercase the whole thing because
            // the core engine expects all IDs to be lowercase.
            return item.ToLower();
        }

        // TODO: Make this also delve into individual subfields and canonicalize those IDs.
        public static LoadedDataFile CanonicalizeId(string prefix, LoadedDataFile item)
        {
            foreach (JObject member in ((JArray)item.EntityContainer.Value))
            {
                JProperty id = member.Property("id");
                if (id != null)
                {
                    id.Value = CanonicalizeId(prefix, id.Value.ToString());
                }
            }

            return item;
        }
    }
}
