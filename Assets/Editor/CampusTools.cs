using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CadetCampus.Editor
{
    public static class CampusTools
    {
        public const string ScenePath = "Assets/Scenes/Campus.unity";

        [MenuItem("Tools/Cadet Campus/Open campus scene")]
        public static void OpenCampus()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(ScenePath);
        }

        // This runs only in the Unity Editor; reference-only C# compilation is separate.
        [MenuItem("Tools/Cadet Campus/Validate project")]
        public static void ValidateProject()
        {
            if (!File.Exists(ScenePath)) throw new InvalidOperationException("Campus scene is missing.");
            var scene = EditorSceneManager.OpenScene(ScenePath);
            int bootstrapCount = 0;
            foreach (var root in scene.GetRootGameObjects())
                bootstrapCount += root.GetComponentsInChildren<CampusGame>(true).Length;
            if (bootstrapCount != 1) throw new InvalidOperationException("The scene needs exactly one CampusGame bootstrap.");
            if (CampusGame.Circuit.Length != CampusRules.CheckpointCount)
                throw new InvalidOperationException("Circuit markers and rule count do not match.");
            if (!Shader.Find("Standard")) throw new InvalidOperationException("Use the Built-in Render Pipeline for this project.");
            EditorBuildSettings.scenes = new [] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log("Campus project validation passed: scene, bootstrap, circuit and Standard shader.");
        }

        [MenuItem("Tools/Cadet Campus/Build Linux game")]
        public static void BuildLinux()
        {
            Build(BuildTarget.StandaloneLinux64, "Builds/Linux/CadetAfterHours.x86_64");
        }

        [MenuItem("Tools/Cadet Campus/Build Windows game")]
        public static void BuildWindows()
        {
            Build(BuildTarget.StandaloneWindows64, "Builds/Windows/CadetAfterHours.exe");
        }

        static void Build(BuildTarget target, string output)
        {
            ValidateProject();
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            PlayerSettings.companyName = "Cadet Campus";
            PlayerSettings.productName = "Cadet After Hours";
            PlayerSettings.defaultScreenWidth = 1440;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.runInBackground = false;
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new [] { ScenePath },
                target = target,
                locationPathName = output,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Unity build failed: " + report.summary.result);
            Debug.Log("Built Cadet After Hours: " + output);
        }
    }
}
