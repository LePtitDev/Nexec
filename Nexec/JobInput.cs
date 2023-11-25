using System.Reflection;

namespace Nexec;

public class JobInput
{
    private readonly Action<object, object?> _setter;

    private JobInput(Action<object, object?> setter)
    {
        _setter = setter;
    }

    public string Name { get; init; } = default!;

    public Type Type { get; init; } = default!;

    public void Set(JobInstance instance, object? value)
    {
        if ((Type.IsValueType && (value == null || value.GetType() != Type)) || (!Type.IsValueType && value != null && !Type.IsInstanceOfType(value)))
            throw new ArgumentException(nameof(value), $"Value must be of type '{Type.FullName}'");

        _setter(instance.Value, value);
    }

    public static JobInput Create<TState, TValue>(string name, Action<TState, TValue?> setter)
    {
        return Create(name, typeof(TValue), (TState s, object? v) => setter(s, (TValue?)v));
    }

    public static JobInput Create<TState>(string name, Type type, Action<TState, object?> setter)
    {
        return new JobInput((i, v) => setter(i is TState state ? state : throw new ArgumentException($"Instance is not of type '{typeof(TState).FullName}'"), v))
        {
            Name = name,
            Type = type
        };
    }

    internal static JobInput FromProperty(PropertyInfo property)
    {
        if (!property.CanWrite)
            throw new InvalidOperationException($"Cannot write property '{property.Name}' from '{property.DeclaringType!.FullName}'");

        return new JobInput(property.SetValue)
        {
            Name = property.Name,
            Type = property.PropertyType
        };
    }
}
