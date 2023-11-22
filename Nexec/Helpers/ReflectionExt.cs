using System.Reflection;

namespace Nexec.Helpers;

internal static class ReflectionExt
{
    public static T? GetAttribute<T>(this ICustomAttributeProvider member, bool inherit = true) where T : Attribute
    {
        return GetAttributes<T>(member, inherit).FirstOrDefault();
    }

    public static T[] GetAttributes<T>(this ICustomAttributeProvider member, bool inherit = true) where T : Attribute
    {
        return (T[])member.GetCustomAttributes(typeof(T), inherit);
    }

    public static bool HasAttribute(this ICustomAttributeProvider member, Type attributeType, bool inherit = true)
    {
        return member.GetCustomAttributes(attributeType, inherit).Any();
    }

    public static bool HasAttribute<T>(this ICustomAttributeProvider member, bool inherit = true) where T : Attribute
    {
        return GetAttributes<T>(member, inherit).Any();
    }

    public static IEnumerable<Type> GetTypesOf(this Assembly assembly, Type baseType, bool withAbstract = true, bool withGeneric = true)
    {
        var types = assembly.GetTypes().Where(t => t != baseType && baseType.IsAssignableFrom(t));
        if (!withAbstract)
            types = types.Where(x => !x.IsAbstract);

        if (!withGeneric)
            types = types.Where(x => !x.IsGenericType);

        return types;
    }

    public static IEnumerable<Type> GetTypesOf<TBase>(this Assembly assembly, bool withAbstract = true, bool withGeneric = true)
    {
        return GetTypesOf(assembly, typeof(TBase), withAbstract, withGeneric);
    }
}
