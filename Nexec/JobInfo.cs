using Nexec.Attributes;
using Nexec.Helpers;

namespace Nexec;

public class JobInfo
{
    private readonly Func<object> _instantiateFunc;

    private JobInfo(Func<object> instantiateFunc)
    {
        _instantiateFunc = instantiateFunc;
    }

    public string Name { get; init; } = default!;

    public IReadOnlyList<JobInput> Inputs { get; init; } = default!;

    public IReadOnlyList<JobOutput> Outputs { get; init; } = default!;

    public object Instantiate()
    {
        return _instantiateFunc();
    }

    internal static JobInfo FromType(Type type)
    {
        return new JobInfo(() => Activator.CreateInstance(type)!)
        {
            Name = type.Name,
            Inputs = type
                .GetProperties()
                .Where(p => p.HasAttribute<InputAttribute>())
                .Select(JobInput.FromProperty)
                .ToArray(),
            Outputs = type
                .GetProperties()
                .Where(p => p.HasAttribute<OutputAttribute>())
                .Select(JobOutput.FromProperty)
                .ToArray()
        };
    }
}
