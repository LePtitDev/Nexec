using System.Reflection;

namespace Nexec;

public class JobOutput
{
    private readonly Func<object, object?> _getter;

    private JobOutput(Func<object, object?> getter)
    {
        _getter = getter;
    }

    public string Name { get; init; } = default!;

    public Type Type { get; init; } = default!;

    public object? Get(JobInstance instance)
    {
        var value = _getter(instance.Value);

        if ((Type.IsValueType && (value == null || value.GetType() != Type)) || (!Type.IsValueType && value != null && !Type.IsInstanceOfType(value)))
            throw new InvalidOperationException($"Value must be of type '{Type.FullName}'");

        return value;
    }

    public static JobOutput Create<TState, TValue>(string name, Func<TState, TValue?> getter)
    {
        return Create(name, typeof(TValue), (TState s) => getter(s));
    }

    public static JobOutput Create<TState>(string name, Type type, Func<TState, object?> getter)
    {
        return new JobOutput(i => getter(i is TState state ? state : throw new ArgumentException($"Instance is not of type '{typeof(TState).FullName}'")))
        {
            Name = name,
            Type = type
        };
    }

    internal static JobOutput FromProperty(PropertyInfo property)
    {
        if (!property.CanRead)
            throw new InvalidOperationException($"Cannot read property '{property.Name}' from '{property.DeclaringType!.FullName}'");

        return new JobOutput(property.GetValue)
        {
            Name = property.Name,
            Type = property.PropertyType
        };
    }
}
