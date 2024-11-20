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
        private static Assembly[] ScriptAssemblies => CompilationPipeline.GetAssemblies(BuildPipeline.isBuildingPlayer ? AssembliesType.PlayerWithoutTestAssemblies :  AssembliesType.Editor);
        private static string[] PrecompiledAssemblyPaths => CompilationPipeline.GetPrecompiledAssemblyPaths(CompilationPipeline.PrecompiledAssemblySources.All);
        private static bool _isBuildScheduled = false;

        [MenuItem("Tools/F#/Build Assembly-FSharp")]
        public static Task GenerateAndBuild()
        {
            return GenerateAndBuild(true);
        }

        public static async Task GenerateAndBuild(bool async)
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
                IEnumerable<string> sources = AssetDatabase.FindAssets("glob:\"*.fs\"")
                    .Select(AssetDatabase.GUIDToAssetPath);
                string fsprojPath = GenerateFsproj("Assembly-FSharp", sources);
                await DotnetRunner.Run(async, "build", fsprojPath);
                AssetDatabase.Refresh();
            }
            finally
            {
                _isBuildScheduled = false;
            }
        }

        public static string GenerateFsproj(string assemblyName, IEnumerable<string> sources)
        {
            var fsproj = new XmlDocument();
            
            var project = fsproj.AddElement("Project", "Sdk", "Microsoft.NET.Sdk");
            var defaultProperties = project.AddElement("PropertyGroup");
            defaultProperties.AddElement("GenerateAssemblyInfo", "false");
            defaultProperties.AddElement("EnableDefaultItems", "false");
            defaultProperties.AddElement("AppendTargetFrameworkToOutputPath", "false");
            defaultProperties.AddElement("Configurations", "Debug;Release");
            defaultProperties.AddElement("Configuration", "Condition", " '$(Configuration)' == '' ", "Debug");
            defaultProperties.AddElement("Platform", "Condition", " '$(Platform)' == '' ", "AnyCPU");
            defaultProperties.AddElement("AssemblyName", assemblyName);
            defaultProperties.AddElement("OutputType", "Library");
            defaultProperties.AddElement("OutputPath", OutputDir);
            defaultProperties.AddElement("TargetFramework", "netstandard2.0");
            defaultProperties.AddElement("CopyLocalLockFileAssemblies", "true");
            defaultProperties.AddElement("SatelliteResourceLanguages", "none");

            Assembly[] scriptAssemblies = ScriptAssemblies;
            string[] scriptingDefines = scriptAssemblies[0].defines;
            defaultProperties.AddElement("DefineConstants", string.Join(";", scriptingDefines));

            defaultProperties = project.AddElement("PropertyGroup");
            defaultProperties.AddElement("NoStandardLibraries", "true");
            defaultProperties.AddElement("NoStdLib", "true");
            defaultProperties.AddElement("NoConfig", "true");
            defaultProperties.AddElement("DisableImplicitFrameworkReferences", "true");
            defaultProperties.AddElement("MSBuildWarningsAsMessages", "MSB3277");

            var debugProperties = project.AddElement("PropertyGroup", "Condition", " '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ");
            debugProperties.AddElement("DebugSymbols", "true");
            debugProperties.AddElement("DebugType", "full");
            debugProperties.AddElement("Optimize", "false");
            
            var releaseProperties = project.AddElement("PropertyGroup", "Condition", " '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ");
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

            foreach (Assembly assembly in scriptAssemblies)
            {
                var reference = precompiledReferences.AddElement("Reference", "Include", assembly.name);
                reference.AddElement("HintPath", assembly.outputPath);
                reference.AddElement("Private", "false");
            }

            string fsprojPath = Path.ChangeExtension(assemblyName, "fsproj");
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
