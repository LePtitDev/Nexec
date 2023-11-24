namespace Nexec;

public class JobInstance
{
    private readonly Delegate _executeFunc;

    public JobInstance(JobInfo info, object value, Delegate executeFunc)
    {
        _executeFunc = executeFunc;
        Info = info;
        Value = value;
    }

    public JobInfo Info { get; }

    public object Value { get; }

    public Task ExecuteAsync()
    {
        var result = _executeFunc.DynamicInvoke();
        return result is Task task ? task : Task.CompletedTask;
    }
}
