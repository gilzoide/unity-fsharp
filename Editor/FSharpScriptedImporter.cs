using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Gilzoide.FSharp.Editor
{
    [ScriptedImporter(0, "fs")]
    public class FSharpScriptedImporter : ScriptedImporter
    {
        public const string AssemblyFolder = "Assets/FSharpAssemblies";

        public override void OnImportAsset(AssetImportContext ctx)
        {
            Directory.CreateDirectory(AssemblyFolder);
            
            FSharpProjectGenerator.GenerateFsproj(ctx.assetPath);

            var scriptTextAsset = new TextAsset(File.ReadAllText(ctx.assetPath));
            ctx.AddObjectToAsset("main", scriptTextAsset);
        }
    }
}
