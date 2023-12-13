using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Unity.Android.Types;
using UnityEditor.Android;

namespace Rubix.Android.VisualStudio.AGDE.Editor
{
    internal class AndroidTool
    {
        public string Path { get; }
        public string Version { get; }

        internal AndroidTool(string path)
        {
            Path = path;
            Version = Utilities.ParseAndroidToolVersion(path);
        }
    }

    internal class AGDECreateArguments
    {
        public string Name { get; }
        public string GradleProjectPath { get; }
        public IReadOnlyList<string> GradleProjectFiles { get; private set; }
        public IReadOnlyList<string> GradleProjectDirectories { get; private set; }
        public string ApplicationModule => "launcher";
        public int MinSdkVersion => (int)PlayerSettings.Android.minSdkVersion;
        public string PackageName
        {
            get
            {
#if UNITY_2023_1_OR_NEWER
                var target = UnityEditor.Build.NamedBuildTarget.Android;
#else
                var target = BuildTargetGroup.Android;
#endif

                return PlayerSettings.GetApplicationIdentifier(target);
            }
        }

        public string LaunchActivity
        {
            get
            {
#if UNITY_2023_1_OR_NEWER
                var e = PlayerSettings.Android.applicationEntry;
                if (e.HasFlag(UnityEditor.AndroidApplicationEntry.Activity))
                    return "com.unity3d.player.UnityPlayerActivity";
                if (e.HasFlag(UnityEditor.AndroidApplicationEntry.GameActivity))
                    return "com.unity3d.player.UnityPlayerGameActivity";
#endif
                return "com.unity3d.player.UnityPlayerActivity";
            }
        }
        public string VCXProjectGuid => "A94A179B-CBF3-45D0-B3DA-0CC6F8DFC9AA";
        public string VCXProjectReferenceGuid => "8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942";
        public string SolutionGuid => "E8AC4F6F-E1A1-411E-8C7A-9571F9EDDBFE";
        public string VisualStudioRelativeDirectory => "VisualStudioAGDE";
        public string VisualStudioDirectory => Path.Combine(GradleProjectPath, VisualStudioRelativeDirectory);
        public string PrefixIncludeFile { get; }
        public AndroidTool AndroidSDK { get; }
        public AndroidTool AndroidNDK { get; }

        public AGDECreateArguments(string name, string gradleProjectPath)
        {
            Name = name;
            GradleProjectPath = gradleProjectPath;

            AndroidSDK = new AndroidTool(AndroidExternalToolsSettings.sdkRootPath);
            AndroidNDK = new AndroidTool(AndroidExternalToolsSettings.ndkRootPath);

            Directory.CreateDirectory(VisualStudioDirectory);

            var prefixIncludeFile = Utilities.CalculateRelativePath(VisualStudioDirectory, gradleProjectPath);
            if (!string.IsNullOrEmpty(prefixIncludeFile))
                prefixIncludeFile += "\\";
            PrefixIncludeFile = prefixIncludeFile;
            CollectGradleProjectPaths();
        }

        private void CollectGradleProjectPaths()
        {
            var gradleProjectFiles = new List<string>();
            
            var files = Directory.GetFiles(GradleProjectPath, "*.*", SearchOption.AllDirectories);

            bool ShouldSkip(string path)
            {
                if (path[0] == '.' ||
                    path.Contains("\\."))
                    return true;

                if (path.Contains(VisualStudioRelativeDirectory))
                    return true;

                // Skip build folders, don't skip build.gradle
                if (path.Contains("\\build\\") ||
                    path.EndsWith("build"))
                    return true;

                if (path.EndsWith(".sln") ||
                    path.EndsWith(".vcxproj") ||
                    path.EndsWith(".vcxproj.filters") ||
                    path.EndsWith(".vcxproj.user"))
                    return true;
                return false;
            }

            foreach (var file in files)
            {
                var relativePath = file.Substring(GradleProjectPath.Length + 1);
                if (ShouldSkip(relativePath))
                    continue;

                gradleProjectFiles.Add(PrefixIncludeFile + relativePath);

                var directory = Path.GetDirectoryName(relativePath);
                if (string.IsNullOrEmpty(directory))
                    continue;
                
            }

            GradleProjectFiles = gradleProjectFiles;
            

            var gradleProjectDirectories = new HashSet<string>();
            var directories = Directory.GetDirectories(GradleProjectPath, "*.*", SearchOption.AllDirectories);
            foreach (var directory in directories)
            {
                var relativePath = directory.Substring(GradleProjectPath.Length + 1);
                if (ShouldSkip(relativePath))
                    continue;

                gradleProjectDirectories.Add(relativePath);
            }
            GradleProjectDirectories = gradleProjectDirectories.ToList();
        }

        internal IEnumerable<string> EnumerateConfigurations()
        {
            yield return "Debug";
            yield return "Release";
        }

        internal IEnumerable<string> EnumeratePlatforms()
        {
            var targetArchitectures = (Unity.Android.Types.AndroidArchitecture)PlayerSettings.Android.targetArchitectures;
            foreach (var deviceType in AndroidTargetDeviceType.AllSupported)
            {
                if (!targetArchitectures.HasFlag(deviceType.TargetArchitecture))
                    continue;
                yield return $"Android-{deviceType.ABI}";
            }
        }
    }
}
