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
        private static bool _isBuildScheduled = false;

        public static void Build(FSharpPlatform platform, FSharpConfiguration configuration)
        {
            FSharpProjectGenerator.GenerateFsprojIfNotFound();
            DotnetRunner.Run(false, "build", FSharpProjectGenerator.FSProjPath, $"-p:Platform={platform}", $"-p:Configuration={configuration}").Wait();
            AssetDatabase.ImportAsset(FSharpProjectGenerator.OutputDllPath);
        }
        
        public static async Task BuildAsync(FSharpPlatform platform, FSharpConfiguration configuration)
        {
            FSharpProjectGenerator.GenerateFsprojIfNotFound();
            if (await DotnetRunner.Run(true, "build", FSharpProjectGenerator.FSProjPath, $"-p:Platform={platform}", $"-p:Configuration={configuration}"))
            {
                AssetDatabase.ImportAsset(FSharpProjectGenerator.OutputDllPath);
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
    }
}
