using System.IO;
using System.Text;

namespace Rubix.Android.VisualStudio.AGDE.Editor
{
    class AGDEProjectGeneratorSolution : AGDEProjectGeneratorBase
    {
        internal AGDEProjectGeneratorSolution(AGDECreateArguments args) : base(args) { }

        internal void SaveTo(string path)
        {
            var contents = new StringBuilder();
            contents.AppendLine(
@$"Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.8.34330.188
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{{{CreateArgs.VCXProjectReferenceGuid}}}"") = ""{CreateArgs.Name}"", ""{CreateArgs.Name}.vcxproj"", ""{{{CreateArgs.VCXProjectGuid}}}""
EndProject
Global
    GlobalSection(SolutionConfigurationPlatforms) = preSolution");

            foreach (var c in CreateArgs.EnumerateConfigurations())
            {
                foreach (var p in CreateArgs.EnumeratePlatforms())
                {
                    contents.AppendLine($"        {c}|{p} = {c}|{p}");
                }
            }

            contents.AppendLine(
@"    EndGlobalSection
    GlobalSection(ProjectConfigurationPlatforms) = postSolution");

            foreach (var c in CreateArgs.EnumerateConfigurations())
            {
                foreach (var p in CreateArgs.EnumeratePlatforms())
                {
                    contents.AppendLine($"        {{{CreateArgs.VCXProjectGuid}}}.{c}|{p}.ActiveCfg = {c}|{p}");
                    contents.AppendLine($"        {{{CreateArgs.VCXProjectGuid}}}.{c}|{p}.Build.0 = {c}|{p}");
                }
            }
            contents.AppendLine(
@$"    EndGlobalSection
    GlobalSection(SolutionProperties) = preSolution
        HideSolutionNode = FALSE
    EndGlobalSection
    GlobalSection(ExtensibilityGlobals) = postSolution
        SolutionGuid = {{{CreateArgs.SolutionGuid}}}
    EndGlobalSection
EndGlobal
");
            File.WriteAllText(path, contents.ToString());
        }
    }
}
