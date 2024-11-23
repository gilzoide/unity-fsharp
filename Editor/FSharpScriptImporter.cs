using System.IO;
using Gilzoide.FSharp.Editor.Internal;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Gilzoide.FSharp.Editor
{
    [ScriptedImporter(0, "fs")]
    public class FSharpScriptImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var scriptTextAsset = new TextAsset(File.ReadAllText(ctx.assetPath));
            ctx.AddObjectToAsset("main", scriptTextAsset);

            FSharpBuilder.BuildOnceAsync(FSharpPlatform.Editor, FSharpConfiguration.Debug).Forget();
        }
    }
}
