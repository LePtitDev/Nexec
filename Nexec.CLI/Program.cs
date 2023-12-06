﻿using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Nexec.Build;
using Nexec.Engine;

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

        if (!string.IsNullOrEmpty(options.ProjectPath))
        {
            Console.WriteLine($"Building '{options.ProjectPath}'...");
            var builder = new NexecBuilder(options.ProjectPath);
            var result = await builder.RunAsync();
            if (!result)
            {
                Console.WriteLine("Build failed!");
                return -1;
            }

            Console.WriteLine("Build finished!");
            Console.WriteLine();
            options.AssemblyPath = builder.OutputFilePath;
        }

        if (string.IsNullOrEmpty(options.AssemblyPath))
        {
            options.AssemblyPath = typeof(Program).Assembly.Location;
        }

        IJobProvider provider;
        try
        {
            var assembly = Assembly.LoadFile(options.AssemblyPath);
            provider = JobProvider.From(assembly);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Cannot resolve assembly from path '{options.AssemblyPath}'");
            Console.Error.WriteLine($"Exception: {ex}");
            return -1;
        }

        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var job = provider.GetJobs(serviceProvider).FirstOrDefault(t => string.Equals(t.Name, options.JobName, StringComparison.OrdinalIgnoreCase));
        if (job == null)
        {
            Console.WriteLine($"Job '{options.JobName}' not found");
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
            Console.Error.WriteLine($"Cannot instantiate job '{job.Name}'");
            Console.Error.WriteLine($"Exception: {ex}");
            return -1;
        }

        foreach (var (key, value) in options.Properties)
        {
            var input = job.Inputs.FirstOrDefault(i => string.Equals(i.Name, key, StringComparison.OrdinalIgnoreCase));
            if (input == null)
            {
                Console.WriteLine($"Input '{key}' not found");
                return -1;
            }

            input.Set(instance, Convert.ChangeType(value, input.Type, CultureInfo.InvariantCulture));
        }

        try
        {
            await runner.ExecuteAsync(instance);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Execution of job '{job.Name}' failed!");
            Console.Error.WriteLine($"Exception: {ex}");
            return -1;
        }

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

        public string? ProjectPath { get; set; }

        public Dictionary<string, string> Properties { get; } = new Dictionary<string, string>();
    }
}
