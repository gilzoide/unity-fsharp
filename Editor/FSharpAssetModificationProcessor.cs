using Gilzoide.FSharp.Editor.Internal;
using UnityEditor;

namespace Gilzoide.FSharp.Editor
{
    public class FSharpAssetModificationProcessor : AssetModificationProcessor
    {
        private static void OnWillCreateAsset(string assetName)
        {
            if (assetName.EndsWith(".fs"))
            {
                FSharpProjectGenerator.GenerateFsprojOnceAsync().Forget();
            }
        }
        
        private static AssetDeleteResult OnWillDeleteAsset(string assetName, RemoveAssetOptions options)
        {
            if (assetName.EndsWith(".fs"))
            {
                FSharpProjectGenerator.GenerateFsprojOnceAsync().Forget();
            }
            return AssetDeleteResult.DidNotDelete;
        }

        private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            if (sourcePath.EndsWith(".fs") || destinationPath.EndsWith(".fs"))
            {
                FSharpProjectGenerator.GenerateFsprojOnceAsync().Forget();
            }
            return AssetMoveResult.DidNotMove;
        }
    }
}
