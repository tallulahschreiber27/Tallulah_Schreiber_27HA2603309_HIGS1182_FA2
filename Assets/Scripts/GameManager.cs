using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Score & Victory Configuration")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText; // Added for Section A high score display
    [SerializeField] private int targetWinScore = 10;

    [Header("In-Scene UI Overlay Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

    [Header("Scene Navigation Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string gameplayScene = "Gameplay";

    private int currentScore = 0;
    private int persistentHighScore = 0; // Added to cache the loaded high score
    private bool isGameActive = true;
    private bool isPaused = false;

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

        // Load the persistent high score using our static SaveManager method (Rubric requirement)
        persistentHighScore = SaveManager.LoadHighScore();

        // Ensure all UI overlay screens are hidden on startup
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
    }

    public void AddScore(int points)
    {
        if (!isGameActive) return;

        currentScore += points;

        // Dynamic milestone tracking: If player beats the high score mid-game, update it visually
        if (currentScore > persistentHighScore)
        {
            persistentHighScore = currentScore;
        }

        UpdateScoreUI();

        Debug.Log($"[Score Update] Scavenged item processed. Score: {currentScore}/{targetWinScore}");

        if (currentScore >= targetWinScore)
        {
            TriggerWinState();
        }
    }

    public void SetTargetWinScore(int amount)
    {
        targetWinScore = amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Scrap Collected: " + currentScore + " / " + targetWinScore;
        }

        // Keep the high score text component up to date on your HUD/Screen
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
            Debug.Log("[Game Loop] System execution paused by player.");
        }
        else
        {
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
            Debug.Log("[Game Loop] System execution resumed.");
        }
    }

    public void TriggerGameOver()
    {
        if (!isGameActive) return;

        isGameActive = false;
        Time.timeScale = 0f;

        // --- SECTION A REQUIREMENT: Save data to disk upon death ---
        SaveManager.SaveHighScore(currentScore);

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Debug.Log("[Game State] Drone hull compromised. Game Over display initialized.");
    }

    private void TriggerWinState()
    {
        if (!isGameActive) return;

        isGameActive = false;
        Time.timeScale = 0f;

        // --- SECTION A REQUIREMENT: Save data to disk upon winning ---
        SaveManager.SaveHighScore(currentScore);

        if (winPanel != null) winPanel.SetActive(true);
        Debug.Log("[Game State] Scavenger milestones completed. Victory menu displayed.");
    }

    public void RestartGame()
    {
        Debug.Log("GameManager: Re-initializing active gameplay arena scene elements.");
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplayScene);
    }

    public void ReturnToMainMenu()
    {
        Debug.Log("GameManager: Exiting gameplay layout environment context.");
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void StartGame()
    {
        Debug.Log($"GameManager: Loading core gameplay scene context: '{gameplayScene}'");
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplayScene);
    }
}
