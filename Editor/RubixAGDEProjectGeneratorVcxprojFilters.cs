using System;
using System.IO;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Rubix.Android.VisualStudio.AGDE.Editor
{
    class AGDEProjectGeneratorVcxprojFilters : AGDEProjectGeneratorBase
    {
        internal AGDEProjectGeneratorVcxprojFilters(AGDECreateArguments createArgs) 
            : base(createArgs)
        {
        }

        private string PathToGuid(MD5 md5, string path)
        {
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(path));
            return new Guid(hash).ToString();
        }

        private XElement CreateFilters()
        {
            using var md5 = MD5.Create();

            var itemGroup = ElementItemGroup();
            foreach (var directory in CreateArgs.GradleProjectDirectories)
            {
                var filter = Element("Filter", AttributeInclude(directory));
                //filter.Add(Element("Extensions", "cpp;c;cc;cxx;def;odl;idl;hpj;bat;asm;asmx"));
                filter.Add(Element("UniqueIdentifier", $"{{{PathToGuid(md5, directory)}}}"));
                itemGroup.Add(filter);
            }

            return itemGroup;
        }

        private string GetIncludeName(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            /*
            if (extension.Equals(".cpp", System.StringComparison.InvariantCultureIgnoreCase) ||
                extension.Equals(".c", System.StringComparison.InvariantCultureIgnoreCase))
                return "ClCompile";
            if (extension.Equals(".h", System.StringComparison.InvariantCultureIgnoreCase) ||
                extension.Equals(".hpp", System.StringComparison.InvariantCultureIgnoreCase))
                return "ClInclude";
            */
            return "None";
        }

        private XElement CreateFileFilters()
        {
            var itemGroup = ElementItemGroup();
            foreach (var file in CreateArgs.GradleProjectFiles)
            {
                var item = Element(GetIncludeName(file),
                        AttributeInclude(file));

                var directory = Path.GetDirectoryName(file);
                if (directory.Length >= CreateArgs.PrefixIncludeFile.Length)
                    directory = directory.Substring(CreateArgs.PrefixIncludeFile.Length);
                if (!string.IsNullOrEmpty(directory))
                    item.Add(Element("Filter", directory));

                itemGroup.Add(item);
            }

            return itemGroup;
        }

        public void SaveTo(string path)
        {
            var project = Element("Project",
                new XAttribute("ToolsVersion", "4.0"));

            project.Add(CreateFilters());
            project.Add(CreateFileFilters());
            var xmlDoc = new XDocument(project);
            xmlDoc.Save(path);
        }
    }
}
