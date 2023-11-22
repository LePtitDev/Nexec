namespace Nexec;

public class JobRunner
{
    public Task ExecuteAsync(object instance)
    {
        var executeMethod = instance.GetType().GetMethod("Execute");
        executeMethod!.Invoke(instance, Array.Empty<object>());
        return Task.CompletedTask;
    }
}
