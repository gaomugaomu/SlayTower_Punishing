using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PunishingTower.Editor
{
    /// <summary>
    /// Windows build entry (doc 89): build the playable demo with both test scenes.
    /// Usage: Unity -batchmode -executeMethod PunishingTower.Editor.WindowsBuildBuilder.Build
    /// </summary>
    public static class WindowsBuildBuilder
    {
        public const string BuildPath = "Builds/Windows/PunishingTower.exe";

        [MenuItem("PunishingTower/Build/Windows Demo")]
        public static void Build()
        {
            string[] scenes =
            {
                OrbTestSceneBuilder.ScenePath,
                CombatTestSceneBuilder.ScenePath
            };

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = BuildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"WindowsBuildBuilder: build succeeded at {BuildPath}");
            }
            else
            {
                Debug.LogError($"WindowsBuildBuilder: build failed - {report.summary.result}");
            }
        }
    }
}
