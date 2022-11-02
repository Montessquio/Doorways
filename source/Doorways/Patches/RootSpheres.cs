using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.UI;
using sh.monty.doorways.logging;

namespace sh.monty.doorways.HarmonyPatchLayers
{
    /// <summary>
    /// HEAVY WIP
    /// </summary>
    internal class RootSphereContainer
    {
        /*
        public readonly int RegistryID;
        public FucineRoot sphereContainer { get; private set; }
        public Dictionary<Type, object> watchmanRegistry { get; private set; }

        public RootSphereContainer(int registryID, FucineRoot sphereContainer, Dictionary<Type, object> watchmanRegistry)
        {
            RegistryID = registryID;
            this.sphereContainer = sphereContainer ?? throw new ArgumentNullException(nameof(sphereContainer));
            this.watchmanRegistry = watchmanRegistry ?? throw new ArgumentNullException(nameof(watchmanRegistry));
        }

        public RootSphereContainer(int registryID)
        {
            RegistryID = registryID;
            sphereContainer = new FucineRoot();
            watchmanRegistry = new Dictionary<Type, object>();
        }
    }

    /// <summary>
    /// Intercepts the FucineRoot singleton in order to
    /// allow library consumers to modify the root sphere.
    /// <para />
    /// This is a dangerous operation, and as such consumers
    /// must take great care not to contend for this state.
    /// </summary>
    [HarmonyPatch]
    public class RootSpheres
    {
        // State

        /// <summary>
        /// A list of all root states we're taking care of.
        /// </summary>
        private static Dictionary<int, RootSphereContainer> RootsRegistry = new Dictionary<int, RootSphereContainer>() {
            { 0, new RootSphereContainer(0) }
        };

        /// <summary>
        /// The current root this system is tracking.
        /// Index 0 is always the vanilla game root.
        /// </summary>
        private static int CurrentRoot = 0;
        // This is an atomically incremented count
        // we use to generate "unique" IDs for newly
        // created root sphere containers.
        // Technically this is inefficient, since we're
        // not using half the possible values, but
        // I think anyone creating more than 2,147,483,646
        // new root spheres needs help and possibly a priest
        // instead of more slots.
        private static int NextFreeRootIndex = 0;

        /// <summary>
        /// Slot zero always corresponds to the vanilla root element.
        /// It is the default element and is always present when the
        /// game starts.
        /// </summary>
        public const int VANILLA_ROOT = 0;

        // Public API

        /// <summary>
        /// Create a new root instance and return its
        /// internal ID and the instance itself.
        /// </summary>
        public static RootSphereContainer Create()
        {
            NextFreeRootIndex += 1;
            RootSphereContainer n = new RootSphereContainer(NextFreeRootIndex);
            RootsRegistry.Add(NextFreeRootIndex, n);
            return n;
        }

        /// <summary>
        /// Sets the current global root sphere container to
        /// a specific value.
        /// </summary>
        public static void Set(int RootElementID)
        {
            var _span = Logger.Span();
            if (!RootsRegistry.ContainsKey(RootElementID))
            {
                throw new IndexOutOfRangeException(String.Format("Requested ID '{0}' did not exist in registry", RootElementID));
            }
            _span.Info("Setting active root sphere container to ID {0}", RootElementID);
            CurrentRoot = RootElementID;
            _span.Debug("Set active Fucine Root element");

            SetWatchmanInstance(RootsRegistry[CurrentRoot].watchmanRegistry);
            _span.Debug("Set Watchman registry");
        }

        /// <summary>
        /// Returns the currently active sphere root. 
        /// </summary>
        public static RootSphereContainer Get()
        {
            var _span = Logger.Span();
            if (!RootsRegistry.ContainsKey(CurrentRoot))
            {
                throw new IndexOutOfRangeException();
            }
            _span.Debug("Fetching root sphere container with ID {0}", CurrentRoot);
            return RootsRegistry[CurrentRoot];
        }

        /// <summary>
        /// Returns the sphere root with the associated ID.
        /// </summary>
        public static RootSphereContainer Get(int RootElementID)
        {
            var _span = Logger.Span();
            if (!RootsRegistry.ContainsKey(RootElementID))
            {
                throw new IndexOutOfRangeException();
            }
            _span.Debug("Fetching root sphere container with ID {0}", CurrentRoot);
            return RootsRegistry[CurrentRoot];
        }

        /// <summary>
        /// Reset the currently loaded root sphere container.
        /// This will cause it to completely empty and reset
        /// state - as if a new game had been loaded.
        /// The old root container is returned.
        /// </summary>
        public static RootSphereContainer Reset()
        {
            var _span = Logger.Span();
            _span.Debug("Resetting root container with ID {0}", CurrentRoot);
            var old = RootsRegistry[CurrentRoot];
            RootsRegistry[CurrentRoot] = new RootSphereContainer(old.RegistryID);
            return old;
        }

        /// <summary>
        /// Reset the specified root sphere container.
        /// This will cause it to completely empty and reset
        /// state - as if a new game had been loaded.
        /// </summary>
        public static RootSphereContainer Reset(int RootElementID)
        {
            var _span = Logger.Span();
            if (!RootsRegistry.ContainsKey(RootElementID))
            {
                throw new IndexOutOfRangeException();
            }
            _span.Info("Resetting root container with ID {0}", RootElementID);
            var old = RootsRegistry[CurrentRoot];
            RootsRegistry[CurrentRoot] = new RootSphereContainer(old.RegistryID);
            return old;
        }

        /// <summary>
        /// Remove the current FucineRoot from the
        /// registry. The vanilla FucineRoot will be
        /// loaded in its place.
        /// </summary>
        public static RootSphereContainer Remove()
        {
            var _span = Logger.Span();
            if (CurrentRoot == 0)
            {
                throw new ArgumentException("Cannot remove vanilla root container");
            }
            _span.Info("Removing root container with ID {0}", CurrentRoot);
            var removed = Get();
            RootsRegistry.Remove(CurrentRoot);
            CurrentRoot = 0;
            return removed;
        }

        /// <summary>
        /// Remove the specified FucineRoot from the
        /// registry. If it is the currently loaded
        /// FucineRoot, the vanilla FucineRoot will be
        /// loaded in its place.
        /// </summary>
        public static RootSphereContainer Remove(int RootElementID)
        {
            var _span = Logger.Span();
            if (!RootsRegistry.ContainsKey(RootElementID))
            {
                throw new IndexOutOfRangeException();
            }
            if (RootElementID == 0)
            {
                throw new ArgumentException("Cannot remove vanilla root container");
            }
            if (RootElementID == CurrentRoot)
            {
                Get(0); // Set current to Vanilla with log message
            }
            _span.Info("Removing root container with ID {0}", RootElementID);
            var removed = Get(RootElementID);
            RootsRegistry.Remove(RootElementID);
            CurrentRoot = 0;
            return removed;
        }


        // Patch Methods

        // It's more efficient to use the public API methods to get
        // and manipulate the current root sphere container,
        // but to ensure interop with other mods as well as the
        // base game engine we're intercepting these function calls.

        /// <summary>
        /// Digs into the Watchman class via reflection and
        /// sets the value of <c>registered</c> which stores
        /// all of Watchman's singleton instances.
        /// </summary>
        private static void SetWatchmanInstance(Dictionary<Type, object> registry)
        {
            Traverse.Create<Watchman>().Field("registered").SetValue(registry);
        }

        /// <summary>
        /// Intercept calls to get the Fucine Root Sphere
        /// and instead substitute whatever our currently
        /// swapped-in sphere is.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPriority(int.MaxValue)]
        [HarmonyPatch(typeof(FucineRoot), nameof(FucineRoot.Get))]
        private static FucineRoot HookGet(FucineRoot r)
        {
            var _span = Logger.Span();
            _span.Debug("Caught FucineRoot.Get() hook. Substituting with internal ID '{0}'", CurrentRoot);
            return Get(CurrentRoot).sphereContainer;
        }

        /// <summary>
        /// Intercept calls to clear the Fucine Root sphere
        /// and instead substitute whatever our currently
        /// swapped-in sphere is.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPriority(int.MinValue)] // run as late as possible to give others a chance to do what they need to do.
        [HarmonyPatch(typeof(FucineRoot), nameof(FucineRoot.Reset))]
        private static void HookReset()
        {
            var _span = Logger.Span();
            _span.Debug("Caught FucineRoot.Reset() hook. Resetting internal ID '{0}'", CurrentRoot);
            Reset();
        }
        
    */
    }
}
