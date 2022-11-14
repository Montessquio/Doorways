using HarmonyLib;
using System.Reflection;
using UniverseLib;

namespace Doorways.Entities.Mixins
{
    /// <summary>
    /// Applies the <see cref="ForcedSuperclassMixin.ApplyPropertiesToBaseClass"/>
    /// mixin to the implementing class.
    /// The implementing class <b>must</b> be
    /// a descendant of <c>TBaseClass</c>.
    /// </summary>
    public interface IForcedSuperclass<TBaseClass> { }

    public static class ForcedSuperclassMixin
    {
        /// <summary>
        /// Statically (at the time this method is called) forcibly copies
        /// all properties in the superclass with the same names as ones in
        /// TBaseClass into its parent class' properties.
        /// <para/>
        /// This allows superclasses to "override" their base class' properties
        /// even if those properties aren't abstract or virtual.
        /// </summary>
        public static void ApplyPropertiesToBaseClass<TBaseClass>(this IForcedSuperclass<TBaseClass> obj)
        {
            foreach (PropertyInfo property in AccessTools.GetDeclaredProperties(obj.GetActualType()))
            {
                PropertyInfo underlyingProperty = AccessTools.Property(typeof(TBaseClass), property.Name);
                if (property.IsDeclaredMember())
                {
                    if (property.CanRead && underlyingProperty != null && underlyingProperty.CanWrite)
                    {
                        underlyingProperty.SetValue(obj, property.GetValue(obj));
                    }
                }
            }
        }
    }
}
