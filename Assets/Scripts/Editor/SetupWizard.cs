#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

namespace XCAPE.Editor
{
    public class SetupWizard : EditorWindow
    {
        [MenuItem("Tools/XCAPE/Setup Wizard")]
        public static void ShowWindow()
        {
            GetWindow<SetupWizard>(false, "XCAPE Setup", true);
        }

        private void OnGUI()
        {
            GUILayout.Label("XCAPE Project Setup", EditorStyles.boldLabel);
            if (GUILayout.Button("Apply Recommended Settings"))
            {
                ApplySettings();
            }
            if (GUILayout.Button("Create Default Scenes + Build Settings"))
            {
                CreateDefaultScenes();
                SetupBuildSettings();
            }
            if (GUILayout.Button("Switch Platform to Android"))
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                EditorUtility.DisplayDialog("XCAPE", "Switched active build target to Android.", "OK");
            }
        }

        private void ApplySettings()
        {
            PlayerSettings.companyName = "XCapeStudio";
            PlayerSettings.productName = "XCAPE Bowling";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            if (!defines.Contains("ADMOB_ENABLED")) defines += ";ADMOB_ENABLED";
            if (!defines.Contains("XCAPE_MOBILE")) defines += ";XCAPE_MOBILE";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defines);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, defines);

            EditorUtility.DisplayDialog("XCAPE", "Recommended settings applied.", "OK");
        }

        private void CreateDefaultScenes()
        {
            Directory.CreateDirectory("Assets/Scenes/MainMenu");
            Directory.CreateDirectory("Assets/Scenes/GamePlay");

            // Main Menu Scene
            var mainMenuPath = "Assets/Scenes/MainMenu/MainMenu.unity";
            if (!File.Exists(mainMenuPath))
            {
                var menuScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                menuScene.name = "MainMenu";
                CreateOrFindObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
                CreateOrFindObject("UI Canvas", typeof(Canvas));
                EditorSceneManager.SaveScene(menuScene, mainMenuPath);
            }

            // Gameplay Scene
            var gameplayPath = "Assets/Scenes/GamePlay/GamePlay.unity";
            if (!File.Exists(gameplayPath))
            {
                var gameScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                gameScene.name = "GamePlay";
                CreateOrFindObject("GameManager", typeof(XCAPE.Core.GameManager));
                EditorSceneManager.SaveScene(gameScene, gameplayPath);
            }

            EditorUtility.DisplayDialog("XCAPE", "Default scenes created (MainMenu, GamePlay).", "OK");
        }

        private void SetupBuildSettings()
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            void AddScene(string p)
            {
                if (File.Exists(p)) scenes.Add(new EditorBuildSettingsScene(p, true));
            }
            AddScene("Assets/Scenes/MainMenu/MainMenu.unity");
            AddScene("Assets/Scenes/GamePlay/GamePlay.unity");
            EditorBuildSettings.scenes = scenes.ToArray();
            EditorUtility.DisplayDialog("XCAPE", "Build Settings updated with default scenes.", "OK");
        }

        private static GameObject CreateOrFindObject(string name, System.Type type)
        {
            var go = GameObject.Find(name);
            if (!go)
            {
                go = new GameObject(name);
            }
            if (type != null && go.GetComponent(type) == null)
            {
                go.AddComponent(type);
            }
            return go;
        }
    }
}
#endif
