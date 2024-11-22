using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.Compilation;

namespace Gilzoide.FSharp.Editor
{
    public static class FSharpProjectGenerator
    {
        public const string AssemblyName = "Assembly-FSharp";
        public const string FSProjPath = AssemblyName + ".fsproj";
        public const string OutputDir = "Assets/FSharpOutput";
        public const string OutputDllPath = OutputDir + "/" + AssemblyName + ".dll";

        public static void GenerateFsprojIfNotFound()
        {
            if (!File.Exists(FSProjPath))
            {
                GenerateFsproj();
            }
        }

        [MenuItem("Tools/F#/Generate Assembly-FSharp.fsproj")]
        public static string GenerateFsproj()
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

            Assembly[] editorAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Editor);
            string[] editorScriptingDefines = editorAssemblies[0].defines;
            var editorProperties = project.AddElement("PropertyGroup", "Condition", " '$(Platform)' == 'Editor' ");
            editorProperties.AddElement("DefineConstants", string.Join(";", editorScriptingDefines));

            Assembly[] playerAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies);
            string[] playerScriptingDefines = playerAssemblies[0].defines;
            var playerProperties = project.AddElement("PropertyGroup", "Condition", " '$(Platform)' == 'Player' ");
            playerProperties.AddElement("DefineConstants", string.Join(";", playerScriptingDefines));

            var debugProperties = project.AddElement("PropertyGroup", "Condition", " '$(Configuration)' == 'Debug' ");
            debugProperties.AddElement("DebugSymbols", "true");
            debugProperties.AddElement("DebugType", "full");
            debugProperties.AddElement("Optimize", "false");
            
            var releaseProperties = project.AddElement("PropertyGroup", "Condition", " '$(Configuration)' == 'Release' ");
            releaseProperties.AddElement("DebugSymbols", "false");
            releaseProperties.AddElement("Optimize", "true");

            var compileItems = project.AddElement("ItemGroup");
            foreach (string source in FSharpScriptOrder.LoadScriptPaths())
            {
                compileItems.AddElement("Compile", "Include", source);
            }

            var precompiledReferences = project.AddElement("ItemGroup");
            string[] precompiledAssemblyPaths = CompilationPipeline.GetPrecompiledAssemblyPaths(CompilationPipeline.PrecompiledAssemblySources.All);
            foreach (string path in precompiledAssemblyPaths)
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

            fsproj.Save(FSProjPath);
            return FSProjPath;
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
