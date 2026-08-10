using PunishingTower.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PunishingTower.Editor
{
    /// <summary>
    /// Builds the UGUI combat scene: 3D placeholder stage, BattleHud (Canvas), CombatTestDriver.
    /// </summary>
    public static class CombatUiSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/CombatUiTest.unity";

        [MenuItem("PunishingTower/CombatTest/Create CombatUiTest Scene")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera (angled view, Slay the Spire feel).
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
            cameraGo.transform.position = new Vector3(0, 3.2f, -8f);
            cameraGo.transform.rotation = Quaternion.Euler(14f, 0f, 0f);
            cameraGo.AddComponent<AudioListener>();

            // Placeholder stage: ground + simple silhouettes for the squad and enemies.
            BuildStage();

            // Logic driver + HUD.
            var driverGo = new GameObject("CombatDriver");
            var driver = driverGo.AddComponent<CombatTestDriver>();

            var hudGo = new GameObject("BattleHud");
            var hud = hudGo.AddComponent<BattleHud>();
            var so = new SerializedObject(hud);
            so.FindProperty("driver").objectReferenceValue = driver;
            so.ApplyModifiedPropertiesWithoutUndo();

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            if (!saved)
            {
                Debug.LogError("CombatUiSceneBuilder: failed to save scene " + ScenePath);
                return;
            }

            AddSceneToBuildSettings(ScenePath);
            Debug.Log("CombatUiSceneBuilder: scene created at " + ScenePath);
        }

        private static void BuildStage()
        {
            // Ground.
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0, -0.5f, 0);
            ground.transform.localScale = new Vector3(20, 1, 12);
            ground.GetComponent<Renderer>().material.color = new Color(0.16f, 0.17f, 0.22f);

            // Squad silhouettes (left side): 3 capsule figures.
            string[] names = { "Lucia", "Lee", "Liv" };
            Color[] colors = { new Color(0.8f, 0.3f, 0.3f), new Color(0.3f, 0.5f, 0.85f), new Color(0.4f, 0.8f, 0.5f) };
            for (int i = 0; i < names.Length; i++)
            {
                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Construct_" + names[i];
                body.transform.position = new Vector3(-5f + i * 1.6f, 0.8f, 1.5f);
                body.transform.localScale = new Vector3(0.9f, 1.4f, 0.9f);
                body.GetComponent<Renderer>().material.color = colors[i];

                var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                head.name = "Head";
                head.transform.SetParent(body.transform, false);
                head.transform.localPosition = new Vector3(0, 0.85f, 0);
                head.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                head.GetComponent<Renderer>().material.color = colors[i];
            }

            // Enemy silhouettes (right side).
            Color[] enemyColors =
            {
                new Color(0.75f, 0.3f, 0.3f), // infected unit
                new Color(0.55f, 0.55f, 0.65f)  // defensive machine
            };
            for (int i = 0; i < enemyColors.Length; i++)
            {
                var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "Enemy_" + i;
                body.transform.position = new Vector3(3.5f + i * 2.2f, 0.8f, 1.5f);
                body.transform.localScale = new Vector3(1.5f, 1.6f, 1.5f);
                body.GetComponent<Renderer>().material.color = enemyColors[i];

                var eye = GameObject.CreatePrimitive(PrimitiveType.Cube);
                eye.name = "Eye";
                eye.transform.SetParent(body.transform, false);
                eye.transform.localPosition = new Vector3(0, 0.2f, 0.76f);
                eye.transform.localScale = new Vector3(0.8f, 0.3f, 0.1f);
                eye.GetComponent<Renderer>().material.color = new Color(1f, 0.9f, 0.3f);
            }
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
    }
}
