using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using UnityEditor;
using UnityEditor.Compilation;

namespace Gilzoide.FSharp.Editor
{
    public static class FSharpProjectGenerator
    {
        private const string OutputDir = "Assets/FSharpOutput";
        private const string AssemblyName = "Assembly-FSharp";
        private static string[] PrecompiledAssemblyPaths => CompilationPipeline.GetPrecompiledAssemblyPaths(CompilationPipeline.PrecompiledAssemblySources.All);
        private static bool _isBuildScheduled = false;

        [MenuItem("Tools/F#/Build Assembly-FSharp")]
        public static Task GenerateAndBuild()
        {
            return GenerateAndBuild(false, true);
        }

        [MenuItem("Tools/F#/Build Assembly-FSharp-Editor")]
        public static Task GenerateAndBuildEditor()
        {
            return GenerateAndBuild(true, true);
        }

        public static async Task GenerateAndBuild(bool editor, bool async)
        {
            if (_isBuildScheduled)
            {
                return;
            }

            _isBuildScheduled = true;
            if (async)
            {
                await Task.Yield();
            }
            try
            {
                IEnumerable<string> sources = AssetDatabase.FindAssets("glob:\"*.fs\"").Select(AssetDatabase.GUIDToAssetPath);
                string fsprojPath = GenerateFsproj(sources);
                await DotnetRunner.Run(async, "build", fsprojPath, $"-p:Platform={(editor ? "Editor" : "Player")}");
                AssetDatabase.Refresh();
            }
            finally
            {
                _isBuildScheduled = false;
            }
        }

        public static string GenerateFsproj(IEnumerable<string> sources)
        {
            var fsproj = new XmlDocument();
            
            var project = fsproj.AddElement("Project", "Sdk", "Microsoft.NET.Sdk");
            var defaultProperties = project.AddElement("PropertyGroup");
            defaultProperties.AddElement("GenerateAssemblyInfo", "false");
            defaultProperties.AddElement("EnableDefaultItems", "false");
            defaultProperties.AddElement("AppendTargetFrameworkToOutputPath", "false");
            defaultProperties.AddElement("Configurations", "Debug;Release");
            defaultProperties.AddElement("Configuration", "Condition", " '$(Configuration)' == '' ", "Debug");
            defaultProperties.AddElement("Platforms", "Editor;Player");
            defaultProperties.AddElement("Platform", "Condition", " '$(Platform)' == '' ", "Editor");
            defaultProperties.AddElement("AssemblyName", AssemblyName);
            defaultProperties.AddElement("OutputType", "Library");
            defaultProperties.AddElement("OutputPath", OutputDir);
            defaultProperties.AddElement("TargetFramework", "netstandard2.0");
            defaultProperties.AddElement("CopyLocalLockFileAssemblies", "true");
            defaultProperties.AddElement("SatelliteResourceLanguages", "none");
            defaultProperties.AddElement("NoStandardLibraries", "true");
            defaultProperties.AddElement("NoStdLib", "true");
            defaultProperties.AddElement("NoConfig", "true");
            defaultProperties.AddElement("DisableImplicitFrameworkReferences", "true");
            defaultProperties.AddElement("MSBuildWarningsAsMessages", "MSB3277");

            var editorProperties = project.AddElement("PropertyGroup", "Condition", " '$(Platform)' == 'Editor' ");
            editorProperties.AddElement("OutputPath", OutputDir);
            Assembly[] editorAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Editor);
            string[] editorScriptingDefines = editorAssemblies[0].defines;
            editorProperties.AddElement("DefineConstants", string.Join(";", editorScriptingDefines));

            var playerProperties = project.AddElement("PropertyGroup", "Condition", " '$(Platform)' == 'Player' ");
            Assembly[] playerAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies);
            string[] playerScriptingDefines = playerAssemblies[0].defines;
            playerProperties.AddElement("DefineConstants", string.Join(";", playerScriptingDefines));

            var debugProperties = project.AddElement("PropertyGroup", "Condition", " '$(Configuration)' == 'Debug' ");
            debugProperties.AddElement("DebugSymbols", "true");
            debugProperties.AddElement("DebugType", "full");
            debugProperties.AddElement("Optimize", "false");
            
            var releaseProperties = project.AddElement("PropertyGroup", "Condition", " '$(Configuration)' == 'Release' ");
            releaseProperties.AddElement("DebugSymbols", "false");
            releaseProperties.AddElement("Optimize", "true");

            var compileItems = project.AddElement("ItemGroup");
            foreach (string source in sources)
            {
                compileItems.AddElement("Compile", "Include", source);
            }

            var precompiledReferences = project.AddElement("ItemGroup");
            foreach (string path in PrecompiledAssemblyPaths)
            {
                if (path.Contains(OutputDir))
                {
                    continue;
                }
                var reference = precompiledReferences.AddElement("Reference", "Include", Path.GetFileNameWithoutExtension(path));
                reference.AddElement("HintPath", path);
                reference.AddElement("Private", "false");
            }

            var editorReferences = project.AddElement("ItemGroup", "Condition", " '$(Platform)' == 'Editor' ");
            foreach (Assembly assembly in editorAssemblies)
            {
                var reference = editorReferences.AddElement("Reference", "Include", assembly.name);
                reference.AddElement("HintPath", assembly.outputPath);
                reference.AddElement("Private", "false");
            }

            var playerReferences = project.AddElement("ItemGroup", "Condition", " '$(Platform)' == 'Player' ");
            foreach (Assembly assembly in playerAssemblies)
            {
                var reference = playerReferences.AddElement("Reference", "Include", assembly.name);
                reference.AddElement("HintPath", assembly.outputPath);
                reference.AddElement("Private", "false");
            }

            string fsprojPath = Path.ChangeExtension(AssemblyName, "fsproj");
            fsproj.Save(fsprojPath);
            return fsprojPath;
        }

        private static XmlElement AddElement(this XmlNode xml, string tag)
        {
            var child = (xml is XmlDocument xmlDoc ? xmlDoc : xml.OwnerDocument).CreateElement(tag);
            xml.AppendChild(child);
            return child;
        }

        private static XmlElement AddElement(this XmlNode xml, string tag, string text)
        {
            var child = xml.AddElement(tag);
            child.InnerText = text;
            return child;
        }
        
        private static XmlElement AddElement(this XmlNode xml, string tag, string attribute, string value)
        {
            var child = xml.AddElement(tag);
            child.SetAttribute(attribute, value);
            return child;
        }
        
        private static XmlElement AddElement(this XmlNode xml, string tag, string attribute, string value, string text)
        {
            var child = xml.AddElement(tag, attribute, value);
            child.InnerText = text;
            return child;
        }
    }
}
