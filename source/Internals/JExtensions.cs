using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doorways.Internals
{
    /// <summary>
    /// JSON Object extensions
    /// to make other code cleaner.
    /// </summary>
    internal static class JExtensions
    {
        /// <summary>
        /// Returns <c>true</c> if and only if
        /// the JObject has the given key that is also
        /// the given type. Returns false otherwise.
        /// </summary>
        public static bool ContainsKey(this JObject j, string key, JTokenType t)
        {
            if(j != null && j.ContainsKey(key) && j[key].Type == t)
            {
                return true;
            }
            return false;
        }
    }
}
