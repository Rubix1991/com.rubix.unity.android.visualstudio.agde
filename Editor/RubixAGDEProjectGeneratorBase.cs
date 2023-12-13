using System.Xml.Linq;

namespace Rubix.Android.VisualStudio.AGDE.Editor
{
    abstract class AGDEProjectGeneratorBase
    {
        protected XNamespace Namespace => @"http://schemas.microsoft.com/developer/msbuild/2003";

        protected AGDECreateArguments CreateArgs { get; }

        protected AGDEProjectGeneratorBase(AGDECreateArguments createArgs)
        {
            CreateArgs = createArgs;
        }

        protected XElement Element(string name, params object[] objects)
        {
            return new XElement(Namespace + name, objects);
        }

        protected XElement ElementItemGroup(params object[] objects)
        {
            return Element("ItemGroup", objects);
        }

        protected XElement ElementPropertyGroup(params object[] objects)
        {
            return Element("PropertyGroup", objects);
        }

        protected XElement ElementImport(params object[] objects)
        {
            return Element("Import", objects);
        }

        protected XElement ElementImportGroup(params object[] objects)
        {
            return Element("ImportGroup", objects);
        }

        protected XAttribute AttributeLabel(string value)
        {
            return new XAttribute("Label", value);
        }

        protected XAttribute AttributeProject(string value)
        {
            return new XAttribute("Project", value);
        }

        protected XAttribute AttributeCondition(string value)
        {
            return new XAttribute("Condition", value);
        }

        protected XAttribute AttributeInclude(string value)
        {
            return new XAttribute("Include", value);
        }
    }
}
