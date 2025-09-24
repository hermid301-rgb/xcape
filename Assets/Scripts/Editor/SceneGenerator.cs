using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

namespace XCAPE.Editor
{
    public class SceneGenerator
    {
        [MenuItem("XCAPE/Generate Basic Scenes")]
        public static void GenerateBasicScenes()
        {
            Debug.Log("Generating basic scenes for XCAPE Bowling...");
            
            // Crear MainMenu scene
            CreateMainMenuScene();
            
            // Crear GamePlay scene
            CreateGamePlayScene();
            
            Debug.Log("Basic scenes generated successfully!");
        }
        
        private static void CreateMainMenuScene()
        {
            // Crear nueva escena
            Scene mainMenuScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Configurar Camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.2f, 1f);
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
            }
            
            // Crear Canvas UI
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Crear EventSystem
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            // Crear título
            GameObject titleGO = new GameObject("Title");
            titleGO.transform.SetParent(canvasGO.transform);
            UnityEngine.UI.Text titleText = titleGO.AddComponent<UnityEngine.UI.Text>();
            titleText.text = "XCAPE BOWLING";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 48;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.7f);
            titleRect.anchorMax = new Vector2(0.5f, 0.7f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = new Vector2(400, 80);
            
            // Crear botón Play
            GameObject playButtonGO = new GameObject("PlayButton");
            playButtonGO.transform.SetParent(canvasGO.transform);
            UnityEngine.UI.Button playButton = playButtonGO.AddComponent<UnityEngine.UI.Button>();
            UnityEngine.UI.Image buttonImage = playButtonGO.AddComponent<UnityEngine.UI.Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);
            
            // Texto del botón
            GameObject buttonTextGO = new GameObject("Text");
            buttonTextGO.transform.SetParent(playButtonGO.transform);
            UnityEngine.UI.Text buttonText = buttonTextGO.AddComponent<UnityEngine.UI.Text>();
            buttonText.text = "PLAY";
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = 24;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform buttonTextRect = buttonTextGO.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            RectTransform buttonRect = playButtonGO.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.4f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.4f);
            buttonRect.anchoredPosition = Vector2.zero;
            buttonRect.sizeDelta = new Vector2(200, 60);
            
            // Guardar escena
            string scenePath = "Assets/Scenes/MainMenu.unity";
            Directory.CreateDirectory(Path.GetDirectoryName(scenePath));
            EditorSceneManager.SaveScene(mainMenuScene, scenePath);
            
            Debug.Log($"MainMenu scene created at: {scenePath}");
        }
        
        private static void CreateGamePlayScene()
        {
            // Crear nueva escena
            Scene gamePlayScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Configurar Camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(0, 2, -10);
                mainCamera.transform.rotation = Quaternion.Euler(10, 0, 0);
                mainCamera.backgroundColor = new Color(0.2f, 0.3f, 0.5f, 1f);
            }
            
            // Crear suelo de bowling
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "BowlingLane";
            floor.transform.position = new Vector3(0, 0, 10);
            floor.transform.localScale = new Vector3(2, 1, 6);
            
            // Material del suelo
            Material laneMaterial = new Material(Shader.Find("Standard"));
            laneMaterial.color = new Color(0.8f, 0.6f, 0.4f, 1f);
            floor.GetComponent<Renderer>().material = laneMaterial;
            
            // Crear pines
            for (int i = 0; i < 10; i++)
            {
                GameObject pin = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                pin.name = $"Pin_{i + 1}";
                
                // Posicionamiento en formación triangular
                int row = GetPinRow(i);
                int posInRow = GetPinPositionInRow(i);
                float x = (posInRow - row * 0.5f) * 0.6f;
                float z = 18 + row * 0.8f;
                
                pin.transform.position = new Vector3(x, 1f, z);
                pin.transform.localScale = new Vector3(0.3f, 1f, 0.3f);
                
                // Material de los pines
                Material pinMaterial = new Material(Shader.Find("Standard"));
                pinMaterial.color = Color.white;
                pin.GetComponent<Renderer>().material = pinMaterial;
                
                // Agregar Rigidbody
                Rigidbody pinRb = pin.AddComponent<Rigidbody>();
                pinRb.mass = 1.5f;
            }
            
            // Crear bola de bowling
            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "BowlingBall";
            ball.transform.position = new Vector3(0, 0.5f, -5);
            ball.transform.localScale = Vector3.one * 0.8f;
            
            // Material de la bola
            Material ballMaterial = new Material(Shader.Find("Standard"));
            ballMaterial.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            ballMaterial.metallic = 0.8f;
            ball.GetComponent<Renderer>().material = ballMaterial;
            
            // Rigidbody de la bola
            Rigidbody ballRb = ball.AddComponent<Rigidbody>();
            ballRb.mass = 7f; // Peso realista de bola de bowling
            
            // Crear UI de gameplay
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // EventSystem
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            // Score UI
            GameObject scoreGO = new GameObject("Score");
            scoreGO.transform.SetParent(canvasGO.transform);
            UnityEngine.UI.Text scoreText = scoreGO.AddComponent<UnityEngine.UI.Text>();
            scoreText.text = "Score: 0";
            scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            scoreText.fontSize = 24;
            scoreText.color = Color.white;
            
            RectTransform scoreRect = scoreGO.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0, 1);
            scoreRect.anchorMax = new Vector2(0, 1);
            scoreRect.anchoredPosition = new Vector2(20, -20);
            scoreRect.sizeDelta = new Vector2(200, 30);
            
            // Guardar escena
            string scenePath = "Assets/Scenes/GamePlay.unity";
            Directory.CreateDirectory(Path.GetDirectoryName(scenePath));
            EditorSceneManager.SaveScene(gamePlayScene, scenePath);
            
            Debug.Log($"GamePlay scene created at: {scenePath}");
        }
        
        private static int GetPinRow(int pinIndex)
        {
            if (pinIndex < 1) return 0;
            if (pinIndex < 3) return 1;
            if (pinIndex < 6) return 2;
            return 3;
        }
        
        private static int GetPinPositionInRow(int pinIndex)
        {
            if (pinIndex < 1) return 0;
            if (pinIndex < 3) return pinIndex - 1;
            if (pinIndex < 6) return pinIndex - 3;
            return pinIndex - 6;
        }
    }
}
