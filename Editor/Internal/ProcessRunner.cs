using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Gilzoide.FSharp.Editor.Internal
{
    public static class ProcessRunner
    {
        public static Task<bool> Run(string progressMessage, bool async, string fileName, params string[] args)
        {
            return Run(progressMessage, async, fileName, (IEnumerable<string>) args);
        }

        public static async Task<bool> Run(string progressMessage, bool async, string fileName, IEnumerable<string> args)
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
                        Debug.Log($"[{progressMessage}] {args.Data}");
                    }
                };
                process.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        Debug.LogError($"[{progressMessage}] {args.Data}");
                    }
                };

                var progressId = Progress.Start(progressMessage);
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
