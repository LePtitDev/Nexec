using System.Linq.Expressions;
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

    public static Delegate CreateDelegate(this MethodInfo method, object? target)
    {
        Func<Type[], Type> getType;

        var types = method.GetParameters().Select(p => p.ParameterType);
        if (method.ReturnType == typeof(void))
        {
            getType = Expression.GetActionType;
        }
        else
        {
            getType = Expression.GetFuncType;
            types = types.Append(method.ReturnType);
        }

        if (method.IsStatic)
            return Delegate.CreateDelegate(getType(types.ToArray()), method);

        if (target == null)
            throw new ArgumentNullException(nameof(target), $"Target cannot be null for non-static method '{method.DeclaringType?.FullName}.{method.Name}()'");

        return Delegate.CreateDelegate(getType(types.ToArray()), target, method.Name);
    }

    public static object Instantiate(this Type type, IServiceProvider serviceProvider)
    {
        var constructors = type.GetConstructors();
        foreach (var constructor in constructors)
        {
            var parameters = constructor.GetParameters();
            var paramList = new List<object?>();
            foreach (var parameter in parameters)
            {
                var service = serviceProvider.GetService(parameter.ParameterType);
                if (service != null)
                {
                    paramList.Add(service);
                }
                else
                {
                    break;
                }
            }

            if (parameters.Length != paramList.Count)
                continue;

            return constructor.Invoke(paramList.ToArray());
        }

        throw new InvalidOperationException($"Cannot resolve constructor from '{type.FullName}'");
    }
}
