using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Rubix.Android.VisualStudio.AGDE.Editor
{
    internal class RubixAGDEBuildProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => int.MaxValue;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (!AGDEPreferences.Enabled)
                return;

            if (report.summary.platform != BuildTarget.Android)
                return;

            if (!report.summary.options.HasFlag(BuildOptions.AcceptExternalModificationsToPlayer) &&
                !EditorUserBuildSettings.exportAsGoogleAndroidProject)
                return;

            var name = Utilities.ReplaceInvalidCharsInFileName(PlayerSettings.productName);

            var createArgs = new AGDECreateArguments(
                name,
                gradleProjectPath: report.summary.outputPath
            );

            File.Copy(Utilities.ResolvePackageAssetPath("Gradle~/gradlew.bat"),
                Path.Combine(createArgs.GradleProjectPath, "gradlew.bat"), true);
            File.Copy(Utilities.ResolvePackageAssetPath("Gradle~/gradle-wrapper.jar"),
                Path.Combine(createArgs.GradleProjectPath, "gradle/wrapper/gradle-wrapper.jar"), true);

            var vcxproject = new AGDEProjectGeneratorVcxproj(createArgs);
            var vcxprojectFilters = new AGDEProjectGeneratorVcxprojFilters(createArgs);
            var vcxprojectUser = new AGDEProjectGeneratorVcxprojUser(createArgs);
            var solution = new AGDEProjectGeneratorSolution(createArgs);

            var vsPath = createArgs.VisualStudioDirectory;
            vcxproject.SaveTo(Path.Combine(vsPath, $"{name}.vcxproj"));
            vcxprojectFilters.SaveTo(Path.Combine(vsPath, $"{name}.vcxproj.filters"));
            vcxprojectUser.SaveTo(Path.Combine(vsPath, $"{name}.vcxproj.user"));
            solution.SaveTo(Path.Combine(vsPath, $"{name}.sln"));

            var openSolutionCmd = Path.Combine(createArgs.GradleProjectPath, Constants.OpenVisualStudioAGDEFileName);

            var openSolutionCmdContents = new StringBuilder();
            openSolutionCmdContents.AppendLine($"set ANDROID_SDK_ROOT={createArgs.AndroidSDK.Path}");
            if (!string.IsNullOrEmpty(AGDEPreferences.LaunchSettings.JdkPath))
                openSolutionCmdContents.AppendLine($"set AGDE_JAVA_HOME={AGDEPreferences.LaunchSettings.JdkPath}");
            if (!string.IsNullOrEmpty(AndroidExternalToolsSettings.jdkRootPath))
                openSolutionCmdContents.AppendLine($"set JAVA_HOME={AndroidExternalToolsSettings.jdkRootPath}");
            openSolutionCmdContents.AppendLine(@$"""{createArgs.VisualStudioRelativeDirectory}\{name}.sln""");

            File.WriteAllText(openSolutionCmd, openSolutionCmdContents.ToString());
        }
    }
}
