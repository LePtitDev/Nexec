using Nexec.Attributes;
using Nexec.Helpers;

namespace Nexec.Internal;

internal class JobInfoByTypeProvider : IJobInfoProvider
{
    private readonly Type _type;

    public JobInfoByTypeProvider(Type type)
    {
        _type = type;
    }

    public string GetName()
    {
        return _type.Name;
    }

    public IReadOnlyList<JobInput> GetInputs()
    {
        return _type
            .GetProperties()
            .Where(p => p.HasAttribute<InputAttribute>())
            .Select(JobInput.FromProperty)
            .ToArray();
    }

    public IReadOnlyList<JobOutput> GetOutputs()
    {
        return _type
            .GetProperties()
            .Where(p => p.HasAttribute<OutputAttribute>())
            .Select(JobOutput.FromProperty)
            .ToArray();
    }

    public object CreateInstance(IServiceProvider serviceProvider)
    {
        return _type.Instantiate(serviceProvider);
    }

    public Delegate GetExecuteDelegate(object instance)
    {
        var methods = _type.GetMethods();
        foreach (var method in methods)
        {
            if (method.Name is "Execute" or "ExecuteAsync")
                return method.CreateDelegate(instance);
        }

        throw new InvalidOperationException($"Cannot resolve Execute[Async]() method from '{_type.FullName}'");
    }
}
