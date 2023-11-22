using System.Reflection;
using Nexec.Attributes;
using Nexec.Helpers;

namespace Nexec;

public class JobProvider
{
    public JobProvider(JobInfo[] jobs)
    {
        Jobs = jobs;
    }

    public JobInfo[] Jobs { get; }

    public static JobProvider FromAssembly(Assembly assembly)
    {
        return new JobProvider(assembly.GetTypes().Where(t => t.HasAttribute<JobAttribute>()).Select(JobInfo.FromType).ToArray());
    }
}
