namespace Nexec;

public class JobRunner
{
    private readonly IServiceProvider _serviceProvider;

    public JobRunner(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public JobInstance Instantiate(JobInfo job)
    {
        return job.Instantiate(_serviceProvider);
    }

    public Task ExecuteAsync(JobInstance instance)
    {
        return instance.ExecuteAsync();
    }
}
