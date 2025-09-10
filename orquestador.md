# 🎳 Orquestador de Desarrollo - XCAPE Bowling Game

## 📋 Checklist Pre-Desarrollo

### 🛠 Instalación del Entorno de Desarrollo

#### Unity Hub y Unity Editor
```bash
# 1. Descargar Unity Hub desde: https://unity.com/download
# 2. Instalar Unity 2023.3.40f1 LTS con módulos:
```
- ✅ **Android Build Support** (incluye Android SDK, NDK, OpenJDK)
- ✅ **iOS Build Support** (solo en Mac)
- ✅ **Universal Windows Platform**
- ✅ **Visual Studio Code Editor**
- ✅ **Unity Version Control**

#### Android Studio (para Android)
```bash
# Descargar desde: https://developer.android.com/studio
# Instalar Android SDK Tools, Platform Tools y Build Tools
# Configurar variables de entorno:
export ANDROID_SDK_ROOT=/path/to/Android/Sdk
export PATH=$PATH:$ANDROID_SDK_ROOT/tools:$ANDROID_SDK_ROOT/platform-tools
```

#### Xcode (para iOS - solo Mac)
```bash
# Instalar desde Mac App Store
# Instalar Command Line Tools:
xcode-select --install
```

#### Visual Studio Code
```bash
# Extensiones recomendadas:
- C# for Visual Studio Code
- Unity Tools
- Unity Code Snippets
- Git History
- Auto Rename Tag
```

### 🔑 Cuentas y Servicios Requeridos
- [ ] Unity Account (gratuito)
- [ ] Google Play Console ($25 único)
- [ ] Apple Developer Account ($99/año)
- [ ] Google AdMob Account
- [ ] Firebase Project
- [ ] Unity Cloud Account
- [ ] Git repository (GitHub/GitLab)

---

## 🚀 Fase 1: Setup del Proyecto (Semanas 1-2)

### Día 1: Creación del Proyecto Unity

#### Crear Nuevo Proyecto
```bash
# Abrir Unity Hub
# New Project -> 3D (URP) -> Project Name: XCAPE
# Location: ~/Projects/XCAPE
# Unity Version: 2023.3.40f1 LTS
```

#### Configuración Inicial del Proyecto
```csharp
// En Unity Editor:
// 1. File -> Build Settings
//    - Switch Platform to Android
//    - Add Open Scenes
// 2. Edit -> Project Settings
//    - Company Name: "Tu Empresa"
//    - Product Name: "XCAPE"
//    - Bundle Identifier: com.tuempresa.xcape
//    - Version: 1.0.0
```

### Día 2: Estructura de Carpetas y Packages

#### Estructura de Assets
```
Assets/
├── Scripts/
│   ├── Controllers/
│   │   ├── BallController.cs
│   │   ├── PinController.cs
│   │   └── GameController.cs
│   ├── Managers/
│   │   ├── GameManager.cs
│   │   ├── ScoreManager.cs
│   │   ├── AudioManager.cs
│   │   └── UIManager.cs
│   ├── UI/
│   │   ├── MainMenuUI.cs
│   │   ├── GameHUD.cs
│   │   └── SettingsUI.cs
│   ├── Physics/
│   │   ├── BallPhysics.cs
│   │   └── PinPhysics.cs
│   ├── Networking/
│   │   ├── NetworkManager.cs
│   │   └── MultiplayerManager.cs
│   └── Analytics/
│       ├── AnalyticsManager.cs
│       └── AdManager.cs
├── Scenes/
│   ├── MainMenu.unity
│   ├── GamePlay.unity
│   ├── Settings.unity
│   └── Leaderboards.unity
├── Prefabs/
│   ├── BowlingBall.prefab
│   ├── BowlingPin.prefab
│   ├── BowlingLane.prefab
│   └── UI_Elements/
├── Materials/
│   ├── BallMaterials/
│   ├── LaneMaterials/
│   └── PinMaterials/
├── Audio/
│   ├── SFX/
│   ├── Music/
│   └── Voice/
└── Models/
    ├── BowlingLane/
    ├── BowlingBall/
    ├── Pins/
    └── Environment/
```

#### Instalación de Packages Unity
```bash
# Window -> Package Manager -> Unity Registry
# Instalar packages:
```
- **Universal Render Pipeline** (com.unity.render-pipelines.universal)
- **Unity Ads** (com.unity.ads)
- **Unity Analytics** (com.unity.analytics)
- **Unity Authentication** (com.unity.services.authentication)
- **Unity Cloud Save** (com.unity.services.cloudsave)
- **Netcode for GameObjects** (com.unity.netcode.gameobjects)

### Día 3: Configuración de Version Control

```bash
# Inicializar Git en el proyecto
cd /path/to/XCAPE
git init
git remote add origin https://github.com/tu-usuario/xcape-bowling.git

# Crear .gitignore para Unity
curl -o .gitignore https://raw.githubusercontent.com/github/gitignore/main/Unity.gitignore

# Primer commit
git add .
git commit -m "Initial Unity project setup"
git push -u origin main
```

### Día 4: Setup Básico de Escena

```csharp
// Assets/Scripts/Managers/GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Game Settings")]
    public int totalFrames = 10;
    public int pinsPerFrame = 10;
    
    [Header("Prefab References")]
    public GameObject bowlingBallPrefab;
    public GameObject bowlingPinPrefab;
    public Transform ballSpawnPoint;
    public Transform[] pinPositions;
    
    private int currentFrame = 1;
    private int currentRoll = 1;
    private int totalScore = 0;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeGame();
    }
    
    void InitializeGame()
    {
        SetupBowlingLane();
        SetupPins();
        SpawnBowlingBall();
    }
    
    void SetupBowlingLane()
    {
        // Configurar la pista de boliche
        Debug.Log("Setting up bowling lane...");
    }
    
    void SetupPins()
    {
        // Configurar los pinos en formación triangular
        for (int i = 0; i < pinPositions.Length; i++)
        {
            if (pinPositions[i] != null)
            {
                Instantiate(bowlingPinPrefab, pinPositions[i].position, pinPositions[i].rotation);
            }
        }
    }
    
    void SpawnBowlingBall()
    {
        if (ballSpawnPoint != null && bowlingBallPrefab != null)
        {
            Instantiate(bowlingBallPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation);
        }
    }
}
```

---

## 🎳 Fase 2: Core Gameplay (Semanas 3-6)

### Sistema de Física de la Pelota

```csharp
// Assets/Scripts/Controllers/BallController.cs
using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Ball Physics")]
    public float throwForce = 500f;
    public float spinForce = 200f;
    public float maxSpeed = 15f;
    
    [Header("Input Settings")]
    public float swipeThreshold = 50f;
    public float aimSensitivity = 2f;
    
    private Rigidbody ballRigidbody;
    private Camera mainCamera;
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private bool isAiming = false;
    private bool hasBeenThrown = false;
    
    private LineRenderer trajectoryLine;
    
    void Start()
    {
        ballRigidbody = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        
        // Setup trajectory line
        trajectoryLine = GetComponent<LineRenderer>();
        if (trajectoryLine == null)
        {
            trajectoryLine = gameObject.AddComponent<LineRenderer>();
        }
        
        SetupTrajectoryLine();
    }
    
    void Update()
    {
        if (!hasBeenThrown)
        {
            HandleInput();
        }
        
        // Limitar velocidad máxima
        if (ballRigidbody.velocity.magnitude > maxSpeed)
        {
            ballRigidbody.velocity = ballRigidbody.velocity.normalized * maxSpeed;
        }
    }
    
    void HandleInput()
    {
        // Touch/Mouse input para móviles y PC
        if (Input.GetMouseButtonDown(0))
        {
            StartAiming();
        }
        else if (Input.GetMouseButton(0) && isAiming)
        {
            UpdateAiming();
        }
        else if (Input.GetMouseButtonUp(0) && isAiming)
        {
            ThrowBall();
        }
    }
    
    void StartAiming()
    {
        Vector3 mousePos = Input.mousePosition;
        startTouchPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCamera.nearClipPlane));
        isAiming = true;
        
        // Mostrar línea de trayectoria
        trajectoryLine.enabled = true;
    }
    
    void UpdateAiming()
    {
        Vector3 mousePos = Input.mousePosition;
        endTouchPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCamera.nearClipPlane));
        
        // Actualizar línea de trayectoria
        UpdateTrajectoryLine();
    }
    
    void ThrowBall()
    {
        if (hasBeenThrown) return;
        
        // Calcular dirección y fuerza
        Vector2 swipeDirection = endTouchPosition - startTouchPosition;
        float swipeDistance = swipeDirection.magnitude;
        
        if (swipeDistance > swipeThreshold)
        {
            // Aplicar fuerza a la pelota
            Vector3 forceDirection = new Vector3(swipeDirection.x, 0, swipeDirection.y).normalized;
            float forceMagnitude = Mathf.Clamp(swipeDistance * throwForce * 0.01f, 100f, throwForce);
            
            ballRigidbody.AddForce(forceDirection * forceMagnitude);
            
            // Aplicar spin si hay movimiento lateral
            if (Mathf.Abs(swipeDirection.x) > 10f)
            {
                Vector3 spinDirection = Vector3.up * Mathf.Sign(swipeDirection.x);
                ballRigidbody.AddTorque(spinDirection * spinForce);
            }
            
            hasBeenThrown = true;
            isAiming = false;
            trajectoryLine.enabled = false;
            
            // Notificar al GameManager
            GameManager.Instance?.OnBallThrown();
        }
    }
    
    void SetupTrajectoryLine()
    {
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.color = Color.white;
        trajectoryLine.startWidth = 0.1f;
        trajectoryLine.endWidth = 0.05f;
        trajectoryLine.positionCount = 50;
        trajectoryLine.enabled = false;
    }
    
    void UpdateTrajectoryLine()
    {
        Vector2 swipeDirection = endTouchPosition - startTouchPosition;
        Vector3 forceDirection = new Vector3(swipeDirection.x, 0, swipeDirection.y).normalized;
        float forceMagnitude = Mathf.Clamp(swipeDirection.magnitude * throwForce * 0.01f, 100f, throwForce);
        
        // Simular trayectoria
        Vector3[] trajectoryPoints = CalculateTrajectory(transform.position, forceDirection * forceMagnitude / ballRigidbody.mass);
        trajectoryLine.SetPositions(trajectoryPoints);
    }
    
    Vector3[] CalculateTrajectory(Vector3 startPos, Vector3 velocity)
    {
        Vector3[] points = new Vector3[trajectoryLine.positionCount];
        float timeStep = 0.1f;
        
        for (int i = 0; i < points.Length; i++)
        {
            float time = i * timeStep;
            points[i] = startPos + velocity * time + 0.5f * Physics.gravity * time * time;
        }
        
        return points;
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pin"))
        {
            // Manejar colisión con pino
            AudioManager.Instance?.PlaySFX("pin_hit");
        }
        else if (collision.gameObject.CompareTag("Lane"))
        {
            // Sonido de pelota rodando
            AudioManager.Instance?.PlaySFX("ball_roll");
        }
    }
}
```

### Sistema de Pinos

```csharp
// Assets/Scripts/Controllers/PinController.cs
using UnityEngine;

public class PinController : MonoBehaviour
{
    [Header("Pin Settings")]
    public float fallThreshold = 30f; // Ángulo para considerar caído
    public float resetForce = 10f;
    
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Rigidbody pinRigidbody;
    private bool isFallen = false;
    private bool wasHit = false;
    
    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        pinRigidbody = GetComponent<Rigidbody>();
        
        // Registrar este pino en el ScoreManager
        ScoreManager.Instance?.RegisterPin(this);
    }
    
    void Update()
    {
        CheckIfFallen();
    }
    
    void CheckIfFallen()
    {
        // Calcular ángulo de inclinación
        float angle = Vector3.Angle(Vector3.up, transform.up);
        
        if (angle > fallThreshold && !isFallen)
        {
            isFallen = true;
            ScoreManager.Instance?.OnPinFallen(this);
            
            // Efecto visual/sonoro
            AudioManager.Instance?.PlaySFX("pin_fall");
        }
    }
    
    public void ResetPin()
    {
        // Resetear posición y rotación
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        
        // Resetear física
        pinRigidbody.velocity = Vector3.zero;
        pinRigidbody.angularVelocity = Vector3.zero;
        
        // Resetear estado
        isFallen = false;
        wasHit = false;
    }
    
    public bool IsFallen()
    {
        return isFallen;
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball") && !wasHit)
        {
            wasHit = true;
            // Efecto de partículas opcional
            // ParticleSystem impact = GetComponent<ParticleSystem>();
            // if (impact) impact.Play();
        }
    }
}
```

### Sistema de Puntuación

```csharp
// Assets/Scripts/Managers/ScoreManager.cs
using UnityEngine;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    
    [Header("Scoring Settings")]
    public int strikeBonus = 10;
    public int spareBonus = 10;
    
    private List<PinController> pins = new List<PinController>();
    private List<int> frameScores = new List<int>();
    private int currentFrame = 1;
    private int currentRoll = 1;
    private int totalScore = 0;
    
    // Arrays para trackear puntajes por frame
    private int[] roll1Scores = new int[10];
    private int[] roll2Scores = new int[10];
    private int[] frameTotals = new int[10];
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void RegisterPin(PinController pin)
    {
        if (!pins.Contains(pin))
        {
            pins.Add(pin);
        }
    }
    
    public void OnPinFallen(PinController pin)
    {
        // Contar pinos caídos cuando termina el movimiento
        Invoke("CountFallenPins", 3f);
    }
    
    void CountFallenPins()
    {
        int fallenPins = 0;
        foreach (PinController pin in pins)
        {
            if (pin.IsFallen())
            {
                fallenPins++;
            }
        }
        
        UpdateScore(fallenPins);
    }
    
    void UpdateScore(int pinsDown)
    {
        if (currentRoll == 1)
        {
            roll1Scores[currentFrame - 1] = pinsDown;
            
            if (pinsDown == 10) // Strike
            {
                frameTotals[currentFrame - 1] = 10;
                OnStrike();
                NextFrame();
            }
            else
            {
                currentRoll = 2;
                UIManager.Instance?.UpdateRollDisplay(currentFrame, currentRoll);
            }
        }
        else if (currentRoll == 2)
        {
            roll2Scores[currentFrame - 1] = pinsDown - roll1Scores[currentFrame - 1];
            int frameTotal = roll1Scores[currentFrame - 1] + roll2Scores[currentFrame - 1];
            
            if (frameTotal == 10) // Spare
            {
                frameTotals[currentFrame - 1] = 10;
                OnSpare();
            }
            else
            {
                frameTotals[currentFrame - 1] = frameTotal;
            }
            
            NextFrame();
        }
        
        CalculateTotalScore();
        UIManager.Instance?.UpdateScoreDisplay(totalScore);
    }
    
    void OnStrike()
    {
        AudioManager.Instance?.PlaySFX("strike");
        UIManager.Instance?.ShowStrikeAnimation();
        
        // Bonus scoring se maneja en CalculateTotalScore()
    }
    
    void OnSpare()
    {
        AudioManager.Instance?.PlaySFX("spare");
        UIManager.Instance?.ShowSpareAnimation();
    }
    
    void NextFrame()
    {
        if (currentFrame < 10)
        {
            currentFrame++;
            currentRoll = 1;
            ResetPins();
            GameManager.Instance?.PrepareNextFrame();
        }
        else
        {
            // Juego terminado
            GameManager.Instance?.OnGameComplete(totalScore);
        }
    }
    
    void ResetPins()
    {
        foreach (PinController pin in pins)
        {
            pin.ResetPin();
        }
    }
    
    void CalculateTotalScore()
    {
        totalScore = 0;
        
        for (int frame = 0; frame < 10; frame++)
        {
            if (roll1Scores[frame] == 10) // Strike
            {
                totalScore += 10;
                if (frame < 9) // No es el frame 10
                {
                    totalScore += roll1Scores[frame + 1] + roll2Scores[frame + 1];
                }
            }
            else if (roll1Scores[frame] + roll2Scores[frame] == 10) // Spare
            {
                totalScore += 10;
                if (frame < 9)
                {
                    totalScore += roll1Scores[frame + 1];
                }
            }
            else
            {
                totalScore += roll1Scores[frame] + roll2Scores[frame];
            }
        }
    }
    
    public int GetCurrentFrame() { return currentFrame; }
    public int GetCurrentRoll() { return currentRoll; }
    public int GetTotalScore() { return totalScore; }
}
```

---

## 🎨 Fase 3: UI y Experiencia de Usuario (Semanas 7-10)

### Main Menu UI

```csharp
// Assets/Scripts/UI/MainMenuUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI References")]
    public Button playButton;
    public Button multiplayerButton;
    public Button settingsButton;
    public Button leaderboardButton;
    public Button quitButton;
    
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public GameObject leaderboardPanel;
    
    void Start()
    {
        SetupButtons();
        ShowMainPanel();
    }
    
    void SetupButtons()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        multiplayerButton.onClick.AddListener(OnMultiplayerClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }
    
    void OnPlayClicked()
    {
        AudioManager.Instance?.PlaySFX("button_click");
        SceneManager.LoadScene("GamePlay");
    }
    
    void OnMultiplayerClicked()
    {
        AudioManager.Instance?.PlaySFX("button_click");
        // Implementar lógica de multiplayer
        MultiplayerManager.Instance?.ShowMultiplayerMenu();
    }
    
    void OnSettingsClicked()
    {
        AudioManager.Instance?.PlaySFX("button_click");
        ShowSettingsPanel();
    }
    
    void OnLeaderboardClicked()
    {
        AudioManager.Instance?.PlaySFX("button_click");
        ShowLeaderboardPanel();
    }
    
    void OnQuitClicked()
    {
        AudioManager.Instance?.PlaySFX("button_click");
        Application.Quit();
    }
    
    void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
        leaderboardPanel.SetActive(false);
    }
    
    void ShowSettingsPanel()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
        leaderboardPanel.SetActive(false);
    }
    
    void ShowLeaderboardPanel()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(false);
        leaderboardPanel.SetActive(true);
    }
}
```

### Game HUD

```csharp
// Assets/Scripts/UI/GameHUD.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("Score Display")]
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI currentFrameText;
    public TextMeshProUGUI currentRollText;
    
    [Header("Frame Scorecard")]
    public Transform scorecardParent;
    public GameObject frameScorePrefab;
    
    [Header("Game Info")]
    public TextMeshProUGUI playerNameText;
    public Slider ballPowerSlider;
    public Button pauseButton;
    
    [Header("Animation Objects")]
    public GameObject strikeAnimation;
    public GameObject spareAnimation;
    
    private FrameScore[] frameScoreCards = new FrameScore[10];
    
    void Start()
    {
        SetupScorecard();
        SetupButtons();
        UpdateDisplay();
    }
    
    void SetupScorecard()
    {
        // Crear tarjetas de puntaje para cada frame
        for (int i = 0; i < 10; i++)
        {
            GameObject frameObj = Instantiate(frameScorePrefab, scorecardParent);
            frameScoreCards[i] = frameObj.GetComponent<FrameScore>();
            frameScoreCards[i].SetFrameNumber(i + 1);
        }
    }
    
    void SetupButtons()
    {
        pauseButton.onClick.AddListener(OnPauseClicked);
    }
    
    public void UpdateScoreDisplay(int totalScore)
    {
        totalScoreText.text = totalScore.ToString();
    }
    
    public void UpdateRollDisplay(int frame, int roll)
    {
        currentFrameText.text = $"Frame: {frame}";
        currentRollText.text = $"Roll: {roll}";
    }
    
    public void UpdateFrameScore(int frame, int roll1, int roll2, int total)
    {
        if (frame >= 1 && frame <= 10)
        {
            frameScoreCards[frame - 1].UpdateScore(roll1, roll2, total);
        }
    }
    
    public void ShowStrikeAnimation()
    {
        if (strikeAnimation != null)
        {
            strikeAnimation.SetActive(true);
            Invoke("HideStrikeAnimation", 2f);
        }
    }
    
    public void ShowSpareAnimation()
    {
        if (spareAnimation != null)
        {
            spareAnimation.SetActive(true);
            Invoke("HideSpareAnimation", 2f);
        }
    }
    
    void HideStrikeAnimation()
    {
        strikeAnimation.SetActive(false);
    }
    
    void HideSpareAnimation()
    {
        spareAnimation.SetActive(false);
    }
    
    void OnPauseClicked()
    {
        AudioManager.Instance?.PlaySFX("button_click");
        GameManager.Instance?.PauseGame();
    }
    
    void UpdateDisplay()
    {
        // Actualizar display cada frame
        if (ScoreManager.Instance != null)
        {
            UpdateScoreDisplay(ScoreManager.Instance.GetTotalScore());
            UpdateRollDisplay(ScoreManager.Instance.GetCurrentFrame(), ScoreManager.Instance.GetCurrentRoll());
        }
    }
    
    void Update()
    {
        UpdateDisplay();
    }
}
```

---

## 🌐 Fase 4: Multiplayer y Networking (Semanas 11-14)

### Setup de Unity Netcode

```csharp
// Assets/Scripts/Networking/NetworkManager.cs
using Unity.Netcode;
using UnityEngine;

public class XcapeNetworkManager : NetworkBehaviour
{
    public static XcapeNetworkManager Instance;
    
    [Header("Network Settings")]
    public int maxPlayers = 4;
    public float connectionTimeout = 30f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }
    
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
    
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }
    
    public void Disconnect()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
    
    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }
    
    void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected");
        
        // Notificar a todos los clientes
        UpdatePlayerCountClientRpc((int)NetworkManager.Singleton.ConnectedClientsIds.Count);
    }
    
    void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected");
        
        // Notificar a todos los clientes
        UpdatePlayerCountClientRpc((int)NetworkManager.Singleton.ConnectedClientsIds.Count);
    }
    
    [ClientRpc]
    void UpdatePlayerCountClientRpc(int playerCount)
    {
        UIManager.Instance?.UpdatePlayerCount(playerCount);
    }
}
```

### Multiplayer Game Logic

```csharp
// Assets/Scripts/Networking/MultiplayerGameManager.cs
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class MultiplayerGameManager : NetworkBehaviour
{
    [Header("Multiplayer Settings")]
    public int totalFramesPerPlayer = 10;
    public float turnTimeLimit = 60f;
    
    private NetworkVariable<int> currentPlayerTurn = new NetworkVariable<int>(0);
    private NetworkVariable<float> turnTimer = new NetworkVariable<float>(60f);
    
    private List<ulong> playerIds = new List<ulong>();
    private Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            InitializeMultiplayerGame();
        }
        
        currentPlayerTurn.OnValueChanged += OnPlayerTurnChanged;
        turnTimer.OnValueChanged += OnTurnTimerChanged;
    }
    
    void InitializeMultiplayerGame()
    {
        // Obtener lista de jugadores conectados
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            playerIds.Add(clientId);
            playerScores[clientId] = 0;
        }
        
        // Comenzar con el primer jugador
        currentPlayerTurn.Value = 0;
        turnTimer.Value = turnTimeLimit;
        
        NotifyGameStartClientRpc();
    }
    
    [ClientRpc]
    void NotifyGameStartClientRpc()
    {
        UIManager.Instance?.ShowMultiplayerGameStart();
    }
    
    void OnPlayerTurnChanged(int oldValue, int newValue)
    {
        ulong currentPlayerId = playerIds[newValue];
        bool isMyTurn = currentPlayerId == NetworkManager.Singleton.LocalClientId;
        
        UIManager.Instance?.UpdateTurnIndicator(isMyTurn, newValue + 1);
        
        if (isMyTurn)
        {
            EnablePlayerControls(true);
        }
        else
        {
            EnablePlayerControls(false);
        }
    }
    
    void OnTurnTimerChanged(float oldValue, float newValue)
    {
        UIManager.Instance?.UpdateTurnTimer(newValue);
        
        if (newValue <= 0 && IsServer)
        {
            // Tiempo agotado, pasar al siguiente jugador
            NextPlayerTurn();
        }
    }
    
    void Update()
    {
        if (IsServer && turnTimer.Value > 0)
        {
            turnTimer.Value -= Time.deltaTime;
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void SubmitScoreServerRpc(ulong playerId, int frameScore, int totalScore)
    {
        playerScores[playerId] = totalScore;
        
        // Notificar a todos los clientes del nuevo puntaje
        UpdatePlayerScoreClientRpc(playerId, frameScore, totalScore);
        
        // Pasar al siguiente jugador
        NextPlayerTurn();
    }
    
    [ClientRpc]
    void UpdatePlayerScoreClientRpc(ulong playerId, int frameScore, int totalScore)
    {
        UIManager.Instance?.UpdateMultiplayerScore(playerId, frameScore, totalScore);
    }
    
    void NextPlayerTurn()
    {
        if (!IsServer) return;
        
        currentPlayerTurn.Value = (currentPlayerTurn.Value + 1) % playerIds.Count;
        turnTimer.Value = turnTimeLimit;
        
        // Verificar si el juego ha terminado
        CheckGameEnd();
    }
    
    void CheckGameEnd()
    {
        // Verificar si todos los jugadores han completado todos los frames
        // Implementar lógica de fin de juego
    }
    
    void EnablePlayerControls(bool enable)
    {
        // Habilitar/deshabilitar controles del jugador
        BallController ballController = FindObjectOfType<BallController>();
        if (ballController != null)
        {
            ballController.enabled = enable;
        }
    }
}
```

---

## 💰 Fase 5: Monetización con AdMob (Semanas 15-16)

### Setup de Google AdMob

#### 1. Configuración en Unity
```bash
# Window -> Package Manager -> Unity Registry
# Buscar e instalar: "Google Mobile Ads Unity Plugin"
# O descargar desde: https://github.com/googleads/googleads-mobile-unity
```

#### 2. Configuración Android
```xml
<!-- Assets/Plugins/Android/AndroidManifest.xml -->
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
    <application>
        <!-- AdMob App ID -->
        <meta-data
            android:name="com.google.android.gms.ads.APPLICATION_ID"
            android:value="ca-app-pub-YOUR_ADMOB_APP_ID~YOUR_APP_ID"/>
    </application>
</manifest>
```

#### 3. Ad Manager Script
```csharp
// Assets/Scripts/Analytics/AdManager.cs
using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;
    
    [Header("Ad Unit IDs - Android")]
    public string androidBannerAdUnitId = "ca-app-pub-3940256099942544/6300978111"; // Test ID
    public string androidInterstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712"; // Test ID
    public string androidRewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917"; // Test ID
    
    [Header("Ad Unit IDs - iOS")]
    public string iosBannerAdUnitId = "ca-app-pub-3940256099942544/2934735716"; // Test ID
    public string iosInterstitialAdUnitId = "ca-app-pub-3940256099942544/4411468910"; // Test ID
    public string iosRewardedAdUnitId = "ca-app-pub-3940256099942544/1712485313"; // Test ID
    
    private BannerView bannerView;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;
    
    private string bannerAdUnitId;
    private string interstitialAdUnitId;
    private string rewardedAdUnitId;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAds();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeAds()
    {
        // Configurar Ad Unit IDs según plataforma
        #if UNITY_ANDROID
            bannerAdUnitId = androidBannerAdUnitId;
            interstitialAdUnitId = androidInterstitialAdUnitId;
            rewardedAdUnitId = androidRewardedAdUnitId;
        #elif UNITY_IOS
            bannerAdUnitId = iosBannerAdUnitId;
            interstitialAdUnitId = iosInterstitialAdUnitId;
            rewardedAdUnitId = iosRewardedAdUnitId;
        #endif
        
        // Inicializar Google Mobile Ads SDK
        MobileAds.Initialize(initStatus => {
            Debug.Log("Google Mobile Ads SDK initialized");
            LoadBannerAd();
            LoadInterstitialAd();
            LoadRewardedAd();
        });
    }
    
    #region Banner Ads
    void LoadBannerAd()
    {
        // Crear banner ad
        bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
        
        // Eventos del banner
        bannerView.OnBannerAdLoaded += OnBannerAdLoaded;
        bannerView.OnBannerAdLoadFailed += OnBannerAdLoadFailed;
        
        // Cargar el banner
        AdRequest request = new AdRequest();
        bannerView.LoadAd(request);
    }
    
    void OnBannerAdLoaded()
    {
        Debug.Log("Banner ad loaded");
    }
    
    void OnBannerAdLoadFailed(LoadAdError error)
    {
        Debug.LogError($"Banner ad failed to load: {error}");
    }
    
    public void ShowBannerAd()
    {
        if (bannerView != null)
        {
            bannerView.Show();
        }
    }
    
    public void HideBannerAd()
    {
        if (bannerView != null)
        {
            bannerView.Hide();
        }
    }
    #endregion
    
    #region Interstitial Ads
    void LoadInterstitialAd()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }
        
        AdRequest request = new AdRequest();
        InterstitialAd.Load(interstitialAdUnitId, request, OnInterstitialAdLoaded);
    }
    
    void OnInterstitialAdLoaded(InterstitialAd ad, LoadAdError error)
    {
        if (error != null)
        {
            Debug.LogError($"Interstitial ad failed to load: {error}");
            return;
        }
        
        interstitialAd = ad;
        
        // Registrar eventos
        interstitialAd.OnAdFullScreenContentClosed += OnInterstitialAdClosed;
        interstitialAd.OnAdFullScreenContentFailed += OnInterstitialAdFailedToShow;
    }
    
    public void ShowInterstitialAd()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.Show();
        }
        else
        {
            Debug.Log("Interstitial ad not ready");
            LoadInterstitialAd(); // Cargar uno nuevo
        }
    }
    
    void OnInterstitialAdClosed()
    {
        Debug.Log("Interstitial ad closed");
        LoadInterstitialAd(); // Cargar siguiente anuncio
    }
    
    void OnInterstitialAdFailedToShow(AdError error)
    {
        Debug.LogError($"Interstitial ad failed to show: {error}");
        LoadInterstitialAd();
    }
    #endregion
    
    #region Rewarded Ads
    void LoadRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
        
        AdRequest request = new AdRequest();
        RewardedAd.Load(rewardedAdUnitId, request, OnRewardedAdLoaded);
    }
    
    void OnRewardedAdLoaded(RewardedAd ad, LoadAdError error)
    {
        if (error != null)
        {
            Debug.LogError($"Rewarded ad failed to load: {error}");
            return;
        }
        
        rewardedAd = ad;
        
        // Registrar eventos
        rewardedAd.OnAdFullScreenContentClosed += OnRewardedAdClosed;
        rewardedAd.OnAdFullScreenContentFailed += OnRewardedAdFailedToShow;
    }
    
    public void ShowRewardedAd(Action<bool> onComplete)
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) => {
                Debug.Log($"Rewarded ad completed. Reward: {reward.Amount} {reward.Type}");
                onComplete?.Invoke(true);
            });
        }
        else
        {
            Debug.Log("Rewarded ad not ready");
            onComplete?.Invoke(false);
            LoadRewardedAd();
        }
    }
    
    void OnRewardedAdClosed()
    {
        Debug.Log("Rewarded ad closed");
        LoadRewardedAd();
    }
    
    void OnRewardedAdFailedToShow(AdError error)
    {
        Debug.LogError($"Rewarded ad failed to show: {error}");
        LoadRewardedAd();
    }
    #endregion
    
    void OnDestroy()
    {
        bannerView?.Destroy();
        interstitialAd?.Destroy();
        rewardedAd?.Destroy();
    }
}
```

### Integración de Anuncios en el Gameplay

```csharp
// Integrar en GameManager.cs
public class GameManager : MonoBehaviour
{
    [Header("Ad Settings")]
    public int gamesUntilInterstitial = 3;
    private int gamesPlayed = 0;
    
    void Start()
    {
        // Mostrar banner al iniciar
        AdManager.Instance?.ShowBannerAd();
    }
    
    public void OnGameComplete(int finalScore)
    {
        gamesPlayed++;
        
        // Mostrar anuncio intersticial cada X juegos
        if (gamesPlayed >= gamesUntilInterstitial)
        {
            AdManager.Instance?.ShowInterstitialAd();
            gamesPlayed = 0;
        }
        
        // Mostrar pantalla de resultados
        UIManager.Instance?.ShowGameResults(finalScore);
    }
    
    public void ShowRewardedAdForExtraBall()
    {
        AdManager.Instance?.ShowRewardedAd((success) => {
            if (success)
            {
                // Dar pelota extra al jugador
                GrantExtraBall();
            }
        });
    }
    
    void GrantExtraBall()
    {
        // Lógica para dar pelota extra
        Debug.Log("Extra ball granted!");
    }
}
```

---

## 🧪 Fase 6: Testing y Deployment (Semanas 17-21)

### Build Settings y Configuración

#### Android Build Configuration
```csharp
// Build Settings -> Player Settings -> Android
/* 
Company Name: Tu Empresa
Product Name: XCAPE
Package Name: com.tuempresa.xcape
Version: 1.0.0
Bundle Version Code: 1

XR Settings: None
Resolution and Presentation:
- Default Orientation: Portrait
- Allowed Orientations: Portrait, Portrait Upside Down

Icon: 
- Adaptive (Android API 26+): Configurar
- Legacy (Android API 25 and earlier): Configurar

Splash Image:
- Virtual Reality Splash Image: Configurar

Other Settings:
- Rendering:
  * Color Space: Linear
  * Auto Graphics API: Unchecked
  * Graphics APIs: OpenGLES3, OpenGLES2
- Identification:
  * Minimum API Level: Android 7.0 'Nougat' (API level 24)
  * Target API Level: Automatic (highest installed)
- Configuration:
  * Scripting Backend: IL2CPP
  * Api Compatibility Level: .NET Standard 2.1
  * Target Architectures: ARM64 ✓, ARMv7 ✓
- Optimization:
  * Managed Stripping Level: Minimal
*/
```

#### iOS Build Configuration
```csharp
// Build Settings -> Player Settings -> iOS
/*
Target minimum iOS Version: 12.0
Target Device Family: iPhone & iPad
Requires ARKit support: Unchecked

Architecture: ARM64

Camera Usage Description: "This app uses camera to capture photos of your pets"
Location Usage Description: "This app uses location to show nearby lost pets"
*/
```

### Scripts de Build Automatizado

```csharp
// Assets/Editor/BuildScript.cs
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

public class BuildScript
{
    static string[] SCENES = FindEnabledEditorScenes();
    
    [MenuItem("Build/Build Android APK")]
    static void BuildAndroid()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = SCENES;
        buildPlayerOptions.locationPathName = "Builds/Android/XCAPE.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;
        
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;
        
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Android build succeeded: " + summary.totalSize + " bytes");
            EditorUtility.RevealInFinder("Builds/Android/");
        }
        
        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Android build failed");
        }
    }
    
    [MenuItem("Build/Build iOS Xcode Project")]
    static void BuildiOS()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = SCENES;
        buildPlayerOptions.locationPathName = "Builds/iOS";
        buildPlayerOptions.target = BuildTarget.iOS;
        buildPlayerOptions.options = BuildOptions.None;
        
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;
        
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("iOS build succeeded");
            EditorUtility.RevealInFinder("Builds/iOS/");
        }
        
        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("iOS build failed");
        }
    }
    
    private static string[] FindEnabledEditorScenes()
    {
        var editorScenes = new System.Collections.Generic.List<string>();
        
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                editorScenes.Add(scene.path);
            }
        }
        
        return editorScenes.ToArray();
    }
}
```

### Verificación de Políticas de Store

#### Google Play Store Checklist
- [ ] **Target API Level**: Android 13 (API level 33) o superior
- [ ] **64-bit Support**: Requerido
- [ ] **App Bundle**: Usar AAB en lugar de APK
- [ ] **Privacy Policy**: URL válida en Play Console
- [ ] **Content Rating**: Completar cuestionario IARC
- [ ] **Permissions**: Justificar cada permiso en la descripción
- [ ] **Data Safety**: Completar sección de seguridad de datos
- [ ] **Ads Content**: Cumplir política de contenido publicitario

#### Apple App Store Checklist
- [ ] **iOS Version**: Soporte iOS 12.0+
- [ ] **64-bit**: Arquitectura ARM64
- [ ] **App Privacy**: Completar etiquetas de privacidad
- [ ] **Age Rating**: Apropiado para todas las edades
- [ ] **In-App Purchases**: Configurar si aplicable
- [ ] **Game Center**: Integrar si se usa leaderboards
- [ ] **TestFlight**: Beta testing antes de release

### Testing Automatizado

```csharp
// Assets/Tests/EditMode/ScoreManagerTests.cs
using NUnit.Framework;
using UnityEngine;

public class ScoreManagerTests
{
    private ScoreManager scoreManager;
    
    [SetUp]
    public void Setup()
    {
        GameObject gameObject = new GameObject();
        scoreManager = gameObject.AddComponent<ScoreManager>();
    }
    
    [Test]
    public void Strike_CalculatesCorrectScore()
    {
        // Arrange: Setup strike scenario
        
        // Act: Simulate strike
        
        // Assert: Verify score calculation
        Assert.AreEqual(30, scoreManager.GetTotalScore());
    }
    
    [Test]
    public void Spare_CalculatesCorrectScore()
    {
        // Test spare scoring logic
    }
    
    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(scoreManager.gameObject);
    }
}
```

---

## 📱 Comandos Finales de Build

### Android Release Build
```bash
# En Unity:
# File -> Build Settings -> Android
# Build System: Gradle
# Export Project: Unchecked (para APK directo)
# Create App Bundle: Checked (recomendado para Play Store)

# Configurar Keystore:
# Publishing Settings -> 
# Create New Keystore -> Guardar como: XCAPE_Release.keystore
# Keystore password: [tu_password_seguro]
# Key Alias: xcape_key
# Key password: [tu_password_seguro]

# Build -> Build App Bundle
```

### iOS Release Build
```bash
# En Unity:
# File -> Build Settings -> iOS
# Build

# En Xcode (Mac):
# Abrir proyecto generado en Builds/iOS/
# Configurar Team ID en Signing & Capabilities
# Archive -> Distribute App -> App Store Connect
```

---

## 🎯 Métricas de Éxito

### KPIs Técnicos
- **Performance**: 60 FPS en dispositivos target
- **Load Time**: < 3 segundos inicio de juego
- **Crash Rate**: < 1%
- **App Size**: < 150MB

### KPIs de Engagement
- **Session Length**: 5+ minutos promedio
- **Retention D1**: 40%+
- **Retention D7**: 20%+
- **Games per Session**: 3+

### KPIs de Monetización
- **Ad Fill Rate**: 90%+
- **eCPM**: $1.00+
- **ARPDAU**: $0.03+
- **IAP Conversion**: 2%+

---

## 🆘 Troubleshooting Común

### Unity Issues
```bash
# Error: "Failed to build il2cpp"
# Solución: Actualizar Visual Studio Build Tools

# Error: "Android SDK not found"
# Solución: Edit -> Preferences -> External Tools -> Android SDK

# Error: "Gradle build failed"
# Solución: Verificar Android SDK/NDK versiones en Unity Hub
```

### AdMob Issues
```bash
# Error: "The Google Mobile Ads Unity plugin was not properly initialized"
# Solución: Verificar Application ID en AndroidManifest.xml

# Error: "No ads to show"
# Solución: Verificar Ad Unit IDs y que AdMob esté configurado
```

---

## 📚 Recursos Útiles

### Documentación
- [Unity Manual](https://docs.unity3d.com/Manual/index.html)
- [Unity Scripting API](https://docs.unity3d.com/ScriptReference/)
- [Google Mobile Ads Unity Plugin](https://developers.google.com/admob/unity/quick-start)
- [Unity Netcode Documentation](https://docs-multiplayer.unity3d.com/)

### Assets Store (Opcionales)
- **Physics**: "Realistic Physics Pro"
- **Audio**: "Master Audio"
- **UI**: "NGUI" o "UI Toolkit"
- **Analytics**: "Unity Analytics"

---

**¡Listo para crear XCAPE! 🎳🚀**

**Tiempo estimado total: 15-21 semanas**  
**Resultado: Juego de boliche 3D completo, monetizado y listo para stores**