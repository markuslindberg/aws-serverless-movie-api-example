using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MovieApi.Tests.Infrastructure;

public class Bash
{
    private static string _bashExecutable;
    private readonly string _workingDirectory;

    static Bash()
    {
        _bashExecutable = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "bash.exe" : "bash";
    }

    public Bash(string workingDirectory)
    {
        _workingDirectory = workingDirectory;
    }

    public void Run(string command, Action<string> output = null)
    {
        var info = new ProcessStartInfo("bash")
        {
            WorkingDirectory = _workingDirectory,
            Arguments = $"-c \"{command}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false,
            ErrorDialog = false
        };

        using (var bash = new Process { StartInfo = info })
        {
            bash.Start();
            bash.EnableRaisingEvents = true;
            bash.BeginOutputReadLine();
            bash.BeginErrorReadLine();

            bash.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                    output?.Invoke(args.Data);
            };

            bash.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                    output?.Invoke(args.Data);
            };

            bash.WaitForExit();
            bash.Close();
        }
    }
}