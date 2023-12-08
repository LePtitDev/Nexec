using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexec.Attributes;

namespace Nexec.Samples;

[Provider]
public class MathProvider : IJobProvider
{
    public IEnumerable<JobInfo> GetJobs(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<MathProvider>>();

        yield return JobInfo.Create("Math_Floor", _ => new SingleDouble(), s =>
        {
            logger.LogInformation($"Math.Floor({s.Value})");
            s.Result = Math.Floor(s.Value);
        }, new[]
        {
            JobInput.Create("Value", (SingleDouble s, double value) => s.Value = value)
        }, new[]
        {
            JobOutput.Create("Result", (SingleDouble s) => s.Result)
        });

        yield return JobInfo.Create("Math_Ceil", _ => new SingleDouble(), s =>
        {
            logger.LogInformation($"Math.Ceil({s.Value})");
            s.Result = Math.Ceiling(s.Value);
        }, new[]
        {
            JobInput.Create("Value", (SingleDouble s, double value) => s.Value = value)
        }, new[]
        {
            JobOutput.Create("Result", (SingleDouble s) => s.Result)
        });

        yield return JobInfo.Create("Math_Round", _ => new SingleDouble(), s =>
        {
            logger.LogInformation($"Math.Round({s.Value})");
            s.Result = Math.Round(s.Value);
        }, new[]
        {
            JobInput.Create("Value", (SingleDouble s, double value) => s.Value = value)
        }, new[]
        {
            JobOutput.Create("Result", (SingleDouble s) => s.Result)
        });
    }

    private class SingleDouble
    {
        public double Value { get; set; }

        public double Result { get; set; }
    }
}
