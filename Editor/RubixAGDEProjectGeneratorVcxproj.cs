using System.Xml.Linq;
using System.Collections.Generic;
using UnityEditor.Android;

namespace Rubix.Android.VisualStudio.AGDE.Editor
{
    class AGDEProjectGeneratorVcxproj : AGDEProjectGeneratorBase
    {
        internal AGDEProjectGeneratorVcxproj(AGDECreateArguments createArgs) 
            : base(createArgs)
        {
        }

        private XElement CreateGlobals()
        {
            var globals = ElementPropertyGroup(AttributeLabel("Globals"));

            globals.Add(Element("VCProjectVersion", "15.0"));
            globals.Add(Element("ProjectGuid", $"{{{CreateArgs.VCXProjectGuid}}}"));
            globals.Add(Element("VCProjectVersion", "15.0"));
            globals.Add(Element("Keyword", "Win32Proj"));
            globals.Add(Element("RootNamespace", "launcher"));
            globals.Add(Element("WindowsTargetPlatformVersion", "10.0"));
            globals.Add(Element("AndroidSdk", CreateArgs.AndroidSDK.Path));
            globals.Add(Element("AndroidNdkDirectory", CreateArgs.AndroidNDK.Path));
            globals.Add(Element("JAVA_HOME", AndroidExternalToolsSettings.jdkRootPath));
            return globals;
        }

        private XElement CreateProjectConfiguration(string configuration, string platform)
        {
            return Element("ProjectConfiguration",
                new XAttribute("Include", $"{configuration}|{platform}"),
                Element("Configuration", configuration),
                Element("Platform", platform));
        }

        private XElement CreateProjectConfigurations()
        {
            var projectConfigurations = Element("ItemGroup", AttributeLabel("ProjectConfigurations"));

            foreach (var c in CreateArgs.EnumerateConfigurations())
            {
                foreach (var p in CreateArgs.EnumeratePlatforms())
                    projectConfigurations.Add(CreateProjectConfiguration(c, p));
            }
            return projectConfigurations;
        }

        private IEnumerable<XElement> CreateConfigurationPropertyGroups()
        {
            foreach (var c in CreateArgs.EnumerateConfigurations())
            {
                foreach (var p in CreateArgs.EnumeratePlatforms())
                {
                    var propertyGroup = ElementPropertyGroup(
                        AttributeLabel("Configuration"),
                        AttributeCondition($"'$(Configuration)|$(Platform)'=='{c}|{p}'"));

                    propertyGroup.Add(Element("PlatformSet", "Clang"));
                    propertyGroup.Add(Element("AndroidMinSdkVersion", CreateArgs.MinSdkVersion));
                    propertyGroup.Add(Element("AndroidNdkVersion", CreateArgs.AndroidNDK.Version));
                    yield return propertyGroup;
                }
            }
        }

        private IEnumerable<XElement> CreatePropertyGroups()
        {
            foreach (var c in CreateArgs.EnumerateConfigurations())
            {
                foreach (var p in CreateArgs.EnumeratePlatforms())
                {
                    var propertyGroup = ElementPropertyGroup(AttributeCondition($"'$(Configuration)|$(Platform)'=='{c}|{p}'"));

                    propertyGroup.Add(Element("AndroidEnablePackaging", "true"));
                    propertyGroup.Add(Element("AndroidGradleBuildDir", $"$(MSBuildProjectDirectory)\\{CreateArgs.PrefixIncludeFile}"));
                    propertyGroup.Add(Element("AndroidApplicationModule", CreateArgs.ApplicationModule));
                    propertyGroup.Add(Element("AndroidGradleBuildType", "$(Configuration)"));
                    propertyGroup.Add(Element("AndroidDebugComponent", $"{CreateArgs.PackageName}/{CreateArgs.LaunchActivity}"));
                    yield return propertyGroup;
                }
            }
        }

        private XElement CreateFilesItemGroup()
        {
            var itemGroup = ElementItemGroup();

            foreach (var file in CreateArgs.GradleProjectFiles)
            {
                var none = Element("None",  AttributeInclude(file));
                itemGroup.Add(none);
            }

            return itemGroup;
        }

        public void SaveTo(string path)
        {
            var project = Element("Project",
                new XAttribute("DefaultTargets", "Build"),
                new XAttribute("ToolsVersion", "15"));

            project.Add(CreateProjectConfigurations());
            project.Add(CreateGlobals());
            project.Add(ElementImport(AttributeProject(@"$(VCTargetsPath)\Microsoft.Cpp.Default.props")));
            project.Add(CreateConfigurationPropertyGroups());
            project.Add(ElementImport(AttributeProject(@"$(VCTargetsPath)\Microsoft.Cpp.props")));
            project.Add(ElementImportGroup(AttributeLabel("ExtensionSettings")));
            project.Add(ElementImportGroup(AttributeLabel("Shared")));
            project.Add(CreatePropertyGroups());
            project.Add(CreateFilesItemGroup());
            project.Add(ElementImport(AttributeProject(@"$(VCTargetsPath)\Microsoft.Cpp.targets")));
            project.Add(ElementImportGroup(AttributeLabel("ExtensionTargets")));

            var xmlDoc = new XDocument(project);
            xmlDoc.Save(path);
        }
    }
}
