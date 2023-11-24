namespace Nexec.Internal;

internal interface IJobInfoProvider
{
    public string GetName();

    public IReadOnlyList<JobInput> GetInputs();

    public IReadOnlyList<JobOutput> GetOutputs();

    public object CreateInstance(IServiceProvider serviceProvider);

    public Delegate GetExecuteDelegate(object instance);
}
