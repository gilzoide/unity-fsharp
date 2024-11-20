using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Gilzoide.FSharp.Editor.Internal
{
    public static class ProcessRunner
    {
        public static Task<bool> Run(string fileName, bool async, params string[] args)
        {
            return Run(fileName, async, (IEnumerable<string>) args);
        }

        public static async Task<bool> Run(string fileName, bool async, IEnumerable<string> args)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = fileName,
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

                var progressId = Progress.Start("Installing dotnet SDK");
                Progress.Report(progressId, 0);
                try
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    if (async)
                    {
                        while (!process.HasExited)
                        {
                            await Task.Yield();
                        }
                    }
                    else
                    {
                        process.WaitForExit();
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
