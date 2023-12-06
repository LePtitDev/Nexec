using System.Diagnostics;
using Nexec.Build.Internal;

namespace Nexec.Build;

public class NexecBuilder
{
    public NexecBuilder(string projectOrSolutionPath)
    {
        ProjectOrSolutionPath = projectOrSolutionPath;
    }

    public string ProjectOrSolutionPath { get; }

    public string OutputDirectory => Path.Combine(Environment.CurrentDirectory, ".build");

    public string OutputFilePath => Path.Combine(OutputDirectory, Path.GetFileNameWithoutExtension(ProjectOrSolutionPath) + ".dll");

    public Task<bool> RunAsync()
    {
        var dotnetPath = PlatformHelpers.FindDotnetPath();
        if (string.IsNullOrEmpty(dotnetPath))
            throw new FileNotFoundException("Cannot find dotnet");

        return PlatformHelpers.RunProcessAsync(new ProcessStartInfo
        {
            FileName = dotnetPath,
            ArgumentList =
            {
                "build",
                ProjectOrSolutionPath,
                "-c", "Release",
                "-o", OutputDirectory
            }
        });
    }
}
