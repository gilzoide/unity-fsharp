using System.IO;
using UnityEditor;

namespace Gilzoide.FSharp.Editor
{
    public class FSharpAssetPostprocessor : AssetPostprocessor
    {
        public static string OnGeneratedSlnSolution(string path, string content)
        {
            if (!content.Contains("Assembly-FSharp.fsproj"))
            {
                File.WriteAllText(path, content);
                FSharpProjectGenerator.GenerateFsproj();
                DotnetRunner.Run($"dotnet sln '{path}' add Assembly-FSharp.fsproj", false, "sln", path, "add", "Assembly-FSharp.fsproj").Wait();
                return File.ReadAllText(path);
            }
            return content;
        }
    }
}
