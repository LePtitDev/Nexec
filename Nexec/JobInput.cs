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

    public void Set(object jobInstance, object? value)
    {
        _setter(jobInstance, value);
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
