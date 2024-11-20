using System.IO;
using Gilzoide.FSharp.Editor.Internal;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Gilzoide.FSharp.Editor
{
    [ScriptedImporter(0, "fs")]
    public class FSharpScriptedImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var scriptTextAsset = new TextAsset(File.ReadAllText(ctx.assetPath));
            ctx.AddObjectToAsset("main", scriptTextAsset);

            FSharpProjectGenerator.GenerateAndBuild(true, true).Forget();
        }
    }
}
