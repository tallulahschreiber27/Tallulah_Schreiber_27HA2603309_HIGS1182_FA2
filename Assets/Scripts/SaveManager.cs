using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private const string HighScoreKey = "AsteroidScavenger_HighScore";

    /// <summary>
    /// Custom method to save a new high score using PlayerPrefs (Section A/B requirement)
    /// </summary>
    public static void SaveHighScore(int scoreToSave)
    {
        int currentHighScore = LoadHighScore();

        // Only overwrite if the new score is higher
        if (scoreToSave > currentHighScore)
        {
            PlayerPrefs.SetInt(HighScoreKey, scoreToSave);
            PlayerPrefs.Save(); // Forces Unity to write the data to disk immediately
            Debug.Log($"[SaveManager] New High Score Saved: {scoreToSave} scrap!");
        }
    }

    /// <summary>
    /// Custom method to load the saved high score (Section A/B requirement)
    /// </summary>
    public static int LoadHighScore()
    {
        // Retrieves the saved integer, or returns 0 if no high score exists yet
        int savedScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        return savedScore;
    }
}
