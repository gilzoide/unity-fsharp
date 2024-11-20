using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Gilzoide.FSharp.Editor
{
    public class DotnetInstaller
    {
        public const string DotnetInstallDirKey = "dotnet-sdk-install-dir";
        public const string DotnetInstallDirDefault = "Library/dotnet-sdk-install-dir";
        public const string DotnetSdkVersion = "8.0.300";

        public static string DotnetInstallDir
        {
            get
            {
                string value = EditorPrefs.GetString(DotnetInstallDirKey, DotnetInstallDirDefault);
                if (string.IsNullOrEmpty(value))
                {
                    value = DotnetInstallDirDefault;
                }
                return value;
            }
            set => EditorPrefs.SetString(DotnetInstallDirKey, value);
        }

#if UNITY_EDITOR_WIN
        public static string DotnetInstallScriptName = "dotnet-install.ps1";
#else
        public static string DotnetInstallScriptName = "dotnet-install.sh";
#endif
        public static string DotnetInstallScriptPath => AssetDatabase.FindAssets($"glob:{DotnetInstallScriptName}", null)
            .Select(AssetDatabase.GUIDToAssetPath)
            .First();

        [MenuItem("Tools/F#/Install dotnet SDK")]
        public static Task<bool> InstallDotnetSdkAsync()
        {
            return InstallDotnetSdkAsync(DotnetSdkVersion);
        }

        public static async Task<bool> InstallDotnetSdkAsync(string sdkVersion)
        {
            if (Directory.Exists($"{DotnetInstallDir}/sdk")
                && (string.IsNullOrEmpty(sdkVersion) || Directory.Exists($"{DotnetInstallDir}/sdk/{sdkVersion}")))
            {
                return true;
            }

            var arguments = new List<string>
            {
                "--install-dir",
                $"'{DotnetInstallDir}'",
            };
            if (!string.IsNullOrEmpty(sdkVersion))
            {
                arguments.Add("--version");
                arguments.Add($"'{sdkVersion}'");
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = DotnetInstallScriptPath,
                Arguments = string.Join(" ", arguments),
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
