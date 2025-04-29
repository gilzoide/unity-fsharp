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
        const string FSharpProjectTypeGuid = "{F2A71F9B-5D33-465A-A702-920D77279786}";

        public static string OnGeneratedSlnSolution(string path, string content)
        {
            if (content.Contains($"\"{FSharpProjectGenerator.FSProjPath}\""))
            {
                return content;
            }

            var match = _projectRegex.Match(content);

            string projectType = match.Success
                ? match.Groups[1].Value               // copy first project’s type
                : FSharpProjectTypeGuid;              // or use F# GUID if none exist

            byte[] md5 = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(FSharpProjectGenerator.AssemblyName));
            string guid = new Guid(md5).ToString("B").ToUpper();

            int insertPos = match.Success
                ? match.Index
                : content.IndexOf("\nGlobal", StringComparison.Ordinal);

            if (insertPos < 0) insertPos = content.Length;

            var contentBuilder = new StringBuilder();
            contentBuilder.Append(content, 0, insertPos);
            contentBuilder.AppendLine($"Project({projectType}) = \"{FSharpProjectGenerator.AssemblyName}\", " +
                          $"\"{FSharpProjectGenerator.FSProjPath}\", \"{guid}\"");
            contentBuilder.AppendLine("EndProject");
            contentBuilder.Append(content, insertPos, content.Length - insertPos);

            return contentBuilder.ToString();
        }
    }
}


