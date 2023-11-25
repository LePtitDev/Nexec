namespace Nexec.Internal;

internal class JobInfoByDelegateProvider<TState> : IJobInfoProvider
{
    private readonly string _name;
    private readonly Func<IServiceProvider, TState> _instantiate;
    private readonly Func<TState, Task> _execute;
    private readonly IReadOnlyList<JobInput> _inputs;
    private readonly IReadOnlyList<JobOutput> _outputs;

    public JobInfoByDelegateProvider(string name, Func<IServiceProvider, TState> instantiate, Func<TState, Task> execute, IReadOnlyList<JobInput> inputs, IReadOnlyList<JobOutput> outputs)
    {
        _name = name;
        _instantiate = instantiate;
        _execute = execute;
        _inputs = inputs;
        _outputs = outputs;
    }

    public string GetName() => _name;

    public IReadOnlyList<JobInput> GetInputs() => _inputs;

    public IReadOnlyList<JobOutput> GetOutputs() => _outputs;

    public object CreateInstance(IServiceProvider serviceProvider) => _instantiate(serviceProvider) ?? throw new InvalidOperationException($"Cannot instantiate '{_name}' job");

    public Delegate GetExecuteDelegate(object instance) => new Func<Task>(() => _execute(instance is TState state ? state : throw new ArgumentException($"Instance is not of type '{typeof(TState).FullName}'")));
}
