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
using UIWidgets.Extensions;
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

        // Holds the raw data from every plugin, as well as all their static defs.
        internal Dictionary<string, DoorwaysPlugin> plugins { get; private set; } = new Dictionary<string, DoorwaysPlugin>();

        // This registry holds top-level defs, i.e. raw JSON defs not produced by any plugin.
        internal Dictionary<string, IEntityWithId> registry = new Dictionary<string, IEntityWithId>();

        public void RegisterEntity(IEntityWithId entity)
        {
            var _span = Logger.Instance.Span();

            if (registry.ContainsKey(entity.Id))
            {
                throw new ArgumentException($"The mod {ModName} has already registered an entity with ID {entity.Id}");
            }
            registry.Add(entity.Id, entity);
            _span.Debug($"Loaded Entity '{entity.Id}'/'{entity.Id}' for mod '{ModName}' as '{entity.GetUnderlyingElement().Name}'");
        }

        public void RegisterPlugin(Assembly pluginAssembly)
        {
            var _span = Logger.Instance.Span();

            DoorwaysPlugin p = new DoorwaysPlugin(pluginAssembly);
            if (plugins.ContainsKey(p.Prefix))
            {
                throw new ArgumentException($"The mod {ModName} has already registered a plugin with ID {p.Prefix}");
            }
            var (otherMod, otherPlugin) = ModLoader.IsPluginPrefixTaken(p.Prefix);
            if(otherMod != null)
            {
                throw new ArgumentException($"The mod {otherMod} has already registered a plugin with ID {otherPlugin}");
            }

            plugins.Add(p.Prefix, p);
            _span.Debug($"Loaded Plugin '{p.Name}' for mod '{ModName}' as '{p.Prefix}'");
        }

        public void RegisterPlugin(DoorwaysPlugin p)
        {
            var _span = Logger.Instance.Span();

            if (plugins.ContainsKey(p.Prefix))
            {
                throw new ArgumentException($"The mod {ModName} has already registered a plugin with ID {p.Prefix}");
            }
            var (otherMod, otherPlugin) = ModLoader.IsPluginPrefixTaken(p.Prefix);
            if (otherMod != null)
            {
                throw new ArgumentException($"The mod {otherMod} has already registered a plugin with ID {otherPlugin}");
            }

            plugins.Add(p.Prefix, p);
            _span.Debug($"Loaded Plugin '{p.Name}' for mod '{ModName}' as '{p.Prefix}'");
        }

        public DoorwaysMod()
        {
            Assembly mod = Assembly.GetCallingAssembly();
            ModName = mod.FullName;

            if (ModLoader.Mods.ContainsKey(ModName))
            {
                DoorwaysMod other = ModLoader.Mods[ModName];
                throw new ArgumentException($"Could not register {ModName}: the mod name has already been claimed.");
            }
            ModLoader.Mods.Add(ModName, this);
        }

        public DoorwaysMod(string modName)
        {
            ModName = modName;

            if (ModLoader.Mods.ContainsKey(ModName))
            {
                DoorwaysMod other = ModLoader.Mods[ModName];
                throw new ArgumentException($"Could not register {ModName}: the mod name has already been claimed.");
            }
            ModLoader.Mods.Add(ModName, this);
        }

        public IDoorwaysMod GetInitializerMetadata(DoorwaysPlugin forPlugin)
        {
            // TODO
            return null;
        }
    }

    public class DoorwaysPlugin
    {
        internal Assembly PluginAssembly { get; set; }

        public string Name
        {
            get
            {
                return PluginAssembly.GetCustomAttribute<DoorwaysAttribute>().Name;
            }
        }

        public string Prefix
        {
            get
            {
                return PluginAssembly.GetCustomAttribute<DoorwaysAttribute>().Prefix;
            }
        }

        // This registry holds pre-canonicalized defs produced by this plugin.
        internal Dictionary<string, IEntityWithId> registry = new Dictionary<string, IEntityWithId>();

        public DoorwaysPlugin(Assembly plugin)
        {
            PluginAssembly = plugin;
            LoadPlugin();
        }

        public string CanonicalizeId(string rawid)
        {
            var _span = Logger.Instance.Span();
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
                rawid = Prefix + "." + rawid;
            }

            // Lowercase the whole thing because
            // the core engine expects all IDs to be lowercase.
            return rawid.ToLower();
        }

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
            var _span = Logger.Instance.Span();

            if(entity.Id == null || entity.Id == "id")
            {
                entity.SetId(entity.GetActualType().Name.ToLower());
            }

            entity.SetId(CanonicalizeId(entity.Id));
            if (registry.ContainsKey(entity.Id))
            {
                throw new ArgumentException($"The plugin {Name} has already registered an entity with ID {entity.Id}");
            }
            registry.Add(entity.Id, entity);
            _span.Debug($"Loaded Entity '{entity.Id}'/'{entity.Id}' for mod '{Name}' as '{entity.GetUnderlyingElement().Name}'");
        }


        private void LoadPlugin()
        {
            var _span = Logger.Instance.Span();

            var doorwaysModAttr = PluginAssembly.GetCustomAttribute<DoorwaysAttribute>();

            string PluginName = doorwaysModAttr.Name ?? PluginAssembly.GetName().Name;
            string PluginPrefix = doorwaysModAttr.Prefix ?? PluginName.ToLower();

            // Instantiating this class automatically adds the instance
            // to the doorways mod registry.

            // Get all [DoorwaysObject]s in the assembly
            foreach (Type t in PluginAssembly.GetTypes())
            {
                if (t.IsClass)
                {
                    // Determine if this is a valid entity we can register.
                    _span.Debug($"Inspecting type '{t.Name}'");
                    bool hasDoorwaysAttr = t.GetCustomAttribute<DoorwaysObjectAttribute>() != null;
                    if (hasDoorwaysAttr)
                    {
                        bool isRegisterable = typeof(IEntityWithId).IsAssignableFrom(t);
                        bool isFactory = typeof(IDoorwaysFactory).IsAssignableFrom(t);
                        _span.Debug($"  {t.Name}: (Registerable: {(isRegisterable ? "true" : "false")}, Factory: {(isFactory ? "true" : "false")})");

                        if (isRegisterable)
                        {
                            RegisterEntity(ModLoader.InstantiateStaticEntity(t));
                        }
                        else if (isFactory)
                        {
                            IDoorwaysFactory factory = Activator.CreateInstance(t) as IDoorwaysFactory;
                            factory.GetAll().ForEach((entity) => RegisterEntity(entity));
                        }
                        else
                        {
                            _span.Error($"Marked entity '{t.FullName}' does not derive from IEntityWithId or DoorwaysFactory and/or is not marked with the [DoorwaysObject] attribute, so it cannot be registered as game data.");
                        }
                    }
                }
            }
        }
    }
}
