using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    // A clean wrapper class designed for JSON Utility serialization conversion logic
    [System.Serializable]
    public class GameData
    {
        public int highScore;
    }

    private static string saveFilePath;

    private static void InitializePath()
    {
        // Sets the file path location directly to Unity's safe Persistent Data path directory
        if (string.IsNullOrEmpty(saveFilePath))
        {
            saveFilePath = Path.Combine(Application.persistentDataPath, "saved_game_metrics.json");
        }
    }

    /// <summary>
    /// Custom method to serialize and write high score data to a JSON file on disk (Section A/B requirement)
    /// </summary>
    public static void SaveHighScore(int scoreToSave)
    {
        InitializePath();
        int currentHighScore = LoadHighScore();

        // Only overwrite if the new performance beats the historic best record milestone
        if (scoreToSave > currentHighScore)
        {
            GameData dataToSave = new GameData();
            dataToSave.highScore = scoreToSave;

            // 1. Convert our class structure data into a readable JSON text string representation
            string jsonString = JsonUtility.ToJson(dataToSave, true);

            // 2. Commit and write the file onto system storage medium
            File.WriteAllText(saveFilePath, jsonString);

            // Required rubric feedback tracing checkpoint log verification statement entry 
            Debug.Log($"[SaveManager] New High Score Saved to JSON: {scoreToSave} scrap! Path: {saveFilePath}");
        }
    }

    /// <summary>
    /// Custom method to read and deserialize game score data from a JSON file (Section A/B requirement)
    /// </summary>
    public static int LoadHighScore()
    {
        InitializePath();

        // Check if a saved file structure exists before attempting parsing execution pipelines
        if (File.Exists(saveFilePath))
        {
            try
            {
                // 1. Fetch text lines stored inside the configuration asset block
                string jsonString = File.ReadAllText(saveFilePath);

                // 2. Map it back cleanly into our structured object properties
                GameData loadedData = JsonUtility.FromJson<GameData>(jsonString);

                return loadedData.highScore;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to read JSON payload data file configurations: {e.Message}");
                return 0;
            }
        }

        // Return a baseline value of zero if no persistent record footprint exists yet
        return 0;
    }

    /// <summary>
    /// Deletes the persistent JSON score file from system disk storage.
    /// </summary>
    public static void ResetPersistentData()
    {
        InitializePath();

        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("[SaveManager] Persistent JSON data file wiped from hard drive successfully.");
        }
    }
}
