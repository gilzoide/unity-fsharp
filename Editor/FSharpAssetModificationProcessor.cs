using System.Globalization;
using System.Threading.Tasks;
using UnityEditor;

namespace Gilzoide.FSharp.Editor
{
    public class FSharpAssetModificationProcessor : AssetModificationProcessor
    {
        private static void OnWillCreateAsset(string assetName)
        {
            if (ShouldGenerateFsproj(assetName))
            {
                OnSomethingChanged();
            }
        }
        
        private static AssetDeleteResult OnWillDeleteAsset(string assetName, RemoveAssetOptions options)
        {
            if (ShouldGenerateFsproj(assetName))
            {
                OnSomethingChanged();
            }
            return AssetDeleteResult.DidNotDelete;
        }

        private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            if (ShouldGenerateFsproj(sourcePath) || ShouldGenerateFsproj(destinationPath))
            {
                OnSomethingChanged();
            }
            return AssetMoveResult.DidNotMove;
        }

        private static bool ShouldGenerateFsproj(string assetName)
        {
            return assetName.EndsWith(".fs", true, CultureInfo.InvariantCulture)
                || assetName.EndsWith(".dll", true, CultureInfo.InvariantCulture)
                || assetName.EndsWith(".asmdef", true, CultureInfo.InvariantCulture);
        }

        private static bool _scheduled = false;
        private static async void OnSomethingChanged()
        {
            if (_scheduled)
            {
                return;
            }

            _scheduled = true;
            try
            {
                await Task.Yield();
                FSharpScriptOrder.SyncMissingScripts();
                FSharpProjectGenerator.GenerateFsproj();
            }
            finally
            {
                _scheduled = false;
            }
        }
    }
}
