using System.IO;
using System.Threading.Tasks;
using Gilzoide.FSharp.Editor.Internal;
using UnityEditor;

namespace Gilzoide.FSharp.Editor
{
    public enum FSharpPlatform
    {
        Editor,
        Player,
    }

    public enum FSharpConfiguration
    {
        Debug,
        Release,
    }

    public static class FSharpBuilder
    {
        public const string NuGetConfigPath = "nuget.config";

        private static bool _isBuildScheduled = false;

        public static void Build(FSharpPlatform platform, FSharpConfiguration configuration)
        {
            Build(platform, configuration, false).Wait();
        }

        public static Task BuildAsync(FSharpPlatform platform, FSharpConfiguration configuration)
        {
            return Build(platform, configuration, true);
        }

        private static async Task Build(FSharpPlatform platform, FSharpConfiguration configuration, bool async)
        {
            FSharpProjectGenerator.GenerateFsproj();
            await GenerateNuGetConfig(async);
            if (await DotnetRunner.Run("dotnet build Assembly-FSharp.fsproj", async, "build", FSharpProjectGenerator.FSProjPath, $"-p:Platform={platform}", $"-p:Configuration={configuration}"))
            {
                AssetDatabase.ImportAsset(FSharpProjectGenerator.OutputDir, ImportAssetOptions.ImportRecursive);
            }
        }

        public static async Task BuildOnceAsync(FSharpPlatform platform, FSharpConfiguration configuration)
        {
            if (_isBuildScheduled)
            {
                return;
            }

            _isBuildScheduled = true;
            try
            {
                await Task.Yield();
                await BuildAsync(platform, configuration);
            }
            finally
            {
                _isBuildScheduled = false;
            }
        }

        [MenuItem("Tools/F#/Build Assembly-FSharp")]
        private static void BuildEditor()
        {
            BuildAsync(FSharpPlatform.Editor, FSharpConfiguration.Debug).Forget();
        }

        public static async Task<bool> GenerateNuGetConfig(bool async)
        {
            if (File.Exists(NuGetConfigPath))
            {
                return true;
            }
            return await DotnetRunner.Run($"Generating {NuGetConfigPath}", async, "new", "nugetconfig");
        }
    }
}
