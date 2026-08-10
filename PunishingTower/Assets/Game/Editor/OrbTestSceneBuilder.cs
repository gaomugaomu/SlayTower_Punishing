using PunishingTower.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PunishingTower.Editor
{
    /// <summary>
    /// Creates the manual orb test scene used from Unity Hub:
    /// a single GameObject carrying OrbTestDriver.
    /// </summary>
    public static class OrbTestSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/OrbTest.unity";

        [MenuItem("PunishingTower/OrbTest/Create OrbTest Scene")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var go = new GameObject("OrbTestDriver");
            go.AddComponent<OrbTestDriver>();

            Camera camera = CreateCamera();
            camera.transform.position = new Vector3(0, 0, -10);

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            if (!saved)
            {
                Debug.LogError("OrbTestSceneBuilder: failed to save scene " + ScenePath);
                return;
            }

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            Debug.Log("OrbTestSceneBuilder: scene created at " + ScenePath);
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
