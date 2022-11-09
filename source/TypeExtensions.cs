using SecretHistories.Entities;
using SecretHistories.Fucine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Technically not a Patch,
/// but provides some extension
/// methods to extract entity
/// type metadata for the ModLoader.
/// </summary>
internal static class TypeExtensions
{
    public static Type AsInstanceOfGenericType(Type genericType, object instance)
    {
        Type type = instance.GetType();
        while (type != null)
        {
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == genericType)
            {
                return type.GenericTypeArguments[0];
            }
            type = type.BaseType;
        }
        return null;
    }
    public static bool IsDerivedFromGenericType(Type genericType, Type unknown)
    {
        Type type = unknown;
        while (type != null)
        {
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }
            type = type.BaseType;
        }
        return false;
    }

    public static Type GetUnderlyingElementType(this Type element)
    {
        if (typeof(IEntityWithId).IsAssignableFrom(element))
        {
            object obj = Activator.CreateInstance(element);
            IEntityWithId o = obj as IEntityWithId;
            return o.GetUnderlyingElement();
        }
        return null;
    }

    /// <summary>
    /// Returns either the underlying base element type
    /// of this entity. If Doorways can't find that type,
    /// it returns the top-level type definition.
    /// </summary>
    public static Type GetUnderlyingElement(this IEntityWithId element)
    {
        Type t = element.GetType();
        return AsInstanceOfGenericType(typeof(AbstractEntity<>), element) ?? element.GetType();
    }

    public static Type GetUnderlyingElement(this Achievement element)
    {
        return typeof(Achievement);
    }

    public static Type GetUnderlyingElement(this AngelSpecification element)
    {
        return typeof(AngelSpecification);
    }

    public static Type GetUnderlyingElement(this DeckSpec element)
    {
        return typeof(DeckSpec);
    }

    public static Type GetUnderlyingElement(this Dictum element)
    {
        return typeof(Dictum);
    }

    public static Type GetUnderlyingElement(this Element element)
    {
        return typeof(Element);
    }

    public static Type GetUnderlyingElement(this Ending element)
    {
        return typeof(Ending);
    }

    public static Type GetUnderlyingElement(this Expulsion element)
    {
        return typeof(Expulsion);
    }

    public static Type GetUnderlyingElement(this Legacy element)
    {
        return typeof(Legacy);
    }

    public static Type GetUnderlyingElement(this LinkedRecipeDetails element)
    {
        return typeof(LinkedRecipeDetails);
    }

    public static Type GetUnderlyingElement(this MorphDetails element)
    {
        return typeof(MorphDetails);
    }

    public static Type GetUnderlyingElement(this MutationEffect element)
    {
        return typeof(MutationEffect);
    }

    public static Type GetUnderlyingElement(this NullDeckSpec element)
    {
        return typeof(NullDeckSpec);
    }

    public static Type GetUnderlyingElement(this NullElement element)
    {
        return typeof(NullElement);
    }

    public static Type GetUnderlyingElement(this NullPortal element)
    {
        return typeof(NullPortal);
    }

    public static Type GetUnderlyingElement(this NullRecipe element)
    {
        return typeof(NullRecipe);
    }

    public static Type GetUnderlyingElement(this Portal element)
    {
        return typeof(Portal);
    }

    public static Type GetUnderlyingElement(this Recipe element)
    {
        return typeof(Recipe);
    }

    public static Type GetUnderlyingElement(this Setting element)
    {
        return typeof(Setting);
    }

    public static Type GetUnderlyingElement(this SphereSpec element)
    {
        return typeof(SphereSpec);
    }

    public static Type GetUnderlyingElement(this Verb element)
    {
        return typeof(Verb);
    }
}