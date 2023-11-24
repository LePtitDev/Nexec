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
        return _getter(instance.Value);
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
