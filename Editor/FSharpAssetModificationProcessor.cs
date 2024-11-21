using System.Globalization;
using Gilzoide.FSharp.Editor.Internal;
using UnityEditor;

namespace Gilzoide.FSharp.Editor
{
    public class FSharpAssetModificationProcessor : AssetModificationProcessor
    {
        private static void OnWillCreateAsset(string assetName)
        {
            if (ShouldGenerateFsproj(assetName))
            {
                FSharpProjectGenerator.GenerateFsprojOnceAsync().Forget();
            }
        }
        
        private static AssetDeleteResult OnWillDeleteAsset(string assetName, RemoveAssetOptions options)
        {
            if (ShouldGenerateFsproj(assetName))
            {
                FSharpProjectGenerator.GenerateFsprojOnceAsync().Forget();
            }
            return AssetDeleteResult.DidNotDelete;
        }

        private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            if (ShouldGenerateFsproj(sourcePath) || ShouldGenerateFsproj(destinationPath))
            {
                FSharpProjectGenerator.GenerateFsprojOnceAsync().Forget();
            }
            return AssetMoveResult.DidNotMove;
        }

        private static bool ShouldGenerateFsproj(string assetName)
        {
            return assetName.EndsWith(".fs", true, CultureInfo.InvariantCulture)
                || assetName.EndsWith(".dll", true, CultureInfo.InvariantCulture)
                || assetName.EndsWith(".asmdef", true, CultureInfo.InvariantCulture);
        }
    }
}
