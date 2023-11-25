using System.Reflection;
using Nexec.Attributes;
using Nexec.Helpers;

namespace Nexec;

public interface IJobProvider
{
    public IEnumerable<JobInfo> GetJobs(IServiceProvider serviceProvider);
}

public class JobProvider : IJobProvider
{
    private readonly Assembly[] _assemblies;

    private JobProvider(Assembly[] assemblies)
    {
        _assemblies = assemblies;
    }

    public static IJobProvider Default => From(AppDomain.CurrentDomain.GetAssemblies());

    public IEnumerable<JobInfo> GetJobs(IServiceProvider serviceProvider)
    {
        return _assemblies
            .SelectMany(a => a
                .GetTypes()
                .Where(t => t.HasAttribute<JobAttribute>())
                .Select(JobInfo.FromType)
                .Concat(a
                    .GetTypes()
                    .Where(t => t.HasAttribute<ProviderAttribute>())
                    .SelectMany(t =>
                    {
                        if (!typeof(IJobProvider).IsAssignableFrom(t))
                            throw new InvalidOperationException($"Type '{t.FullName}' do not implement '{nameof(IJobProvider)}'");

                        var provider = (IJobProvider)t.Instantiate(serviceProvider);
                        return provider.GetJobs(serviceProvider);
                    }))
            )
            .ToArray();
    }

    public static IJobProvider From(params Assembly[] assemblies)
    {
        return new JobProvider(assemblies);
    }
}
