using Nexec.Attributes;

namespace Nexec.Samples;

[Provider]
public class MathProvider : IJobProvider
{
    public IEnumerable<JobInfo> GetJobs(IServiceProvider _)
    {
        yield return JobInfo.Create("Math_Floor", _ => new SingleDouble(), s =>
        {
            Console.WriteLine($"Math.Floor({s.Value})");
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
            Console.WriteLine($"Math.Ceil({s.Value})");
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
            Console.WriteLine($"Math.Round({s.Value})");
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
