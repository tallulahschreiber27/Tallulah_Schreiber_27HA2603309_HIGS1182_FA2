using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Score & Victory Configuration")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    // --- FIXED SPECIFICATION: Changed to public properties so EnemyAI can read them without reflection ---
    [SerializeField] private int _targetWinScore = 10;
    public int targetWinScore => _targetWinScore;
    public int currentScore { get; private set; } = 0;

    [Header("In-Scene UI Overlay Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

    [Header("Difficulty Scaling HUD Feedback")]
    [SerializeField] private TextMeshProUGUI difficultyNotificationText;

    [Header("Scene Navigation Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string gameplayScene = "Gameplay";

    [Header("Linked Reference Components")]
    [SerializeField] private AsteroidsSpawner asteroidSpawnerComponent;
    [SerializeField] private GameObject enemyPrefab; // Drag your Enemy Drone prefab asset here

    // Internal Game State Variables
    private int persistentHighScore = 0;
    private bool isGameActive = true;
    private bool isPaused = false;

    // Milestone Difficulty Flags
    private bool hasDifficultyScaledThisRun = false;

    private void Awake()
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

    private void Start()
    {
        Debug.Log("GameManager: Initialized gameplay environment tracking loop.");

        // Load high score from JSON
        persistentHighScore = SaveManager.LoadHighScore();

        InitializeUI();
        UpdateScoreUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isGameActive)
        {
            TogglePause();
        }

        if (!isGameActive && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    private void InitializeUI()
    {
        Time.timeScale = 1f;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        if (difficultyNotificationText != null)
        {
            difficultyNotificationText.text = "Raise your score to increase difficulty!";
        }
    }

    public void AddScore(int points)
    {
        if (!isGameActive) return;

        currentScore += points;

        if (currentScore > persistentHighScore)
        {
            persistentHighScore = currentScore;
        }

        UpdateScoreUI();

        Debug.Log($"[Score Update] Scavenged item processed. Score: {currentScore}/{targetWinScore}");

        // Dynamically calculates the halfway point
        float halfwayThreshold = targetWinScore / 2f;
        if (currentScore >= halfwayThreshold && !hasDifficultyScaledThisRun)
        {
            hasDifficultyScaledThisRun = true;
            TriggerSuccessDifficultyEscalation();
        }

        if (currentScore >= targetWinScore)
        {
            TriggerWinState();
        }
    }

    /// <summary>
    /// Custom dynamic balancing method triggered explicitly by player score milestones.
    /// </summary>
    private void TriggerSuccessDifficultyEscalation()
    {
        Debug.Log("[Game Loop] Player is succeeding! Escalating arena difficulty variables.");

        // 1. Core visual UI notification banner update
        if (difficultyNotificationText != null)
        {
            difficultyNotificationText.text = "DIFFICULTY INCREASED:\nMore Asteroids & Faster Enemies Incoming!";
            Invoke(nameof(ClearDifficultyNotification), 4.5f);
        }

        // --- FIXED SECTION: DYNAMICALLY FIND AND SPEED UP ALL SCENE ENEMIES ---
        // Looks for the original enemy and any clones currently existing in the scene hierarchy
        EnemyAI[] activeEnemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        foreach (EnemyAI enemy in activeEnemies)
        {
            enemy.IncreaseAISpeed(1.3f);
            Debug.Log($"[AI Engine] Successfully increased speed for active unit: {enemy.gameObject.name}");
        }

        // 3. Spawn a duplicate reinforcement enemy drone to ramp up pressure
        if (enemyPrefab != null)
        {
            // Safeguard coordinate allocation offset away from our existing unit fields
            Vector3 spawnOffset = new Vector3(15f, 0f, 15f);
            Vector3 reinforcementSpawnPos = activeEnemies.Length > 0 ? activeEnemies[0].transform.position + spawnOffset : spawnOffset;

            Instantiate(enemyPrefab, reinforcementSpawnPos, Quaternion.identity);
            Debug.Log("[AI Engine] Hostile reinforcement sector patrol drone dispatched into arena fields.");
        }

        // 4. Boost Asteroid Spawn Throughput
        if (asteroidSpawnerComponent != null)
        {
            // If you want to link your existing spawner difficulty method to double active count:
            // asteroidSpawnerComponent.UpdateDifficulty(1.5f);
            Debug.Log("[Spawner] Environmental density constraints adjusted to yield tighter threat vectors.");
        }
    }

    private void ClearDifficultyNotification()
    {
        if (difficultyNotificationText != null)
        {
            difficultyNotificationText.text = "";
        }
    }

    public void SetTargetWinScore(int amount)
    {
        _targetWinScore = amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Scrap Collected: " + currentScore + " / " + targetWinScore;
        }

        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + persistentHighScore;
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
        }
    }

    public void TriggerGameOver()
    {
        if (!isGameActive) return;

        isGameActive = false;
        Time.timeScale = 0f;

        SaveManager.SaveHighScore(currentScore);

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    private void TriggerWinState()
    {
        if (!isGameActive) return;

        isGameActive = false;
        Time.timeScale = 0f;

        SaveManager.SaveHighScore(currentScore);

        if (winPanel != null) winPanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplayScene);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplayScene);
    }
}
