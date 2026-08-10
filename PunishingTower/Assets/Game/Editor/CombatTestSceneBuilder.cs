using PunishingTower.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PunishingTower.Editor
{
    /// <summary>
    /// Creates the manual combat test scene used from Unity Hub:
    /// a single GameObject carrying CombatTestDriver.
    /// </summary>
    public static class CombatTestSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/CombatTest.unity";

        [MenuItem("PunishingTower/CombatTest/Create CombatTest Scene")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var go = new GameObject("CombatTestDriver");
            go.AddComponent<CombatTestDriver>();

            Camera camera = CreateCamera();
            camera.transform.position = new Vector3(0, 0, -10);

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            if (!saved)
            {
                Debug.LogError("CombatTestSceneBuilder: failed to save scene " + ScenePath);
                return;
            }

            AddSceneToBuildSettings(ScenePath);
            AddSceneToBuildSettings(OrbTestSceneBuilder.ScenePath);

            Debug.Log("CombatTestSceneBuilder: scene created at " + ScenePath);
        }

        private static void AddSceneToBuildSettings(string path)
        {
            var existing = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool found = false;
            foreach (EditorBuildSettingsScene scene in existing)
            {
                if (scene.path == path)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                existing.Add(new EditorBuildSettingsScene(path, true));
                EditorBuildSettings.scenes = existing.ToArray();
            }
        }

        private static Camera CreateCamera()
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            var camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.12f, 0.16f);
            go.AddComponent<AudioListener>();
            return camera;
        }
    }
}
