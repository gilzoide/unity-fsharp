using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Gilzoide.FSharp.Editor.Internal;
using UnityEditor;

namespace Gilzoide.FSharp.Editor
{
    public class DotnetInstaller
    {
        public const string DotnetInstallDirKey = "dotnet-sdk-install-dir";
        public const string DotnetInstallDirDefault = "Library/dotnet-sdk-install-dir";
        public const string DotnetSdkVersion = "9.0.100";

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
        public static Task<bool> InstallDotnetSdk()
        {
            return InstallDotnetSdk(DotnetSdkVersion, true);
        }

        public static Task<bool> InstallDotnetSdk(bool async)
        {
            return InstallDotnetSdk(DotnetSdkVersion, async);
        }

        public static async Task<bool> InstallDotnetSdk(string sdkVersion, bool async)
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

            return await ProcessRunner.Run("Installing .NET SDK", async, DotnetInstallScriptPath, arguments);
        }
    }
}
