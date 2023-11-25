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

    public static JobInfo Create<TState>(string name, Func<IServiceProvider, TState> instantiate, Action<TState> execute, IReadOnlyList<JobInput>? inputs = null, IReadOnlyList<JobOutput>? outputs = null)
    {
        return Create(name, instantiate, s =>
        {
            execute(s);
            return Task.CompletedTask;
        }, inputs, outputs);
    }

    public static JobInfo Create<TState>(string name, Func<IServiceProvider, TState> instantiate, Func<TState, Task> execute, IReadOnlyList<JobInput>? inputs = null, IReadOnlyList<JobOutput>? outputs = null)
    {
        var infoProvider = new JobInfoByDelegateProvider<TState>(name, instantiate, execute, inputs ?? Array.Empty<JobInput>(), outputs ?? Array.Empty<JobOutput>());
        return new JobInfo(infoProvider);
    }

    internal static JobInfo FromType(Type type)
    {
        var infoProvider = new JobInfoByTypeProvider(type);
        return new JobInfo(infoProvider);
    }
}
