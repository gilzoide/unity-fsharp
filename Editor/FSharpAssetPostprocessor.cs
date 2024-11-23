using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Gilzoide.FSharp.Editor
{
    public class FSharpAssetPostprocessor : AssetPostprocessor
    {
        static readonly Regex _projectRegex = new Regex(@"Project\(([^)]+)\)");

        public static string OnGeneratedSlnSolution(string path, string content)
        {
            if (_projectRegex.Match(content) is Match match)
            {
                byte[] md5 = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes("Assembly-FSharp.fsproj"));
                string guid = new Guid(md5).ToString("B").ToUpper();
                var contentBuilder = new StringBuilder();
                contentBuilder.Append(content, 0, match.Index);
                contentBuilder.AppendLine($"{match.Captures[0]} = \"Assembly-FSharp\", \"Assembly-FSharp.fsproj\", \"{guid}\"");
                contentBuilder.AppendLine("EndProject");
                contentBuilder.Append(content, match.Index, content.Length - match.Index);
                content = contentBuilder.ToString();
            }
            return content;
        }
    }
}
