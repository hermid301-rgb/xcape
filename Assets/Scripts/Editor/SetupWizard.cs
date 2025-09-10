#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
#if UNITY_RENDER_PIPELINE_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif
using UnityEditor.Presets;
using UnityEditor.Experimental.SceneManagement;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditorInternal.VR;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

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
            if (GUILayout.Button("Create WIRED Gameplay Scene (auto-setup)"))
            {
                CreateWiredGameplayScene();
                SetupBuildSettings();
            }
            if (GUILayout.Button("Apply Physics & Materials Tuning"))
            {
                ApplyPhysicsAndMaterialsTuning();
            }
            if (GUILayout.Button("Ensure Unity Services & Netcode Packages"))
            {
                EnsureUnityPackages();
            }
            if (GUILayout.Button("Open Services Project Settings"))
            {
                OpenServicesSettings();
            }
            if (GUILayout.Button("Add Lobby Panel to Current Scene"))
            {
                AddLobbyPanelToCurrentScene();
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
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_Unity_4_8);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.iOS, ApiCompatibilityLevel.NET_Unity_4_8);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan, GraphicsDeviceType.OpenGLES3 });
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.iOS, true);
            PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, true);
            PlayerSettings.SetMobileMTRendering(BuildTargetGroup.iOS, true);
            PlayerSettings.SetPropertyInt("ScriptingBackend", (int)ScriptingImplementation.IL2CPP, BuildTargetGroup.Android);
            PlayerSettings.SetPropertyInt("ScriptingBackend", (int)ScriptingImplementation.IL2CPP, BuildTargetGroup.iOS);
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Release);
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.iOS, Il2CppCompilerConfiguration.Release);
            PlayerSettings.SetIncrementalIl2CppBuild(BuildTargetGroup.Android, true);
            PlayerSettings.SetIncrementalIl2CppBuild(BuildTargetGroup.iOS, true);
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Medium);
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.iOS, ManagedStrippingLevel.Medium);
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);

            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            if (!defines.Contains("ADMOB_ENABLED")) defines += ";ADMOB_ENABLED";
            if (!defines.Contains("XCAPE_MOBILE")) defines += ";XCAPE_MOBILE";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defines);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, defines);

            // Enable new Input System
            PlayerSettings.SetPropertyInt("ActiveInputHandler", 1, BuildTargetGroup.Android);
            PlayerSettings.SetPropertyInt("ActiveInputHandler", 1, BuildTargetGroup.iOS);

            // Setup URP if available
            SetupURP();

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
                // Build basic Main Menu UI
                BuildMainMenuUI();
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

        private void SetupURP()
        {
#if UNITY_RENDER_PIPELINE_UNIVERSAL
            var settingsPath = "Assets/Settings";
            if (!Directory.Exists(settingsPath)) Directory.CreateDirectory(settingsPath);
            var rpAssetPath = Path.Combine(settingsPath, "XCAPE_URP_Asset.asset");
            UniversalRenderPipelineAsset urp = null;
            if (File.Exists(rpAssetPath))
            {
                urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(rpAssetPath);
            }
            if (urp == null)
            {
                urp = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
                AssetDatabase.CreateAsset(urp, rpAssetPath);
                AssetDatabase.SaveAssets();
            }
            GraphicsSettings.renderPipelineAsset = urp;
#else
            Debug.LogWarning("URP package not detected at compile time. Ensure com.unity.render-pipelines.universal is installed.");
#endif
        }

    // ===== Gameplay Scene Auto-Setup =====
        private void CreateWiredGameplayScene()
        {
            EnsureTags(new[] { "Lane", "Gutter", "Pin", "PinArea", "Ball" });

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = "GamePlay";

            // Root containers
            var root = new GameObject("GamePlayRoot");
            var env = new GameObject("Environment");
            env.transform.parent = root.transform;

            // Ensure PhysicMaterials exist
            var pm = EnsurePhysicMaterials();

            // Lane
            var lane = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lane.name = "Lane";
            lane.transform.parent = env.transform;
            lane.transform.position = new Vector3(0, 0, 9f);
            lane.transform.localScale = new Vector3(1.07f, 0.02f, 18f); // ~41.5" width, 18m largo
            lane.tag = "Lane";
            var laneCol = lane.GetComponent<BoxCollider>();
            laneCol.isTrigger = false;
            laneCol.material = pm.lane;

            // Gutters (triggers)
            var leftGutter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftGutter.name = "Gutter_Left";
            leftGutter.transform.parent = env.transform;
            leftGutter.transform.position = new Vector3(-0.65f, 0.1f, 9f);
            leftGutter.transform.localScale = new Vector3(0.3f, 0.2f, 18f);
            leftGutter.tag = "Gutter";
            var lgc = leftGutter.GetComponent<BoxCollider>(); lgc.isTrigger = true; lgc.material = pm.gutter;

            var rightGutter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightGutter.name = "Gutter_Right";
            rightGutter.transform.parent = env.transform;
            rightGutter.transform.position = new Vector3(0.65f, 0.1f, 9f);
            rightGutter.transform.localScale = new Vector3(0.3f, 0.2f, 18f);
            rightGutter.tag = "Gutter";
            var rgc = rightGutter.GetComponent<BoxCollider>(); rgc.isTrigger = true; rgc.material = pm.gutter;

            // Pin Area (trigger near end)
            var pinArea = new GameObject("PinArea");
            pinArea.transform.parent = env.transform;
            pinArea.transform.position = new Vector3(0, 0.5f, 16f);
            var paCol = pinArea.AddComponent<BoxCollider>();
            paCol.isTrigger = true;
            paCol.size = new Vector3(1.2f, 1.0f, 2.0f);
            pinArea.tag = "PinArea";

            // Spawn point
            var spawn = new GameObject("BallSpawn").transform;
            spawn.parent = root.transform;
            spawn.position = new Vector3(0, 0.12f, 1.0f);

            // Create Pin prefab
            var pinPrefab = CreatePinPrefab(pm);

            // Create 10 pins positions and instances
            var pinPositionsRoot = new GameObject("PinPositions").transform;
            pinPositionsRoot.parent = root.transform;
            var pinsParent = new GameObject("Pins").transform;
            pinsParent.parent = root.transform;

            float s = 0.3048f; // 12 inches spacing
            Vector3 headPos = new Vector3(0, 0, 16.0f);
            Vector3[] rel = new Vector3[10]
            {
                new Vector3(0,0,0),                          // 1
                new Vector3(-s/2f,0,-s), new Vector3(s/2f,0,-s), // 2,3
                new Vector3(-s,0,-2*s), new Vector3(0,0,-2*s), new Vector3(s,0,-2*s), // 4,5,6
                new Vector3(-1.5f*s,0,-3*s), new Vector3(-0.5f*s,0,-3*s), new Vector3(0.5f*s,0,-3*s), new Vector3(1.5f*s,0,-3*s) // 7-10
            };

            var pinControllers = new XCAPE.Gameplay.PinController[10];
            var pinPosTransforms = new Transform[10];
            for (int i = 0; i < 10; i++)
            {
                var posT = new GameObject($"PinPos_{i+1:00}").transform;
                posT.parent = pinPositionsRoot;
                posT.position = headPos + rel[i];
                pinPosTransforms[i] = posT;

                var pinInstance = (GameObject)PrefabUtility.InstantiatePrefab(pinPrefab);
                pinInstance.transform.position = posT.position;
                pinInstance.transform.rotation = Quaternion.identity;
                pinInstance.transform.parent = pinsParent;
                var pc = pinInstance.GetComponent<XCAPE.Gameplay.PinController>();
                pc.SetPinNumber(i + 1);
                pinControllers[i] = pc;
            }

            // Ball prefab and instance
            var ballPrefab = CreateBallPrefab(pm);
            var ballInstance = (GameObject)PrefabUtility.InstantiatePrefab(ballPrefab);
            ballInstance.transform.position = spawn.position;
            ballInstance.transform.parent = root.transform;
            var ballCtrl = ballInstance.GetComponent<XCAPE.Gameplay.BallController>();

            // LaneController
            var laneCtrlGO = new GameObject("LaneController");
            laneCtrlGO.transform.parent = root.transform;
            var laneCtrl = laneCtrlGO.AddComponent<XCAPE.Gameplay.LaneController>();
            var so = new SerializedObject(laneCtrl);
            so.FindProperty("ballSpawnPoint").objectReferenceValue = spawn;
            so.FindProperty("ball").objectReferenceValue = ballCtrl;
            so.FindProperty("pins").arraySize = 10;
            for (int i = 0; i < 10; i++)
            {
                so.FindProperty("pins").GetArrayElementAtIndex(i).objectReferenceValue = pinControllers[i];
            }
            so.FindProperty("pinPositions").arraySize = 10;
            for (int i = 0; i < 10; i++)
            {
                so.FindProperty("pinPositions").GetArrayElementAtIndex(i).objectReferenceValue = pinPosTransforms[i];
            }
            so.FindProperty("laneTrigger").objectReferenceValue = laneCol;
            so.FindProperty("leftGutterTrigger").objectReferenceValue = lgc;
            so.FindProperty("rightGutterTrigger").objectReferenceValue = rgc;
            so.FindProperty("pinAreaTrigger").objectReferenceValue = paCol;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Managers
            var managers = new GameObject("Managers");
            var gm = managers.AddComponent<XCAPE.Core.GameManager>();
            var sm = managers.AddComponent<XCAPE.Core.ScoreManager>();
            var am = managers.AddComponent<XCAPE.Core.AudioManager>();
            var ui = managers.AddComponent<XCAPE.Core.UIManager>();
            managers.AddComponent<XCAPE.Core.InputManager>();

            // UI
            CreateBasicHUD(ui);

            // Camera
            var cam = Camera.main != null ? Camera.main.gameObject : new GameObject("Main Camera");
            if (!cam.GetComponent<Camera>()) cam.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.transform.position = new Vector3(0, 2.2f, -2.5f);
            cam.transform.LookAt(ballInstance.transform.position + Vector3.forward * 3f);
            var follow = cam.GetComponent<XCAPE.Core.CameraFollow>() ?? cam.AddComponent<XCAPE.Core.CameraFollow>();
            var soCam = new SerializedObject(follow);
            soCam.FindProperty("target").objectReferenceValue = ballCtrl;
            soCam.ApplyModifiedPropertiesWithoutUndo();

            // Save
            var gameplayPath = "Assets/Scenes/GamePlay/GamePlay.unity";
            Directory.CreateDirectory("Assets/Scenes/GamePlay");
            EditorSceneManager.SaveScene(scene, gameplayPath);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("XCAPE", "Gameplay Scene (wired) creada y guardada.", "OK");
        }

        private GameObject CreatePinPrefab((PhysicMaterial ball, PhysicMaterial lane, PhysicMaterial gutter, PhysicMaterial pin) pm)
        {
            Directory.CreateDirectory("Assets/Prefabs/Gameplay");
            var go = new GameObject("Pin");
            var col = go.AddComponent<CapsuleCollider>();
            var rb = go.AddComponent<Rigidbody>();
            var pc = go.AddComponent<XCAPE.Gameplay.PinController>();
            go.tag = "Pin";
            col.height = 0.381f; col.radius = 0.06f; col.center = new Vector3(0, col.height * 0.5f, 0);
            col.material = pm.pin;
            rb.mass = 1.59f; rb.interpolation = RigidbodyInterpolation.Interpolate; rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            // Netcode components (if available)
#if UNITY_NETCODE_GAMEOBJECTS
            var no = go.AddComponent<Unity.Netcode.NetworkObject>();
            go.AddComponent<Unity.Netcode.Components.NetworkTransform>();
#endif
            var path = "Assets/Prefabs/Gameplay/Pin.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            GameObject.DestroyImmediate(go);
            return prefab;
        }

        private GameObject CreateBallPrefab((PhysicMaterial ball, PhysicMaterial lane, PhysicMaterial gutter, PhysicMaterial pin) pm)
        {
            Directory.CreateDirectory("Assets/Prefabs/Gameplay");
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Ball";
            var rb = go.GetComponent<Rigidbody>() ?? go.AddComponent<Rigidbody>();
            var sc = go.GetComponent<SphereCollider>();
            var aud = go.AddComponent<AudioSource>();
            var bc = go.AddComponent<XCAPE.Gameplay.BallController>();
            go.tag = "Ball";
            rb.mass = 7.26f; rb.interpolation = RigidbodyInterpolation.Interpolate; rb.collisionDetectionMode = CollisionDetectionMode.Continuous; rb.drag = 0.1f; rb.angularDrag = 2f;
            sc.radius = 0.108f;
            sc.material = pm.ball;
            // Netcode components (if available)
#if UNITY_NETCODE_GAMEOBJECTS
            var no = go.AddComponent<Unity.Netcode.NetworkObject>();
            go.AddComponent<Unity.Netcode.Components.NetworkTransform>();
#endif
            // Trail
            var tr = go.AddComponent<TrailRenderer>();
            tr.time = 0.3f; tr.startWidth = 0.05f; tr.endWidth = 0.01f; tr.emitting = false;
            var path = "Assets/Prefabs/Gameplay/Ball.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            GameObject.DestroyImmediate(go);
            return prefab;
        }

        private void CreateBasicHUD(XCAPE.Core.UIManager ui)
        {
            // Canvas + EventSystem + Texts
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<UnityEngine.Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            if (!FindObjectOfType<UnityEngine.EventSystems.EventSystem>())
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            UnityEngine.UI.Text MakeLabel(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos)
            {
                var go = new GameObject(name);
                go.transform.SetParent(canvasGO.transform);
                var txt = go.AddComponent<UnityEngine.UI.Text>();
                txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                txt.fontSize = 20; txt.color = Color.white; txt.alignment = TextAnchor.MiddleLeft;
                var rt = txt.rectTransform; rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.sizeDelta = new Vector2(300, 30); rt.anchoredPosition = anchoredPos;
                return txt;
            }

            var power = MakeLabel("PowerText", new Vector2(0,1), new Vector2(0,1), new Vector2(10,-10));
            var spin  = MakeLabel("SpinText", new Vector2(0,1), new Vector2(0,1), new Vector2(10,-40));
            var frame = MakeLabel("FrameText", new Vector2(1,1), new Vector2(1,1), new Vector2(-150,-10));
            var total = MakeLabel("TotalText", new Vector2(1,1), new Vector2(1,1), new Vector2(-150,-40));

            // Panel HUD container
            var hudPanel = new GameObject("HUDPanel");
            hudPanel.transform.SetParent(canvasGO.transform, false);

            // Pause button
            var pauseBtn = CreateButton(canvasGO.transform, "Pause", new Vector2(0.95f, 0.1f));
            pauseBtn.onClick.AddListener(() => XCAPE.Core.GameManager.Instance?.PauseGame());

            // Scorecard simple
            var scorecardGO = new GameObject("ScorecardText");
            scorecardGO.transform.SetParent(canvasGO.transform, false);
            var scText = scorecardGO.AddComponent<UnityEngine.UI.Text>();
            scText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            scText.fontSize = 14; scText.color = Color.white; scText.alignment = TextAnchor.UpperLeft;
            var scRT = scText.rectTransform; scRT.anchorMin = new Vector2(0,0); scRT.anchorMax = new Vector2(0,0); scRT.anchoredPosition = new Vector2(10,10); scRT.sizeDelta = new Vector2(360, 260);
            scorecardGO.AddComponent<XCAPE.UI.ScorecardUI>();

            // Bowling score grid (compact)
            var gridRoot = new GameObject("BowlingScoreGrid");
            gridRoot.transform.SetParent(canvasGO.transform, false);
            var gridRT = gridRoot.AddComponent<RectTransform>(); gridRT.anchorMin = new Vector2(0.5f,1f); gridRT.anchorMax = new Vector2(0.5f,1f); gridRT.anchoredPosition = new Vector2(0,-20); gridRT.sizeDelta = new Vector2(640, 120);
            gridRoot.AddComponent<XCAPE.UI.BowlingScoreGrid>();

            // Pause panel básico
            var pausePanel = new GameObject("PausePanel");
            pausePanel.transform.SetParent(canvasGO.transform, false);
            var ppImg = pausePanel.AddComponent<UnityEngine.UI.Image>(); ppImg.color = new Color(0,0,0,0.6f);
            var ppRT = ppImg.rectTransform; ppRT.anchorMin = new Vector2(0.2f,0.2f); ppRT.anchorMax = new Vector2(0.8f,0.8f); ppRT.offsetMin = Vector2.zero; ppRT.offsetMax = Vector2.zero; pausePanel.SetActive(false);
            var resumeBtn = CreateButton(pausePanel.transform, "Resume", new Vector2(0.5f,0.65f));
            var restartBtn = CreateButton(pausePanel.transform, "Restart", new Vector2(0.5f,0.55f));
            var menuBtn = CreateButton(pausePanel.transform, "Menu", new Vector2(0.5f,0.45f));
            var pmc = pausePanel.AddComponent<XCAPE.UI.PauseMenuController>();
            var soPM = new SerializedObject(pmc);
            soPM.FindProperty("resumeButton").objectReferenceValue = resumeBtn;
            soPM.FindProperty("restartButton").objectReferenceValue = restartBtn;
            soPM.FindProperty("quitButton").objectReferenceValue = menuBtn;
            soPM.ApplyModifiedPropertiesWithoutUndo();

            // Game Over panel
            var goPanel = new GameObject("GameOverPanel");
            goPanel.transform.SetParent(canvasGO.transform, false);
            var goImg = goPanel.AddComponent<UnityEngine.UI.Image>(); goImg.color = new Color(0,0,0,0.75f);
            var goRT = goImg.rectTransform; goRT.anchorMin = new Vector2(0.15f,0.2f); goRT.anchorMax = new Vector2(0.85f,0.8f); goRT.offsetMin = Vector2.zero; goRT.offsetMax = Vector2.zero; goPanel.SetActive(false);
            var titleGO = CreateText(goPanel.transform, "Game Over", 32, new Vector2(0.5f,0.85f));
            var finalText = CreateText(goPanel.transform, "Score: 000", 24, new Vector2(0.5f,0.65f)); finalText.name = "FinalScoreText";
            var highText = CreateText(goPanel.transform, "High: 000", 20, new Vector2(0.5f,0.58f)); highText.name = "HighScoreText";
            var strikesText = CreateText(goPanel.transform, "Strikes: 0", 18, new Vector2(0.5f,0.50f)); strikesText.name = "StrikesText";
            var sparesText = CreateText(goPanel.transform, "Spares: 0", 18, new Vector2(0.5f,0.44f)); sparesText.name = "SparesText";
            var againBtn = CreateButton(goPanel.transform, "Play Again", new Vector2(0.45f,0.3f));
            var backBtn = CreateButton(goPanel.transform, "Menu", new Vector2(0.55f,0.3f));
            var goc = goPanel.AddComponent<XCAPE.UI.GameOverPanelController>();
            var soGOC = new SerializedObject(goc);
            soGOC.FindProperty("finalScoreText").objectReferenceValue = finalText;
            soGOC.FindProperty("highScoreText").objectReferenceValue = highText;
            soGOC.FindProperty("strikesText").objectReferenceValue = strikesText;
            soGOC.FindProperty("sparesText").objectReferenceValue = sparesText;
            soGOC.FindProperty("playAgainButton").objectReferenceValue = againBtn;
            soGOC.FindProperty("menuButton").objectReferenceValue = backBtn;
            soGOC.ApplyModifiedPropertiesWithoutUndo();

            // Wire UIManager panels
            var so = new SerializedObject(ui);
            so.FindProperty("powerText").objectReferenceValue = power;
            so.FindProperty("spinText").objectReferenceValue = spin;
            so.FindProperty("frameText").objectReferenceValue = frame;
            so.FindProperty("totalText").objectReferenceValue = total;
            so.FindProperty("mainMenuPanel").objectReferenceValue = null; // Solo en MainMenu
            so.FindProperty("hudPanel").objectReferenceValue = hudPanel;
            so.FindProperty("pausePanel").objectReferenceValue = pausePanel;
            so.FindProperty("gameOverPanel").objectReferenceValue = goPanel;
            so.FindProperty("scoreGrid").objectReferenceValue = gridRoot;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void EnsureTags(string[] tags)
        {
            var manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tagsProp = manager.FindProperty("tags");
            foreach (var t in tags)
            {
                bool exists = false;
                for (int i = 0; i < tagsProp.arraySize; i++)
                {
                    var e = tagsProp.GetArrayElementAtIndex(i);
                    if (e.stringValue == t) { exists = true; break; }
                }
                if (!exists)
                {
                    tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                    tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = t;
                }
            }
            manager.ApplyModifiedProperties();
        }

        // ===== Physic Materials Creation & Application =====
        private (PhysicMaterial ball, PhysicMaterial lane, PhysicMaterial gutter, PhysicMaterial pin) EnsurePhysicMaterials()
        {
            var dir = "Assets/Physics/PhysicMaterials";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            PhysicMaterial LoadOrCreate(string name, System.Action<PhysicMaterial> configure)
            {
                var path = Path.Combine(dir, name + ".physicMaterial").Replace("\\", "/");
                var mat = AssetDatabase.LoadAssetAtPath<PhysicMaterial>(path);
                if (mat == null)
                {
                    mat = new PhysicMaterial(name);
                    configure?.Invoke(mat);
                    AssetDatabase.CreateAsset(mat, path);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    configure?.Invoke(mat);
                    EditorUtility.SetDirty(mat);
                }
                return mat;
            }

            var ball = LoadOrCreate("Ball", m =>
            {
                m.dynamicFriction = 0.2f;
                m.staticFriction = 0.25f;
                m.bounciness = 0.02f;
                m.frictionCombine = PhysicMaterialCombine.Average;
                m.bounceCombine = PhysicMaterialCombine.Minimum;
            });

            var lane = LoadOrCreate("Lane", m =>
            {
                m.dynamicFriction = 0.01f;
                m.staticFriction = 0.02f;
                m.bounciness = 0.0f;
                m.frictionCombine = PhysicMaterialCombine.Minimum; // forzar baja fricción
                m.bounceCombine = PhysicMaterialCombine.Minimum;
            });

            var gutter = LoadOrCreate("Gutter", m =>
            {
                m.dynamicFriction = 0.8f;
                m.staticFriction = 0.9f;
                m.bounciness = 0.05f;
                m.frictionCombine = PhysicMaterialCombine.Maximum; // fricción alta
                m.bounceCombine = PhysicMaterialCombine.Minimum;
            });

            var pin = LoadOrCreate("Pin", m =>
            {
                m.dynamicFriction = 0.4f;
                m.staticFriction = 0.6f;
                m.bounciness = 0.1f;
                m.frictionCombine = PhysicMaterialCombine.Average;
                m.bounceCombine = PhysicMaterialCombine.Minimum;
            });

            return (ball, lane, gutter, pin);
        }

        private void ApplyPhysicsAndMaterialsTuning()
        {
            var pm = EnsurePhysicMaterials();

            // Asignar materiales por etiqueta/nombre
            foreach (var col in Object.FindObjectsOfType<Collider>())
            {
                if (col == null || col.gameObject == null) continue;
                var go = col.gameObject;
                if (go.CompareTag("Ball"))
                {
                    col.material = pm.ball;
                    var rb = go.GetComponent<Rigidbody>();
                    if (rb)
                    {
                        rb.mass = 7.26f; rb.drag = 0.1f; rb.angularDrag = 2f; rb.interpolation = RigidbodyInterpolation.Interpolate; rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                        rb.maxAngularVelocity = 50f;
                    }
                }
                else if (go.CompareTag("Pin"))
                {
                    col.material = pm.pin;
                    var rb = go.GetComponent<Rigidbody>();
                    if (rb)
                    {
                        rb.mass = 1.59f; rb.drag = 0.5f; rb.angularDrag = 3f; rb.interpolation = RigidbodyInterpolation.Interpolate; rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    }
                }
                else if (go.CompareTag("Lane"))
                {
                    col.material = pm.lane;
                    col.isTrigger = false;
                }
                else if (go.CompareTag("Gutter"))
                {
                    col.material = pm.gutter;
                    col.isTrigger = true;
                }
            }

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("XCAPE", "Physics & PhysicMaterials tuning applied to scene objects.", "OK");
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

        private void BuildMainMenuUI()
        {
            var canvasGO = new GameObject("Canvas_MainMenu");
            var canvas = canvasGO.AddComponent<UnityEngine.Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            if (!Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>())
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Panel principal
            var panel = new GameObject("Panel");
            panel.transform.SetParent(canvasGO.transform, false);
            var img = panel.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0,0,0,0.5f);
            var rtPanel = img.rectTransform; rtPanel.anchorMin = Vector2.zero; rtPanel.anchorMax = Vector2.one; rtPanel.offsetMin = Vector2.zero; rtPanel.offsetMax = Vector2.zero;

            // Titulo
            var title = CreateText(canvasGO.transform, "XCAPE Bowling", 36, new Vector2(0.5f, 0.8f));

            // Botones
            var playBtn = CreateButton(canvasGO.transform, "Play", new Vector2(0.5f, 0.6f));
            var settingsBtn = CreateButton(canvasGO.transform, "Settings", new Vector2(0.5f, 0.5f));
            var quitBtn = CreateButton(canvasGO.transform, "Quit", new Vector2(0.5f, 0.4f));

            // Panel Multiplayer simple
            var mpPanel = new GameObject("MultiplayerPanel");
            mpPanel.transform.SetParent(canvasGO.transform, false);
            var mpImg = mpPanel.AddComponent<UnityEngine.UI.Image>(); mpImg.color = new Color(0,0,0,0.35f);
            var mpRT = mpImg.rectTransform; mpRT.anchorMin = new Vector2(0.1f,0.1f); mpRT.anchorMax = new Vector2(0.3f,0.3f); mpRT.offsetMin = Vector2.zero; mpRT.offsetMax = Vector2.zero;
            var hostBtn = CreateButton(mpPanel.transform, "Host", new Vector2(0.5f,0.7f));
            var joinBtn = CreateButton(mpPanel.transform, "Join", new Vector2(0.5f,0.5f));
            var statusT = CreateText(mpPanel.transform, "Status", 14, new Vector2(0.5f,0.25f));
            var net = Object.FindObjectOfType<XCAPE.Networking.NetcodeBootstrap>() ?? new GameObject("NetcodeBootstrap").AddComponent<XCAPE.Networking.NetcodeBootstrap>();
            var mpc = mpPanel.AddComponent<XCAPE.UI.MultiplayerMenuController>();
            var soMPC = new SerializedObject(mpc);
            soMPC.FindProperty("hostButton").objectReferenceValue = hostBtn;
            soMPC.FindProperty("joinButton").objectReferenceValue = joinBtn;
            soMPC.FindProperty("statusTextGO").objectReferenceValue = statusT.gameObject;
            soMPC.ApplyModifiedPropertiesWithoutUndo();

            // Lobby panel (Relay + Lobbies)
            BuildLobbyPanel(canvasGO.transform);

            // Settings panel
            var settingsPanel = new GameObject("SettingsPanel");
            settingsPanel.transform.SetParent(canvasGO.transform, false);
            var spImg = settingsPanel.AddComponent<UnityEngine.UI.Image>();
            spImg.color = new Color(0,0,0,0.7f);
            var rtSP = spImg.rectTransform; rtSP.anchorMin = new Vector2(0.2f,0.2f); rtSP.anchorMax = new Vector2(0.8f,0.8f); rtSP.offsetMin = Vector2.zero; rtSP.offsetMax = Vector2.zero;
            settingsPanel.SetActive(false);

            // Sliders básicos
            CreateLabeledSlider(settingsPanel.transform, "Master", new Vector2(0.5f, 0.7f));
            CreateLabeledSlider(settingsPanel.transform, "Music", new Vector2(0.5f, 0.55f));
            CreateLabeledSlider(settingsPanel.transform, "SFX", new Vector2(0.5f, 0.4f));
            var perfToggle = CreateToggle(settingsPanel.transform, "Performance Mode", new Vector2(0.5f, 0.25f));

            // Wire controller
            var mm = canvasGO.AddComponent<XCAPE.UI.MainMenuController>();
            var spc = settingsPanel.AddComponent<XCAPE.UI.SettingsPanelController>();
            // Assign refs
            var so = new SerializedObject(mm);
            so.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
            so.FindProperty("playButton").objectReferenceValue = playBtn;
            so.FindProperty("settingsButton").objectReferenceValue = settingsBtn;
            so.FindProperty("quitButton").objectReferenceValue = quitBtn;
            so.ApplyModifiedPropertiesWithoutUndo();

            var soSP = new SerializedObject(spc);
            soSP.FindProperty("masterSlider").objectReferenceValue = settingsPanel.transform.Find("Slider_Master").GetComponent<UnityEngine.UI.Slider>();
            soSP.FindProperty("musicSlider").objectReferenceValue = settingsPanel.transform.Find("Slider_Music").GetComponent<UnityEngine.UI.Slider>();
            soSP.FindProperty("sfxSlider").objectReferenceValue = settingsPanel.transform.Find("Slider_SFX").GetComponent<UnityEngine.UI.Slider>();
            soSP.FindProperty("performanceToggle").objectReferenceValue = perfToggle;
            soSP.ApplyModifiedPropertiesWithoutUndo();
        }

        private void BuildLobbyPanel(Transform canvasTransform)
        {
            var lobbyPanel = new GameObject("LobbyPanel");
            lobbyPanel.transform.SetParent(canvasTransform, false);
            var lpImg = lobbyPanel.AddComponent<UnityEngine.UI.Image>(); lpImg.color = new Color(0,0,0,0.45f);
            var lpRT = lpImg.rectTransform; lpRT.anchorMin = new Vector2(0.65f,0.25f); lpRT.anchorMax = new Vector2(0.95f,0.95f); lpRT.offsetMin = Vector2.zero; lpRT.offsetMax = Vector2.zero;

            // Title
            CreateText(lobbyPanel.transform, "Lobby", 18, new Vector2(0.5f,0.95f));

            // Lobby name + input
            CreateText(lobbyPanel.transform, "Lobby Name", 14, new Vector2(0.5f,0.89f));
            var nameInputGO = new GameObject("LobbyName"); nameInputGO.transform.SetParent(lobbyPanel.transform, false);
            var nameIFBG = nameInputGO.AddComponent<UnityEngine.UI.Image>(); nameIFBG.color = new Color(1,1,1,0.08f);
            var nameInput = nameInputGO.AddComponent<UnityEngine.UI.InputField>();
            nameInput.textComponent = CreateText(nameInputGO.transform, "XCAPE", 16, new Vector2(0.5f,0.0f));
            var nameRT = nameIFBG.rectTransform; nameRT.anchorMin = new Vector2(0.1f,0.83f); nameRT.anchorMax = new Vector2(0.9f,0.87f); nameRT.offsetMin = Vector2.zero; nameRT.offsetMax = Vector2.zero;
            nameInput.lineType = UnityEngine.UI.InputField.LineType.SingleLine;
            nameInput.characterValidation = UnityEngine.UI.InputField.CharacterValidation.Alphanumeric;

            // Join code + input
            CreateText(lobbyPanel.transform, "Join Code", 14, new Vector2(0.5f,0.80f));
            var joinInputGO = new GameObject("JoinCode"); joinInputGO.transform.SetParent(lobbyPanel.transform, false);
            var joinIFBG = joinInputGO.AddComponent<UnityEngine.UI.Image>(); joinIFBG.color = new Color(1,1,1,0.08f);
            var joinInput = joinInputGO.AddComponent<UnityEngine.UI.InputField>();
            joinInput.textComponent = CreateText(joinInputGO.transform, "", 16, new Vector2(0.5f,0.0f));
            var joinRT = joinIFBG.rectTransform; joinRT.anchorMin = new Vector2(0.1f,0.74f); joinRT.anchorMax = new Vector2(0.9f,0.78f); joinRT.offsetMin = Vector2.zero; joinRT.offsetMax = Vector2.zero;
            joinInput.lineType = UnityEngine.UI.InputField.LineType.SingleLine;
            joinInput.characterValidation = UnityEngine.UI.InputField.CharacterValidation.Alphanumeric;
            joinInput.characterLimit = 6;

            // Players list container
            var listRoot = new GameObject("PlayersList"); listRoot.transform.SetParent(lobbyPanel.transform, false);
            var listImg = listRoot.AddComponent<UnityEngine.UI.Image>(); listImg.color = new Color(1,1,1,0.05f);
            var listRT = listImg.rectTransform; listRT.anchorMin = new Vector2(0.05f,0.36f); listRT.anchorMax = new Vector2(0.95f,0.72f); listRT.offsetMin = Vector2.zero; listRT.offsetMax = Vector2.zero;
            listRoot.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();

            // Buttons
            var createBtn = CreateButton(lobbyPanel.transform, "Create Lobby", new Vector2(0.5f,0.30f));
            var joinBtn2  = CreateButton(lobbyPanel.transform, "Join Lobby", new Vector2(0.5f,0.23f));
            var leaveBtn  = CreateButton(lobbyPanel.transform, "Leave",        new Vector2(0.5f,0.16f));

            // Ready + Start
            var readyToggle = CreateToggle(lobbyPanel.transform, "I'm Ready", new Vector2(0.3f, 0.09f));
            var startBtn = CreateButton(lobbyPanel.transform, "Start Game", new Vector2(0.7f, 0.09f));
            (startBtn.targetGraphic as UnityEngine.UI.Image).color = new Color(0.2f,0.7f,0.2f,0.25f);

            // Codes / status
            var codeText = CreateText(lobbyPanel.transform, "Code: -", 14, new Vector2(0.5f,0.045f));
            var statusText = CreateText(lobbyPanel.transform, "Idle", 12, new Vector2(0.5f,0.01f));

            var _ = Object.FindObjectOfType<XCAPE.Networking.RelayLobbyManager>() ?? new GameObject("RelayLobbyManager").AddComponent<XCAPE.Networking.RelayLobbyManager>();
            var lpc = lobbyPanel.AddComponent<XCAPE.UI.LobbyPanelController>();
            var soLPC = new SerializedObject(lpc);
            soLPC.FindProperty("lobbyNameField").objectReferenceValue = nameInput;
            soLPC.FindProperty("joinCodeField").objectReferenceValue = joinInput;
            soLPC.FindProperty("createButton").objectReferenceValue = createBtn;
            soLPC.FindProperty("joinButton").objectReferenceValue = joinBtn2;
            soLPC.FindProperty("leaveButton").objectReferenceValue = leaveBtn;
            soLPC.FindProperty("playersListRoot").objectReferenceValue = listRoot.GetComponent<RectTransform>();
            soLPC.FindProperty("readyToggle").objectReferenceValue = readyToggle;
            soLPC.FindProperty("startButton").objectReferenceValue = startBtn;
            soLPC.FindProperty("statusText").objectReferenceValue = statusText;
            soLPC.FindProperty("joinCodeText").objectReferenceValue = codeText;
            soLPC.ApplyModifiedPropertiesWithoutUndo();
        }

        private void AddLobbyPanelToCurrentScene()
        {
            var active = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            if (!active.IsValid())
            {
                EditorUtility.DisplayDialog("XCAPE", "No active scene found.", "OK");
                return;
            }
            var canvas = Object.FindObjectOfType<UnityEngine.Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<UnityEngine.Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                if (!Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>())
                {
                    var es = new GameObject("EventSystem");
                    es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
            }
            BuildLobbyPanel(canvas.transform);
            EditorUtility.DisplayDialog("XCAPE", "Lobby Panel added to current scene.", "OK");
        }

    private void OpenServicesSettings()
    {
#if UNITY_2021_3_OR_NEWER
        SettingsService.OpenProjectSettings("Project/Services");
#else
        EditorUtility.DisplayDialog("XCAPE", "Open Project Settings > Services to link your project to Unity Gaming Services.", "OK");
#endif
    }

        // ===== Unity Packages Installer =====
        private Queue<string> _packagesQueue;
        private AddRequest _currentAdd;
        private ListRequest _listRequest;

        private static readonly string[] RequiredPackages = new[]
        {
            "com.unity.services.core",
            "com.unity.services.authentication",
            "com.unity.services.lobby",
            "com.unity.services.relay",
            "com.unity.transport",
            "com.unity.netcode.gameobjects"
        };

        private void EnsureUnityPackages()
        {
            _listRequest = Client.List(true);
            EditorApplication.update += OnListProgress;
            EditorUtility.DisplayProgressBar("XCAPE", "Checking installed packages...", 0.1f);
        }

        private void OnListProgress()
        {
            if (_listRequest == null) return;
            if (!_listRequest.IsCompleted) return;
            EditorApplication.update -= OnListProgress;
            var installed = new HashSet<string>(_listRequest.Result.Select(p => p.name));
            _packagesQueue = new Queue<string>(RequiredPackages.Where(p => !installed.Contains(p)));
            EditorUtility.ClearProgressBar();
            if (_packagesQueue.Count == 0)
            {
                EditorUtility.DisplayDialog("XCAPE", "All required packages are already installed.", "OK");
                return;
            }
            InstallNextPackage();
        }

        private void InstallNextPackage()
        {
            if (_packagesQueue == null || _packagesQueue.Count == 0)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("XCAPE", "Unity Services & Netcode packages installation finished.", "OK");
                return;
            }
            var pkg = _packagesQueue.Dequeue();
            EditorUtility.DisplayProgressBar("XCAPE", $"Installing {pkg}...", 0.5f);
            _currentAdd = Client.Add(pkg);
            EditorApplication.update += OnAddProgress;
        }

        private void OnAddProgress()
        {
            if (_currentAdd == null) return;
            if (!_currentAdd.IsCompleted) return;
            EditorApplication.update -= OnAddProgress;
            if (_currentAdd.Status == StatusCode.Failure)
            {
                Debug.LogError($"Failed to install package: {_currentAdd.Error?.message}");
            }
            InstallNextPackage();
        }

        private UnityEngine.UI.Text CreateText(Transform parent, string text, int size, Vector2 anchor)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<UnityEngine.UI.Text>();
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.text = text; t.fontSize = size; t.alignment = TextAnchor.MiddleCenter; t.color = Color.white;
            var rt = t.rectTransform; rt.anchorMin = anchor; rt.anchorMax = anchor; rt.anchoredPosition = Vector2.zero; rt.sizeDelta = new Vector2(400, 50);
            return t;
        }

        private UnityEngine.UI.Button CreateButton(Transform parent, string label, Vector2 anchor)
        {
            var go = new GameObject($"Button_{label}");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(1,1,1,0.1f);
            var btn = go.AddComponent<UnityEngine.UI.Button>();
            var rt = img.rectTransform; rt.anchorMin = anchor; rt.anchorMax = anchor; rt.anchoredPosition = Vector2.zero; rt.sizeDelta = new Vector2(220, 50);
            var txt = CreateText(go.transform, label, 24, new Vector2(0.5f,0.5f));
            txt.rectTransform.sizeDelta = new Vector2(200, 36);
            return btn;
        }

        private UnityEngine.UI.Slider CreateLabeledSlider(Transform parent, string label, Vector2 anchor)
        {
            var go = new GameObject($"Slider_{label}");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>(); rt.anchorMin = anchor; rt.anchorMax = anchor; rt.anchoredPosition = Vector2.zero; rt.sizeDelta = new Vector2(300, 30);
            var text = CreateText(go.transform, label, 18, new Vector2(0.2f,0.5f)); text.rectTransform.sizeDelta = new Vector2(120, 24);
            var sliderGO = new GameObject("Slider"); sliderGO.transform.SetParent(go.transform, false);
            var img = sliderGO.AddComponent<UnityEngine.UI.Image>(); img.color = new Color(1,1,1,0.2f);
            var slider = sliderGO.AddComponent<UnityEngine.UI.Slider>();
            var srt = img.rectTransform; srt.anchorMin = new Vector2(0.45f,0.5f); srt.anchorMax = new Vector2(0.95f,0.5f); srt.sizeDelta = new Vector2(0,20);
            return slider;
        }

        private UnityEngine.UI.Toggle CreateToggle(Transform parent, string label, Vector2 anchor)
        {
            var go = new GameObject($"Toggle_{label}");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<UnityEngine.UI.Image>(); img.color = new Color(1,1,1,0.1f);
            var toggle = go.AddComponent<UnityEngine.UI.Toggle>();
            var rt = img.rectTransform; rt.anchorMin = anchor; rt.anchorMax = anchor; rt.anchoredPosition = Vector2.zero; rt.sizeDelta = new Vector2(260, 30);
            var text = CreateText(go.transform, label, 18, new Vector2(0.65f,0.5f)); text.rectTransform.sizeDelta = new Vector2(200, 24);
            return toggle;
        }

    }
}
#endif
