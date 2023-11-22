using System.Reflection;

namespace Nexec.CLI;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var options = new Options();
        foreach (var arg in args)
        {
            if (arg.StartsWith("--assembly=", StringComparison.Ordinal))
            {
                if (!string.IsNullOrEmpty(options.AssemblyPath))
                {
                    Console.Error.WriteLine("Assembly already defined");
                    return -1;
                }

                options.AssemblyPath = arg["--assembly=".Length..];
            }
            else if (arg.StartsWith("--property=", StringComparison.Ordinal))
            {
                var keyValue = arg["--property=".Length..];
                var parts = keyValue.Split(':');
                if (parts.Length != 2)
                {
                    Console.Error.WriteLine("Property pattern is '--property=<name>:<value>'");
                    return -1;
                }

                if (options.Properties.ContainsKey(parts[0]))
                {
                    Console.Error.WriteLine($"Property '{parts[0]}' already defined");
                    return -1;
                }

                options.Properties[parts[0]] = parts[1];
            }
            else if (string.IsNullOrEmpty(options.JobName))
            {
                options.JobName = arg;
            }
            else
            {
                Console.Error.WriteLine("Too much arguments");
                return -1;
            }
        }

        if (string.IsNullOrEmpty(options.JobName))
        {
            Console.WriteLine("Job name is missing");
            return -1;
        }

        if (string.IsNullOrEmpty(options.AssemblyPath))
        {
            options.AssemblyPath = typeof(Program).Assembly.Location;
        }

        var provider = JobProvider.FromAssembly(Assembly.LoadFile(options.AssemblyPath));
        var job = provider.Jobs.FirstOrDefault(t => string.Equals(t.Name, options.JobName, StringComparison.OrdinalIgnoreCase));
        if (job == null)
        {
            Console.WriteLine($"Job '{options.JobName}' not found");
            return -1;
        }

        var instance = job.Instantiate();
        foreach (var (key, value) in options.Properties)
        {
            var input = job.Inputs.FirstOrDefault(i => string.Equals(i.Name, key, StringComparison.OrdinalIgnoreCase));
            if (input == null)
            {
                Console.WriteLine($"Input '{key}' not found");
                return -1;
            }

            input.Set(instance, Convert.ChangeType(value, input.Type));
        }

        var runner = new JobRunner();
        await runner.ExecuteAsync(instance);

        if (job.Outputs.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Output(s):");
            foreach (var output in job.Outputs)
            {
                Console.WriteLine($"  {output.Name}: {output.Get(instance)}");
            }
        }

        return 0;
    }

    private class Options
    {
        public string? JobName { get; set; }

        public string? AssemblyPath { get; set; }

        public Dictionary<string, string> Properties { get; } = new Dictionary<string, string>();
    }
}
