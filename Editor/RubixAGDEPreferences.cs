using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;

namespace Rubix.Android.VisualStudio.AGDE.Editor
{
    class AGDEPreferences : SettingsProvider
    {
        internal static readonly string kSettingsPath = "Android/Rubix AGDE Visual Studio Extension";

        private static Dictionary<string, JdkInformation> m_JdkInformation = new Dictionary<string, JdkInformation>();

        private static string GetKey(string name)
        {
            return $"Rubix{nameof(AGDEPreferences)}.{name}";
        }

        internal static bool Enabled
        {
            set => EditorPrefs.SetBool(GetKey(nameof(Enabled)), value);
            get => EditorPrefs.GetBool(GetKey(nameof(Enabled)), true);
        }

        internal static class LaunchSettings
        {
            internal static bool UseUnityJdk 
            {
                set => EditorPrefs.SetBool(GetKey(nameof(UseUnityJdk)), value);
                get => EditorPrefs.GetBool(GetKey(nameof(UseUnityJdk)), true);
            }

            internal static string CustomJdkPath
            {
                set => EditorPrefs.SetString(GetKey(nameof(CustomJdkPath)), value);
                get
                {
                    var path = EditorPrefs.GetString(GetKey(nameof(CustomJdkPath)), string.Empty);
                    if (Application.platform == RuntimePlatform.WindowsEditor)
                        path = path.Replace("/", "\\");
                    return path;
                }
            }

            internal static string JdkPath => UseUnityJdk ? AndroidExternalToolsSettings.jdkRootPath : CustomJdkPath;
            internal static JdkInformation JdkInformation
            {
                get
                {
                    if (!Directory.Exists(JdkPath))
                        return null;

                    if (m_JdkInformation.TryGetValue(JdkPath, out var info))
                        return info;

                    Utilities.ParseJavaVersion(JdkPath, out var version, out var state, out var message);
                    var newInfo = new JdkInformation(version, state, message);
                    m_JdkInformation[JdkPath] = newInfo;
                    return newInfo;
                }
            }
        }

        internal AGDEPreferences(string path, SettingsScope scope)
            : base(path, scope)
        {
        }

        private void DoGeneralSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Launch Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            LaunchSettings.UseUnityJdk = EditorGUILayout.Toggle("Use Unity Jdk", LaunchSettings.UseUnityJdk);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();

            EditorGUI.BeginDisabledGroup(LaunchSettings.UseUnityJdk);
            var newJDKPath = EditorGUILayout.TextField("Custom Jdk Path", LaunchSettings.JdkPath);
            if (!LaunchSettings.UseUnityJdk && GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
            {
                var folder = EditorUtility.OpenFolderPanel("Select JDK 17 directory", LaunchSettings.CustomJdkPath, string.Empty);
                if (!string.IsNullOrEmpty(folder))
                {
                    GUIUtility.keyboardControl = 0;
                    newJDKPath = folder;
                }
            }
            EditorGUI.EndDisabledGroup();

            if (LaunchSettings.UseUnityJdk && GUILayout.Button("Copy Path", GUILayout.ExpandWidth(false)))
                GUIUtility.systemCopyBuffer = LaunchSettings.JdkPath;

            if (EditorGUI.EndChangeCheck())
                LaunchSettings.CustomJdkPath = newJDKPath;
            EditorGUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(LaunchSettings.JdkPath))
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox($"Please provide path to Jdk which version is at least {JdkInformation.MinAGDEJdkVersion}.", MessageType.Info);
            }

            var jdkInformation = LaunchSettings.JdkInformation;
            if (jdkInformation != null)
            {
                if (jdkInformation.State == JdkInformation.JdkState.Error)
                    EditorGUILayout.HelpBox(jdkInformation.Message, MessageType.Error);
                else
                {
                    EditorGUILayout.LabelField("Jdk Version", jdkInformation.Version.ToString());

                    if (jdkInformation.State == JdkInformation.JdkState.Warning)
                        EditorGUILayout.HelpBox(jdkInformation.Message, MessageType.Warning);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox($"Once you export Gradle project, open Visual Studio AGDE solution by double clicking on {Constants.OpenVisualStudioAGDEFileName}.", MessageType.Info);
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
            Enabled = EditorGUILayout.Toggle(nameof(Enabled), Enabled);

            EditorGUI.BeginDisabledGroup(!Enabled);
            DoGeneralSettings();
            EditorGUI.EndDisabledGroup();
        }

        [SettingsProvider]
        public static SettingsProvider CreateAGDEProvider()
        {
            var provider = new AGDEPreferences(kSettingsPath, SettingsScope.User);
            return provider;
        }
    }
}
