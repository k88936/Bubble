using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DemoGame.Editor
{
    public class BuildScript : MonoBehaviour
    {
        private const string DEBUG_FLAG = "[Game Builder]";
        private const string INVALID_FLAG = "INVALID";
        private const string ApplicationName = "DemoGame";
        private const string OutputBasePath = "Build";
        private const string OutputPath_Android = "Android";
        private const string OutputPath_Windows = "Windows";
        
        [MenuItem("Build/Build Android APK (IL2CPP)")]
        public static void PerformBuild_AndroidAPK()
        {
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            PerformBuild(BuildTarget.Android, BuildTargetGroup.Android,
                ScriptingImplementation.IL2CPP, $"{OutputPath_Android}/{ApplicationName}.apk", bCleanBuild: true,
                bOutputIsFolderTarget: false);
        }

        [MenuItem("Build/Build Windows (IL2CPP)")]
        public static void PerformBuild_Windows()
        {
            PerformBuild(BuildTarget.StandaloneWindows64, BuildTargetGroup.Standalone,
                ScriptingImplementation.IL2CPP, $"{OutputPath_Windows}/{ApplicationName}.exe", bCleanBuild: true,
                bOutputIsFolderTarget: false);
        }

        [MenuItem("Build/Export Android Project (IL2CPP)")]
        public static void PerformBuild_AndroidProject()
        {
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
            PerformBuild(BuildTarget.Android, BuildTargetGroup.Android,
                ScriptingImplementation.IL2CPP, $"{OutputPath_Android}/{ApplicationName}", bCleanBuild: true,
                bOutputIsFolderTarget: true);
        }

        public static void PerformBuild(BuildTarget TargetPlatform, BuildTargetGroup TargetGroup,
            ScriptingImplementation BackendScriptImpl, string OutputTarget, bool bCleanBuild = true,
            bool bOutputIsFolderTarget = true)
        {
            if (bCleanBuild)
            {
                DeletePlatformBuildFolder(TargetPlatform);
            }
            
            var buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetBuildSceneList();
            buildPlayerOptions.locationPathName = GetOutputTarget(TargetPlatform, OutputTarget, bOutputIsFolderTarget);
            buildPlayerOptions.target = TargetPlatform;
            buildPlayerOptions.options = BuildOptions.CleanBuildCache;
            PlayerSettings.SetScriptingBackend(TargetGroup, BackendScriptImpl);

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
                Debug.Log($"{DEBUG_FLAG} Build succeed, size: {summary.totalSize} bytes");

            if (summary.result == BuildResult.Failed) Debug.Log($"{DEBUG_FLAG} Build failed");
        }

        private static string GetPlatformOutputFolder(BuildTarget TargetPlatform)
        {
            switch (TargetPlatform)
            {
                case BuildTarget.Android:
                    return $"{OutputBasePath}/{OutputPath_Android}";
                case BuildTarget.StandaloneWindows64:
                    return $"{OutputBasePath}/{OutputPath_Windows}";
            }

            return INVALID_FLAG;
        }
        
        private static void DeletePlatformBuildFolder(BuildTarget TargetPlatform)
        {
            string platformOutputPath = GetPlatformOutputFolder(TargetPlatform);
            string platformOutputFullPath =
                platformOutputPath != INVALID_FLAG ? Path.GetFullPath(platformOutputPath) : INVALID_FLAG;
            if (Directory.Exists(platformOutputFullPath)) Directory.Delete(platformOutputFullPath, true);
        }

        private static string GetOutputTarget(BuildTarget TargetPlatform, string TargetPath,
            bool bTargetIsFolder = true)
        {
            string PlatformOutFolder = GetPlatformOutputFolder(TargetPlatform);
            string resultPath = Path.Combine(OutputBasePath, TargetPath);
            Debug.Log(
                $"{DEBUG_FLAG} result path: {resultPath}, platformFolder: {PlatformOutFolder}, platform fullPath:{Path.GetFullPath(PlatformOutFolder)}");
            if (!Directory.Exists(Path.GetFullPath(PlatformOutFolder))) Directory.CreateDirectory(PlatformOutFolder);
#if UNITY_IOS
            if (!Directory.Exists($"{resultPath}/Unity-iPhone/Images.xcassets/LaunchImage.launchimage"))
            {
                Directory.CreateDirectory($"{resultPath}/Unity-iPhone/Images.xcassets/LaunchImage.launchimage");
            }
#endif
            return resultPath;
        }

        private static string[] GetBuildSceneList()
        {
            List<string> sceneList = new List<string>()
            {
                "Assets/Scene/SampleScene.unity"
            };
            return sceneList.ToArray();
        }
        
        [MenuItem("Build/Print Debug Info", priority = 100)]
        public static void PrintDebugInfo()
        {
            foreach (var scene_name in GetBuildSceneList())
            {
                Debug.Log($"{DEBUG_FLAG} Pre Build Scene: {scene_name}");
            }
        }
    }
}