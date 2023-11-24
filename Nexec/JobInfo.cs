using Nexec.Internal;

namespace Nexec;

public class JobInfo
{
    private readonly IJobInfoProvider _infoProvider;

    private JobInfo(IJobInfoProvider infoProvider)
    {
        _infoProvider = infoProvider;
        Name = infoProvider.GetName();
        Inputs = infoProvider.GetInputs();
        Outputs = infoProvider.GetOutputs();
    }

    public string Name { get; }

    public IReadOnlyList<JobInput> Inputs { get; }

    public IReadOnlyList<JobOutput> Outputs { get; }

    public JobInstance Instantiate(IServiceProvider serviceProvider)
    {
        var value = _infoProvider.CreateInstance(serviceProvider);
        return new JobInstance(this, value, _infoProvider.GetExecuteDelegate(value));
    }

    internal static JobInfo FromType(Type type)
    {
        var infoProvider = new JobInfoByTypeProvider(type);
        return new JobInfo(infoProvider);
    }
}
