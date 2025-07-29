using UnityEngine;
using TMPro; // Required for TextMeshPro UI elements

/// <summary>
/// Manages overall game state, including score and potentially game over conditions.
/// This should be a singleton (only one instance in the scene).
/// </summary>
public class GameManager : MonoBehaviour
{
    // Public static instance to allow other scripts to easily access the GameManager.
    public static GameManager Instance { get; private set; }

    private int currentScore = 0; // The player's current score.

    [Header("UI References")]
    [SerializeField]
    private TextMeshProUGUI scoreTextDisplay; // Assign the TextMeshProUGUI element for score.

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Sets up the singleton instance.
    /// </summary>
    void Awake()
    {
        // Implement singleton pattern.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate GameManagers.
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Uncomment if you want GameManager to persist across scenes.
        }

        UpdateScoreUI(); // Initialize score display.
    }

    /// <summary>
    /// Adds points to the current score and updates the UI.
    /// </summary>
    /// <param name="points">The amount of points to add.</param>
    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreUI();
        UnityEngine.Debug.Log($"Score: {currentScore}");
    }

    /// <summary>
    /// Resets the score to zero.
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
        UnityEngine.Debug.Log("Score Reset.");
    }

    /// <summary>
    /// Updates the TextMeshPro UI element with the current score.
    /// </summary>
    private void UpdateScoreUI()
    {
        if (scoreTextDisplay != null)
        {
            scoreTextDisplay.text = $"Score: {currentScore}";
        }
    }
}