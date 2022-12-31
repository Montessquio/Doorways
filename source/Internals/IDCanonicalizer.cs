using Newtonsoft.Json.Linq;
using SecretHistories.Entities;
using System.Collections.Generic;

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
            if (item == "") { return ""; }

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
                foreach (string key in MembersToCanonicalize)
                {
                    member.TryCanonicalizeMember(prefix, key);
                }
            }

            return item;
        }

        public static void RenameKey(this JObject dic, string fromKey, string toKey)
        {
            JToken value = dic[fromKey];
            dic.Remove(fromKey);
            dic[toKey] = value;
        }

        private static void TryCanonicalizeMember(this JObject obj, string prefix, string key)
        {
            if(obj.ContainsKey(key))
            {
                switch(obj[key].Type)
                {
                    case JTokenType.String:
                        obj[key] = CanonicalizeId(prefix, (string)obj[key]);
                        break;
                    case JTokenType.Array:
                        JArray arr = (JArray)obj[key];
                        for (int i = 0; i < arr.Count; i++)
                        {
                            if (arr[i].Type == JTokenType.String)
                            {
                                arr[i] = CanonicalizeId(prefix, (string)arr[i]);
                            }
                        }
                        break;
                    case JTokenType.Object:
                        JObject map = (JObject)obj[key];
                        List<string> keys = new List<string>();
                        foreach (KeyValuePair<string, JToken> kv in map)
                        {
                            keys.Add(kv.Key);
                        }

                        foreach (string k in keys)
                        {
                            int _;
                            // Only canonicalize value's content if it's a string and not an int.
                            // We don't want to canonicalize numeric string values like `effects: { heart: 1 }
                            if (map[k].Type == JTokenType.String && !int.TryParse((string)map[k], out _))
                            {
                                map[k] = CanonicalizeId(prefix, (string)map[k]);
                            }
                            map.RenameKey(k, CanonicalizeId(prefix, k));
                        }
                        break;
                }
            }
        }

        private static readonly string[] MembersToCanonicalize = new string[]
        {
            // Global
            "id",
            
            // elements
            "decayTo",
            "burnTo",
            "drownTo",
            "uniquenessgroup",
            "achievements",
            "inherits",
            "aspects",
            "commute",
            // "slots" - needs special handling
            // "induces" - needs special handling
            // "xtriggers" - needs special handling

            // decks
            "defaultcard",
            "spec",

            // endings
            "achievementid",

            // legacies
            "startingverbid",
            "statusbarelements",
            "effects",
            "excludesonending",
            "fromending",

            // recipes
            "actionId",
            "requirements",
            "nearbyreqs",
            "tablereqs",
            "extantreqs",
            "seeking",
            // aspects - already specified for "elements"
            "mutations",
            "purge",
            "haltverb",
            "deleteverb",
            // achievements - already specified for "elements"
            "deckeffects",
            // "alt" - needs special handling
            // "lateAlt" - needs special handling
            // "inductions" - needs special handling
            // "linked" - needs special handling
            "ending",
            // slots - already specified for "elements"
            // "internaldeck" - needs special handling

            // verb
            // slot - needs special handling
        };
    }
}
