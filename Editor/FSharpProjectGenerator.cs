using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor.Compilation;
using UnityEngine;

namespace Gilzoide.FSharp.Editor
{
    public static class FSharpProjectGenerator
    {
        private const string OutputDir = "Assets/FSharpAssemblies";
        private static readonly Assembly[] ScriptAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies);
        private static readonly string[] PrecompiledAssemblyPaths = CompilationPipeline.GetPrecompiledAssemblyPaths(CompilationPipeline.PrecompiledAssemblySources.All);

        public static void GenerateFsproj(string source)
        {
            GenerateFsproj(Path.GetFileNameWithoutExtension(source), source);
        }

        public static void GenerateFsproj(string assemblyName, string source)
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

            defaultProperties.AddElement("NoStandardLibraries", "true");
            defaultProperties.AddElement("NoStdLib", "true");
            defaultProperties.AddElement("NoConfig", "true");
            defaultProperties.AddElement("DisableImplicitFrameworkReferences", "true");
            defaultProperties.AddElement("MSBuildWarningsAsMessages", "MSB3277");

            var compileItems = project.AddElement("ItemGroup");
            compileItems.AddElement("Compile", "Include", source);

            var precompiledReferences = project.AddElement("ItemGroup");
            foreach (Assembly assembly in ScriptAssemblies)
            {
                var reference = precompiledReferences.AddElement("Reference", "Include", assembly.name);
                reference.AddElement("HintPath", assembly.outputPath);
                reference.AddElement("Private", "false");
            }
            
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

            fsproj.Save(Path.ChangeExtension(assemblyName, "fsproj"));
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
