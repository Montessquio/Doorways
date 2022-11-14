using Doorways.Entities;
using Doorways.Entities.Mixins;
using Doorways.Internals.Patches;
using HarmonyLib;
using Microsoft.Win32;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UniverseLib;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static UnityEngine.EventSystems.EventTrigger;

namespace Doorways
{
    /// <summary>
    /// 
    /// </summary>
    public class DoorwaysMod
    {
        public string ModName { get; private set; }
        public string ModId { get; private set; }

        internal Dictionary<string, IEntityWithId> registry = new Dictionary<string, IEntityWithId>();

        /// <summary>
        /// Registers entity instances with
        /// the game's engine. This can only
        /// be done when the game is loading,
        /// and will throw exceptions if it
        /// is modified after the game is
        /// done loading.
        /// <para/>
        /// Note that while this method accepts any
        /// <see cref="IEntityWithId"/> for flexibility,
        /// you're much better off instantiating or
        /// extending one of the classes in the
        /// <see cref="Doorways.Entities"/> namespace.
        /// </summary>
        public void RegisterEntity(IEntityWithId entity)
        {
            var _span = Logger.Span();

            if (typeof(INamespacedIDEntity).IsAssignableFrom(entity.GetActualType()))
            {
                (entity as INamespacedIDEntity).CanonicalizeIds(CanonicalizeId, ModId);
                _span.Debug($"Canonicalized IDs for {entity.Id}");
            }

            if (typeof(IForcedSuperclass<>).IsAssignableFrom(entity.GetActualType()))
            {
                AccessTools.Method(typeof(ForcedSuperclassMixin), nameof(ForcedSuperclassMixin.ApplyPropertiesToBaseClass))
                    .MakeGenericMethod(new Type[] { entity.GetActualType().GetInterface("IForcedSuperclass").GetGenericArguments()[0] })
                    .Invoke(null, new object[] { entity });
            }

            if (registry.ContainsKey(entity.Id))
            {
                throw new ArgumentException($"The mod {ModName} ({ModId}) has already registered an entity with ID {entity.Id}");
            }
            registry.Add(entity.Id, entity);
            _span.Debug($"Loaded Entity '{entity.Id}'/'{entity.Id}' for mod '{ModName}' as '{entity.GetUnderlyingElement().Name}'");
        }

        public string CanonicalizeId(string rawid)
        {
            var _span = Logger.Span();
            // If the entity id starts with a dot, we need to
            // remove the dot and allow the remaining ID through as-is.
            if (rawid.StartsWith("."))
            {
                rawid = rawid.Substring(1);
            }
            // If it's not designated as a literal ID,
            // we need to prepend its mod's prefix.
            else
            {
                rawid = ModId + "." + rawid;
            }

            // Lowercase the whole thing because
            // the core engine expects all IDs to be lowercase.
            return rawid.ToLower();
        }

        public DoorwaysMod()
        {
            Assembly mod = Assembly.GetCallingAssembly();
            ModName = mod.FullName;
            ModId = mod.FullName.ToLower();

            if (ModLoader.Mods.ContainsKey(ModId))
            {
                DoorwaysMod other = ModLoader.Mods[ModId];
                throw new ArgumentException($"Could not register {ModName}: the mod {other.ModName} has already claimed ID {ModId}");
            }
            ModLoader.Mods.Add(ModId, this);
        }

        public DoorwaysMod(string modPrefix)
        {
            Assembly mod = Assembly.GetCallingAssembly();
            ModName = mod.FullName;
            ModId = modPrefix.ToLower();

            if (ModLoader.Mods.ContainsKey(ModId))
            {
                DoorwaysMod other = ModLoader.Mods[ModId];
                throw new ArgumentException($"Could not register {ModName}: the mod {other.ModName} has already claimed ID {ModId}");
            }
            ModLoader.Mods.Add(ModId, this);
        }

        public DoorwaysMod(string modName, string modPrefix)
        {
            ModName = modName;
            ModId = modPrefix;
            
            if(ModLoader.Mods.ContainsKey(ModId))
            {
                DoorwaysMod other = ModLoader.Mods[ModId];
                throw new ArgumentException($"Could not register {ModName}: the mod {other.ModName} has already claimed ID {ModId}");
            }
            ModLoader.Mods.Add(ModId, this);
        }
    }
}
