using HarmonyLib;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Doorways.Internals.Patches
{
    /// <summary>
    /// Handles patches for <see cref="InheritOverrideAttribute"/>.
    /// </summary>
    internal static class OverrideAttributePatches
    {
        public static void ApplyPatches(Assembly forAssembly)
        {
            var _span = Logger.Instance.Span();
            var MarkedClasses = forAssembly.GetTypes()
                .Where(x => x.IsClass && x.GetInterface(typeof(IInheritOverride<>).Name) != null);

            foreach (Type @class in MarkedClasses)
            {
                // Determine the base entity type to override to.
                Type parentType = @class.GetInterface(typeof(IInheritOverride<>).Name).GetGenericArguments()[0];

                // The tagged class must inherit from the overidden class at some point.
                if(!parentType.IsAssignableFrom(@class))
                {
                    throw new InvalidCastException($"Type '{@class.FullName}' does not inherit from type '{parentType.FullName}'");
                }

                // Iterate through all child and parent properties...
                foreach(PropertyInfo childProperty in @class.GetProperties())
                {
                    foreach (PropertyInfo parentProperty in parentType.GetProperties())
                    {
                        // And if the child class declares it shadows a parent class' property...
                        if (childProperty.IsDeclaredMember() && parentProperty.Name == childProperty.Name && parentProperty.PropertyType.IsAssignableFrom(childProperty.PropertyType))
                        {
                            // Perform the patch.
                            _span.Debug($"Patching {parentType.FullName}.{parentProperty.Name} redirect to {@class.FullName}.{childProperty.Name}");
                            if (parentProperty.CanRead)
                            {
                                if(parentProperty.GetGetMethod().IsVirtual || parentProperty.GetGetMethod().IsAbstract) { continue; }
                                try
                                {
                                    MethodInfo interceptor = AccessTools.Method(
                                        typeof(OverrideAttributePatches),
                                        nameof(OverrideAttributePatches.PatchGetter),
                                        generics: new Type[] { parentType, @class, childProperty.PropertyType }
                                    );
                                    DoorwaysFramework.GlobalPatcher.Patch(parentProperty.GetGetMethod(), postfix: new HarmonyMethod(interceptor));
                                }
                                catch (HarmonyException e)
                                {
                                    if (e.GetBaseException().Message == "Method has no body")
                                    {
                                        // Do nothing. This is expected behavior.
                                    }
                                    else
                                    {
                                        _span.Warn($"Error applying override patches for plugin {forAssembly.GetName().Name}::{parentType.Name}.{childProperty.Name}: {e}");
                                    }
                                }
                            }
                            if (parentProperty.CanWrite)
                            {
                                if (parentProperty.GetSetMethod().IsVirtual || parentProperty.GetSetMethod().IsAbstract) { continue; }
                                try
                                {
                                    MethodInfo interceptor = AccessTools.Method(
                                        typeof(OverrideAttributePatches),
                                        nameof(OverrideAttributePatches.PatchSetter),
                                        generics: new Type[] { parentType, @class, childProperty.PropertyType }
                                    );
                                    DoorwaysFramework.GlobalPatcher.Patch(parentProperty.GetSetMethod(), prefix: new HarmonyMethod(interceptor));
                                }
                                catch (HarmonyException e)
                                {
                                    if (e.GetBaseException().Message == "Method has no body")
                                    {
                                        // Do nothing. This is expected behavior.
                                    }
                                    else
                                    {
                                        _span.Warn($"Error applying override patches for plugin {forAssembly.GetName().Name}::{parentType.Name}.{childProperty.Name}: {e}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // When the value is requested from the parent class, get it from the child class instead.
        private static void PatchGetter<TParent, TChild, TParam>(ref TParam __result, TParent __instance, MethodBase __originalMethod) where TChild : TParent
        {
            var _span = Logger.Instance.Span();
            if(typeof(TChild) == typeof(TParent)) { /*_span.Warn($"Refusing to create redirect loop for {typeof(TParent).FullName}.{__originalMethod.Name}");*/ return; }
            TChild instance;
            try
            {
                instance = (TChild)__instance;
            }
            catch (InvalidCastException) { return; }
            _span.Debug($"Redirecting {__originalMethod.Name} : {typeof(TParent).FullName} -> {typeof(TChild).FullName}");
            __result = (TParam)AccessTools.Method(typeof(TChild), __originalMethod.Name)
                .Invoke(instance, new object[] { });
        }

        // When the value is set in the parent class, set it in the child class as well.
        private static void PatchSetter<TParent, TChild, TParam>(TParam value, TParent __instance, MethodBase __originalMethod) where TChild : TParent
        {
            var _span = Logger.Instance.Span();
            if (typeof(TChild) == typeof(TParent)) { /*_span.Warn($"Refusing to create redirect loop for {typeof(TParent).FullName}.{__originalMethod.Name}");*/ return; }
            TChild instance;
            try
            {
                instance = (TChild)__instance;
            }
            catch (InvalidCastException) { return; }
            _span.Debug($"Propagating {__originalMethod.Name} : {typeof(TParent).FullName} -> {typeof(TChild).FullName}");
            AccessTools.Method(typeof(TChild), __originalMethod.Name, parameters: new Type[] { typeof(TParam) })
                .Invoke(instance, new object[] { value });
        }
    }
}
