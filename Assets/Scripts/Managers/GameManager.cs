using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace XCAPE.Core
{
    /// <summary>
    /// GameManager principal que controla el flujo del juego XCAPE Bowling
    /// Maneja estados, transiciones, configuración y coordinación entre sistemas
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Singleton Pattern
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                    if (_instance == null)
                    {
                        GameObject gameManagerGO = new GameObject("GameManager");
                        _instance = gameManagerGO.AddComponent<GameManager>();
                        DontDestroyOnLoad(gameManagerGO);
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Game States
        public enum GameState
        {
            MainMenu,
            Loading,
            Gameplay,
            Paused,
            GameOver,
            Settings,
            Leaderboards,
            Multiplayer
        }

        [Header("Game State")]
        [SerializeField] private GameState currentGameState = GameState.MainMenu;
        [SerializeField] private GameState previousGameState = GameState.MainMenu;
        #endregion

        #region Game Settings
        [Header("Game Configuration")]
        [SerializeField] private float targetFrameRate = 60f;
        [SerializeField] private bool enableVSync = true;
        [SerializeField] private bool enablePerformanceMode = false;
        
        [Header("Audio Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.8f;
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 1f;
        #endregion

        #region Player Data
        [Header("Player Progress")]
        [SerializeField] private int playerLevel = 1;
        [SerializeField] private int playerXP = 0;
        [SerializeField] private int totalGamesPlayed = 0;
        [SerializeField] private int highScore = 0;
        [SerializeField] private float averageScore = 0f;
        #endregion

        #region Events
        public System.Action<GameState> OnGameStateChanged;
        public System.Action<int> OnScoreChanged;
        public System.Action<int> OnLevelChanged;
        public System.Action OnGameStarted;
        public System.Action OnGameEnded;
        public System.Action OnGamePaused;
        public System.Action OnGameResumed;
        #endregion

        #region Manager References
        [Header("Manager References")]
        public ScoreManager ScoreManager { get; private set; }
        public AudioManager AudioManager { get; private set; }
        public UIManager UIManager { get; private set; }
        public InputManager InputManager { get; private set; }
        public AdManager AdManager { get; private set; }
        public AnalyticsManager AnalyticsManager { get; private set; }
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            // Singleton pattern enforcement
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize core systems
            InitializeManagers();
            ConfigureApplication();
        }

        void Start()
        {
            StartCoroutine(InitializeGameSystems());
        }

        void Update()
        {
            // Handle back button (Android)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleBackButton();
            }

            // Update managers
            UpdateManagers();
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                PauseGame();
                SaveGameData();
            }
            else
            {
                if (currentGameState == GameState.Paused)
                {
                    // Don't auto-resume, let player choose
                }
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && currentGameState == GameState.Gameplay)
            {
                PauseGame();
            }
        }
        #endregion

        #region Initialization
        private void InitializeManagers()
        {
            // Find or create manager instances
            ScoreManager = FindObjectOfType<ScoreManager>();
            AudioManager = FindObjectOfType<AudioManager>();
            UIManager = FindObjectOfType<UIManager>();
            InputManager = FindObjectOfType<InputManager>();
            AdManager = FindObjectOfType<AdManager>();
            AnalyticsManager = FindObjectOfType<AnalyticsManager>();

            Debug.Log("[GameManager] Managers initialized");
        }

        private void ConfigureApplication()
        {
            // Set target frame rate
            Application.targetFrameRate = (int)targetFrameRate;

            // Configure VSync
            QualitySettings.vSyncCount = enableVSync ? 1 : 0;

            // Prevent screen dimming
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // Set screen orientation to landscape
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;

            Debug.Log("[GameManager] Application configured");
        }

        private IEnumerator InitializeGameSystems()
        {
            SetGameState(GameState.Loading);

            // Load player data
            yield return StartCoroutine(LoadPlayerData());

            // Initialize audio system
            if (AudioManager != null)
            {
                AudioManager.SetMasterVolume(masterVolume);
                AudioManager.SetMusicVolume(musicVolume);
                AudioManager.SetSFXVolume(sfxVolume);
            }

            // Initialize ads
            if (AdManager != null)
            {
                AdManager.Initialize();
            }

            // Initialize analytics
            if (AnalyticsManager != null)
            {
                AnalyticsManager.Initialize();
                AnalyticsManager.LogEvent("game_started");
            }

            yield return new WaitForSeconds(1f); // Simulate loading time

            // Transition to main menu
            SetGameState(GameState.MainMenu);

            Debug.Log("[GameManager] Game systems initialized");
        }
        #endregion

        #region Game State Management
        public void SetGameState(GameState newState)
        {
            if (currentGameState == newState) return;

            previousGameState = currentGameState;
            currentGameState = newState;

            Debug.Log($"[GameManager] State changed from {previousGameState} to {currentGameState}");

            // Handle state-specific logic
            switch (currentGameState)
            {
                case GameState.MainMenu:
                    HandleMainMenuState();
                    break;
                case GameState.Gameplay:
                    HandleGameplayState();
                    break;
                case GameState.Paused:
                    HandlePausedState();
                    break;
                case GameState.GameOver:
                    HandleGameOverState();
                    break;
            }

            OnGameStateChanged?.Invoke(currentGameState);
        }

        private void HandleMainMenuState()
        {
            Time.timeScale = 1f;
            if (AudioManager != null)
            {
                AudioManager.PlayMenuMusic();
            }
        }

        private void HandleGameplayState()
        {
            Time.timeScale = 1f;
            if (AudioManager != null)
            {
                AudioManager.PlayGameplayMusic();
            }
            OnGameStarted?.Invoke();
        }

        private void HandlePausedState()
        {
            Time.timeScale = 0f;
            if (AudioManager != null)
            {
                AudioManager.PauseMusic();
            }
            OnGamePaused?.Invoke();
        }

        private void HandleGameOverState()
        {
            Time.timeScale = 1f;
            if (AudioManager != null)
            {
                AudioManager.PlayGameOverSound();
            }
            OnGameEnded?.Invoke();
        }

        public GameState GetCurrentGameState()
        {
            return currentGameState;
        }

        public GameState GetPreviousGameState()
        {
            return previousGameState;
        }
        #endregion

        #region Game Control
        public void StartNewGame()
        {
            if (ScoreManager != null)
            {
                ScoreManager.ResetGame();
            }

            SetGameState(GameState.Gameplay);

            if (AnalyticsManager != null)
            {
                AnalyticsManager.LogEvent("new_game_started");
            }
        }

        public void PauseGame()
        {
            if (currentGameState == GameState.Gameplay)
            {
                SetGameState(GameState.Paused);
            }
        }

        public void ResumeGame()
        {
            if (currentGameState == GameState.Paused)
            {
                SetGameState(GameState.Gameplay);
                if (AudioManager != null)
                {
                    AudioManager.ResumeMusic();
                }
                OnGameResumed?.Invoke();
            }
        }

        public void EndGame()
        {
            if (ScoreManager != null)
            {
                int finalScore = ScoreManager.GetTotalScore();
                if (finalScore > highScore)
                {
                    highScore = finalScore;
                    SavePlayerData();
                }

                totalGamesPlayed++;
                UpdateAverageScore(finalScore);
            }

            SetGameState(GameState.GameOver);

            // Show ad after game (if appropriate)
            if (AdManager != null && totalGamesPlayed % 3 == 0) // Every 3rd game
            {
                AdManager.ShowInterstitialAd();
            }
        }

        public void ReturnToMainMenu()
        {
            SetGameState(GameState.MainMenu);
            // Load main menu scene if different from current
            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                SceneManager.LoadScene("MainMenu");
            }
        }

        public void QuitGame()
        {
            SaveGameData();

            if (AnalyticsManager != null)
            {
                AnalyticsManager.LogEvent("game_quit");
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion

        #region Input Handling
        private void HandleBackButton()
        {
            switch (currentGameState)
            {
                case GameState.Gameplay:
                    PauseGame();
                    break;
                case GameState.Paused:
                    ResumeGame();
                    break;
                case GameState.Settings:
                case GameState.Leaderboards:
                    SetGameState(GameState.MainMenu);
                    break;
                case GameState.MainMenu:
                    QuitGame();
                    break;
            }
        }
        #endregion

        #region Manager Updates
        private void UpdateManagers()
        {
            // Update any managers that need per-frame updates
            InputManager?.UpdateInput();
        }
        #endregion

        #region Player Data Management
        private IEnumerator LoadPlayerData()
        {
            // Load from PlayerPrefs (later can be expanded to cloud save)
            playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
            playerXP = PlayerPrefs.GetInt("PlayerXP", 0);
            totalGamesPlayed = PlayerPrefs.GetInt("TotalGamesPlayed", 0);
            highScore = PlayerPrefs.GetInt("HighScore", 0);
            averageScore = PlayerPrefs.GetFloat("AverageScore", 0f);

            // Audio settings
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

            yield return null; // Simulate async loading
        }

        public void SavePlayerData()
        {
            PlayerPrefs.SetInt("PlayerLevel", playerLevel);
            PlayerPrefs.SetInt("PlayerXP", playerXP);
            PlayerPrefs.SetInt("TotalGamesPlayed", totalGamesPlayed);
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.SetFloat("AverageScore", averageScore);

            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);

            PlayerPrefs.Save();
        }

        private void UpdateAverageScore(int newScore)
        {
            averageScore = ((averageScore * (totalGamesPlayed - 1)) + newScore) / totalGamesPlayed;
        }

        public void AddXP(int xp)
        {
            playerXP += xp;
            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            int xpForNextLevel = playerLevel * 1000; // Simple level progression
            if (playerXP >= xpForNextLevel)
            {
                playerLevel++;
                playerXP -= xpForNextLevel;
                OnLevelChanged?.Invoke(playerLevel);

                if (AnalyticsManager != null)
                {
                    AnalyticsManager.LogEvent("level_up", "level", playerLevel);
                }
            }
        }
        #endregion

        #region Audio Settings
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            if (AudioManager != null)
            {
                AudioManager.SetMasterVolume(masterVolume);
            }
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (AudioManager != null)
            {
                AudioManager.SetMusicVolume(musicVolume);
            }
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (AudioManager != null)
            {
                AudioManager.SetSFXVolume(sfxVolume);
            }
        }
        #endregion

        #region Getters
        public int GetPlayerLevel() => playerLevel;
        public int GetPlayerXP() => playerXP;
        public int GetTotalGamesPlayed() => totalGamesPlayed;
        public int GetHighScore() => highScore;
        public float GetAverageScore() => averageScore;
        public float GetMasterVolume() => masterVolume;
        public float GetMusicVolume() => musicVolume;
        public float GetSFXVolume() => sfxVolume;
        #endregion

        #region Performance Settings
        public void SetPerformanceMode(bool enabled)
        {
            enablePerformanceMode = enabled;
            
            if (enabled)
            {
                // Reduce quality for better performance
                QualitySettings.SetQualityLevel(1); // Low quality
                Application.targetFrameRate = 30;
            }
            else
            {
                // Restore normal quality
                QualitySettings.SetQualityLevel(3); // High quality
                Application.targetFrameRate = 60;
            }
        }

        public void SetTargetFrameRate(float frameRate)
        {
            targetFrameRate = frameRate;
            Application.targetFrameRate = (int)targetFrameRate;
        }

        public void SetVSync(bool enabled)
        {
            enableVSync = enabled;
            QualitySettings.vSyncCount = enabled ? 1 : 0;
        }
        #endregion

        #region Debug
        void OnGUI()
        {
            if (Debug.isDebugBuild)
            {
                GUI.Label(new Rect(10, 10, 200, 20), $"State: {currentGameState}");
                GUI.Label(new Rect(10, 30, 200, 20), $"FPS: {1f / Time.unscaledDeltaTime:F1}");
                GUI.Label(new Rect(10, 50, 200, 20), $"Level: {playerLevel}");
                GUI.Label(new Rect(10, 70, 200, 20), $"High Score: {highScore}");
            }
        }
        #endregion
    }
}
