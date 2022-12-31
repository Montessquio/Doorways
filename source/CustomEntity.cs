using SecretHistories.Fucine;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doorways
{
    public static class CustomEntity
    {
        public delegate IEntityWithId EntityConstructor(LoadedDataFile source, IDoorwaysMod forMod);

        private class TaggedEntityConstructor
        {
            public TaggedEntityConstructor(string sourceMod, EntityConstructor constructor)
            {
                SourceMod = sourceMod;
                Construct = constructor;
            }

            public string SourceMod { get; private set; }
            public EntityConstructor Construct { get; private set; }
        }

        private static Dictionary<string, TaggedEntityConstructor> Types = new Dictionary<string, TaggedEntityConstructor>();

        /// <summary>
        /// Informs Doorways that the caller would like to be the one in charge
        /// of converting static (JSON, etc) data into IEntityWithIDs. This is on
        /// a first come, first serve basis, so future callers will get an exception
        /// if they try to register a handler for the same entityTag.
        /// </summary>
        internal static void RegisterNewType(string forMod, string entityTag, EntityConstructor ctor)
        {
            if(Types.ContainsKey(entityTag))
            {
                throw new ArgumentException($"The entity tag '{entityTag}' has already been registered by the mod '{Types[entityTag].SourceMod}'");
            }
            Types.Add(entityTag, new TaggedEntityConstructor(forMod, ctor));
        }

        /// <summary>
        /// Checks the Doorways Custom Types dictionary for an appropriate loader to service the given loaded
        /// data file. If there is an appropriate loader, it will return the IEntityWithId that loader returns.
        /// </summary>
        internal static IEntityWithId Construct(this LoadedDataFile source, IDoorwaysMod forMod)
        {
            if(!Types.ContainsKey(source.EntityTag))
            {
                throw new KeyNotFoundException($"The Entity type '{source.EntityTag}' did not have an associated constructor!");
            }
            return Types[source.EntityTag].Construct(source, forMod);
        }

        internal static class DoorwaysTypeHandlers
        {
            internal static void Initialize()
            {
                RegisterNewType("Doorways", "achievements", Achievement);
            }

            [DFucineHandler("achievements")]
            internal static IEntityWithId Achievement(LoadedDataFile source, IDoorwaysMod forMod)
            {

            }

            [DFucineHandler("cultures")]
            internal static IEntityWithId Culture(LoadedDataFile source, IDoorwaysMod forMod)
            {

            }

            [DFucineHandler("deckspec")]
            [DFucineHandler("decks")]
            internal static IEntityWithId Deck(LoadedDataFile source, IDoorwaysMod forMod)
            {

            }

            [DFucineHandler("dicta")]
            internal static IEntityWithId Dictum(LoadedDataFile source, IDoorwaysMod forMod)
            {

            }

            [DFucineHandler("aspects")]
            [DFucineHandler("cards")]
            [DFucineHandler("elements")]
            internal static IEntityWithId Element(LoadedDataFile source, IDoorwaysMod forMod)
            {
                switch(source.EntityTag.ToLower())
                {
                    case "aspects":

                    case "cards":

                    case "elements":

                    default:
                        throw new InvalidOperationException($"Element handler was called for EntityTag {source.EntityTag}");
                }
            }

            [DFucineHandler("endings")]
            internal static IEntityWithId Ending(LoadedDataFile source, IDoorwaysMod forMod)
            {

            }

            [DFucineHandler("legacies")]
            internal static IEntityWithId Legacy(LoadedDataFile source, IDoorwaysMod forMod)
            {

            }

            [DFucineHandler("portals")]
            internal static IEntityWithId Portal(LoadedDataFile source, IDoorwaysMod forMod)
            {

            }

            [DFucineHandler("recipes")]
            internal static IEntityWithId Recipe(LoadedDataFile source, IDoorwaysMod forMod)
            {

            }

            [DFucineHandler("settings")]
            internal static IEntityWithId Setting(LoadedDataFile source, IDoorwaysMod forMod)
            {

            }

            [DFucineHandler("verbs")]
            internal static IEntityWithId Verb(LoadedDataFile source, IDoorwaysMod forMod)
            {

            }


        }
    }
}
