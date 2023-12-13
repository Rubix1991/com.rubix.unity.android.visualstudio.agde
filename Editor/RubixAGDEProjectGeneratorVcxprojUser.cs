using System.Collections.Generic;
using System.Xml.Linq;

namespace Rubix.Android.VisualStudio.AGDE.Editor
{
    class AGDEProjectGeneratorVcxprojUser : AGDEProjectGeneratorBase
    {
        internal AGDEProjectGeneratorVcxprojUser(AGDECreateArguments createArgs) 
            : base(createArgs)
        {
        }

        private IEnumerable<XElement> CreatePropertyGroups()
        {
            // Disable signals used by Unity's managed debugger
            var lldbPostAttach =
@"process handle SIGXCPU -n true -p true -s false
process handle SIGPWR -n true -p true -s false";
            foreach (var c in CreateArgs.EnumerateConfigurations())
            {
                foreach (var p in CreateArgs.EnumeratePlatforms())
                {
                    var propertyGroup = ElementPropertyGroup(
                        AttributeCondition($"'$(Configuration)|$(Platform)'=='{c}|{p}'"));
                    propertyGroup.Add(Element("AndroidLldbPostAttachCommands", lldbPostAttach));
                    propertyGroup.Add(Element("DebuggerFlavor", "GoogleAndroidDebugger"));
                    // This is inconvient that we need to specify ABI, since if apk holds more than one ABI
                    // We don't know which will be used at runtime
                    // For Android Studio, it's enough to specify folder without ABI, the ABI is appended automatically
                    propertyGroup.Add(Element("AndroidSymbolDirectories", $"{CreateArgs.PrefixIncludeFile}unityLibrary\\symbols\\{p}"));
                    yield return propertyGroup;
                }
            }
        }

        internal void SaveTo(string path)
        {
            var project = Element("Project",
                new XAttribute("ToolsVersion", "Current"));
            project.Add(CreatePropertyGroups());
            var xmlDoc = new XDocument(project);
            xmlDoc.Save(path);
        }
    }
}
