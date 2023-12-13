using System;
using System.IO;
using System.Runtime.Remoting.Contexts;
using System.Text.RegularExpressions;
using static UnityEngine.Networking.UnityWebRequest;

namespace Rubix.Android.VisualStudio.AGDE.Editor
{
    internal class Utilities
    {
        internal static readonly string PackagePathPrefix = "Packages/com.rubix.unity.android.visualstudio.agde";

        internal static string ResolvePackageAssetPath(string path)
        {
            var info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(@$"{PackagePathPrefix}/Gradle/gradlew.bat");
            if (info == null)
                throw new Exception($"Failed to resolve {path}");
            return Path.GetFullPath(Path.Combine(info.resolvedPath, path));
        }

        internal static string ReplaceInvalidCharsInFileName(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        internal static string CalculateRelativePath(string parent, string child)
        {
            var childUri = new Uri(Path.GetFullPath(child));
            var parentUri = new Uri(Path.GetFullPath(parent));
            
            return Path.GetDirectoryName(Uri.UnescapeDataString(
                parentUri.MakeRelativeUri(childUri)
                    .ToString()
                    .Replace('/', Path.DirectorySeparatorChar)
                ));
        }

        internal static bool ParseJavaVersion(string jdkPath, out Version version, out JdkInformation.JdkState state, out string message)
        {
            version = new Version(0, 0, 0);
            var releasePath = Path.Combine(jdkPath, "release");
            var errorPrefix = $"Failed to determine Java version in '{jdkPath}'\n";
            if (!File.Exists(releasePath))
            {
                state = JdkInformation.JdkState.Error;
                message = $"{errorPrefix}Failed to find '{releasePath}'";
                return false;
            }

            var contents = File.ReadAllText(releasePath);
            var regex = new Regex(@"JAVA_VERSION\s*=\s*""(?<version>.*)""");
            var result = regex.Match(contents);
            if (result.Success)
            {
                if (Version.TryParse(result.Groups["version"].Value, out var v))
                {
                    version = v;
                    if (version < JdkInformation.MinAGDEJdkVersion)
                    {
                        state = JdkInformation.JdkState.Warning;
                        message = $"Minimum AGDE Jdk version is {JdkInformation.MinAGDEJdkVersion}, your Jdk version is {version}.";
                    }
                    else
                    {
                        state = JdkInformation.JdkState.Success;
                        message = string.Empty;
                    }
                    return true;
                }
                state = JdkInformation.JdkState.Error;
                message = $"{errorPrefix}Failed to parse java version from:\n{result.Groups["version"].Value}";
                return false;
            }

            state = JdkInformation.JdkState.Error;
            message = $"{errorPrefix}Failed to parse java version from '{releasePath}' :\n{contents}";
            return false;
        }

        internal static string ParseAndroidToolVersion(string toolPath)
        {
            var sourceProperties = Path.Combine(toolPath, "source.properties");
            if (!File.Exists(sourceProperties))
                return string.Empty;

            var contents = File.ReadAllText(sourceProperties);
            var regex = new Regex(@"Pkg.Revision\s*=\s*(?<version>.*)");
            var result = regex.Match(contents);
            if (result.Success)
                return result.Groups["version"].Value;
            return string.Empty;
        }
    }
}
