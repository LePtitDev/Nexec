using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Nexec.Build;
using Nexec.Engine;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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
            else if (arg.StartsWith("--project=", StringComparison.Ordinal))
            {
                if (!string.IsNullOrEmpty(options.ProjectPath))
                {
                    Console.Error.WriteLine("Project already defined");
                    return -1;
                }

                options.ProjectPath = arg["--project=".Length..];
            }
            else if (arg.Equals("--verbose", StringComparison.Ordinal))
            {
                options.Verbose = true;
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

        var serviceProvider = ConfigureServices(new ServiceCollection(), options.Verbose).BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        if (!string.IsNullOrEmpty(options.ProjectPath))
        {
            var builder = new NexecBuilder(options.ProjectPath);
            var buildLogger = loggerFactory.CreateLogger("Build");
            using (buildLogger.BeginScope($"Building '{options.ProjectPath}'..."))
            {
                var result = await builder.RunAsync(buildLogger);
                if (!result)
                {
                    buildLogger.LogError("Build failed!");
                    return -1;
                }
            }

            buildLogger.LogDebug("Build finished!");
            Console.WriteLine();
            options.AssemblyPath = builder.OutputFilePath;
        }

        if (string.IsNullOrEmpty(options.AssemblyPath))
        {
            options.AssemblyPath = typeof(Program).Assembly.Location;
        }

        var runnerLogger = loggerFactory.CreateLogger("Runner");
        IJobProvider provider;
        try
        {
            var assembly = Assembly.LoadFile(options.AssemblyPath);
            provider = JobProvider.From(assembly);
        }
        catch (Exception ex)
        {
            runnerLogger.LogError(ex, $"Cannot resolve assembly from path '{options.AssemblyPath}'");
            return -1;
        }

        var job = provider.GetJobs(serviceProvider).FirstOrDefault(t => string.Equals(t.Name, options.JobName, StringComparison.OrdinalIgnoreCase));
        if (job == null)
        {
            runnerLogger.LogError($"Job '{options.JobName}' not found");
            return -1;
        }

        var runner = new JobRunner(serviceProvider);

        JobInstance instance;
        try
        {
            instance = runner.Instantiate(job);
        }
        catch (Exception ex)
        {
            runnerLogger.LogError(ex, $"Cannot instantiate job '{job.Name}'");
            return -1;
        }

        foreach (var (key, value) in options.Properties)
        {
            var input = job.Inputs.FirstOrDefault(i => string.Equals(i.Name, key, StringComparison.OrdinalIgnoreCase));
            if (input == null)
            {
                runnerLogger.LogError($"Input '{key}' not found");
                return -1;
            }

            input.Set(instance, Convert.ChangeType(value, input.Type, CultureInfo.InvariantCulture));
        }

        try
        {
            var jobLogger = loggerFactory.CreateLogger("Job");
            using (jobLogger.BeginScope("Running job..."))
            {
                await runner.ExecuteAsync(instance);
            }
        }
        catch (Exception ex)
        {
            runnerLogger.LogError(ex, $"Execution of job '{job.Name}' failed!");
            return -1;
        }

        if (job.Outputs.Count > 0)
        {
            Console.WriteLine();
            runnerLogger.LogInformation("Output(s):");
            foreach (var output in job.Outputs)
            {
                runnerLogger.LogInformation($"  {output.Name}: {output.Get(instance)}");
            }
        }

        return 0;
    }

    private static IServiceCollection ConfigureServices(IServiceCollection services, bool verbose)
    {
        var loggerConfiguration = new LoggerConfiguration().WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:Ij}{NewLine}{Exception}");
        if (verbose)
            loggerConfiguration.MinimumLevel.Verbose();

        Log.Logger = loggerConfiguration.CreateLogger();

        services.AddLogging(builder =>
            builder.AddSerilog(dispose: true));

        return services;
    }

    private class Options
    {
        public string? JobName { get; set; }

        public string? AssemblyPath { get; set; }

        public string? ProjectPath { get; set; }

        public bool Verbose { get; set; }

        public Dictionary<string, string> Properties { get; } = new Dictionary<string, string>();
    }
}
