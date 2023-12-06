using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Nexec.Build.Internal;

internal static class PlatformHelpers
{
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static bool IsMac => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static bool IsX64 => RuntimeInformation.OSArchitecture == Architecture.X64;

    public static string? FindDotnetPath()
    {
        if (IsMac)
        {
            if (IsX64)
            {
                const string x64PathForMac = "/usr/local/share/dotnet/x64/dotnet";
                if (File.Exists(x64PathForMac))
                    return x64PathForMac;
            }

            const string pathForMac = "/usr/local/share/dotnet/dotnet";
            if (File.Exists(pathForMac))
                return pathForMac;
        }

        string[] extensions;
        string[] folders;
        if (IsWindows)
        {
            extensions = Environment.GetEnvironmentVariable("PATHEXT")?.Split(';') ?? Array.Empty<string>();
            folders = Environment.GetEnvironmentVariable("PATH")?.Split(';') ?? Array.Empty<string>();
        }
        else
        {
            extensions = new[] { string.Empty };
            folders = Environment.GetEnvironmentVariable("PATH")?.Split(':') ?? Array.Empty<string>();
        }

        var invalidPathChars = Path.GetInvalidPathChars();
        var dirs = new List<string>();
        foreach (var folder in folders)
        {
            if (folder.IndexOfAny(invalidPathChars) != -1)
                continue;

            dirs.Add(folder);
        }

        dirs.Add(Directory.GetCurrentDirectory());

        foreach (var file in dirs.SelectMany(dir => extensions.Select(ext => Path.Combine(dir, $"dotnet{ext}"))))
        {
            if (File.Exists(file))
                return file;
        }

        return null;
    }

    public static async Task<bool> RunProcessAsync(ProcessStartInfo startInfo)
    {
        var process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();
        await process.WaitForExitAsync();

        return process.ExitCode == 0;
    }
}
