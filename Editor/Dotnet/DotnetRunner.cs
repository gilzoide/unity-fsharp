using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Gilzoide.FSharp.Editor
{
    public static class DotnetRunner
    {
        public static async Task<bool> RunAsync(params string[] args)
        {
            // Install dotnet SDK if necessary
            bool installedDotnet = await DotnetInstaller.InstallDotnetSdkAsync();
            if (!installedDotnet)
            {
                return false;
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = $"{DotnetInstaller.DotnetInstallDir}/dotnet",
                Arguments = string.Join(" ", args),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            using (var process = Process.Start(processInfo))
            {
                process.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        Debug.Log(args.Data);
                    }
                };
                process.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        Debug.LogError(args.Data);
                    }
                };

                var progressId = Progress.Start($"dotnet {string.Join(" ", args)}");
                try
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    while (!process.HasExited)
                    {
                        Progress.Report(progressId, 0);
                        await Task.Yield();
                    }
                }
                finally
                {
                    Progress.Remove(progressId);
                }
                return process.ExitCode == 0;
            }
        }
    }
}
