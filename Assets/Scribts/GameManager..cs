using UnityEngine;
using UnityEngine.UI; // For Image (hearts, background) and Button
using TMPro; // For TextMeshProUGUI
using System.Collections.Generic; // For List
using UnityEngine.SceneManagement; // For SceneManager (if going back to menu requires scene reload)
using System.Collections; // For Coroutines

/// <summary>
/// Manages overall game state, including score, lives, spawning, and win/lose conditions.
/// This is a singleton (only one instance in the scene).
/// </summary>
public class GameManager : MonoBehaviour
{
    // Public static instance to allow other scripts to easily access the GameManager.
    public static GameManager Instance { get; private set; }

    // Correctly declared as a private class member
    private int currentScore = 0; // The player's current score.

    [Header("Game Settings")]
    [SerializeField]
    private int maxLives = 3; // Total lives the player has.
    private int currentLives;

    [SerializeField]
    private int enemiesToWin = 10; // Number of enemies to kill to win the game.
    private int enemiesKilledCount = 0;

    [Header("Player References")]
    [SerializeField]
    private GameObject playerPrefab; // Assign your Player Prefab here.
    [SerializeField]
    private Transform playerSpawnPoint; // Assign an empty GameObject as the player's spawn point.
    private GameObject currentPlayerInstance; // Reference to the active player GameObject.
    private Health playerHealthComponent; // Reference to the player's Health script.

    [Header("UI Panels")]
    [SerializeField]
    private GameObject menuPanel; // Assign the UI Panel for the main menu.
    [SerializeField]
    private GameObject hudPanel; // Assign the UI Panel for the in-game HUD (health, score, lives).
    [SerializeField]
    private GameObject winPanel; // Assign the UI Panel for the Game Win screen.
    [SerializeField]
    private GameObject losePanel; // Assign the UI Panel for the Game Over screen.

    [Header("HUD UI Elements")]
    [SerializeField]
    private TextMeshProUGUI scoreTextDisplay; // Assign the TextMeshProUGUI element for score.
    [SerializeField]
    private List<Image> heartImages = new List<Image>(); // Assign heart UI Image elements here.

    [Header("Win/Lose UI Elements")]
    [SerializeField]
    private TextMeshProUGUI winScoreText; // TextMeshPro for displaying score on win screen.
    // No specific text for lose screen mentioned, but you can add one if needed.

    // Reference to the Spawner script.
    private Spawner gameSpawner;

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

        // Find the Spawner in the scene.
        // Replaced FindObjectOfType with FindFirstObjectByType
        gameSpawner = FindFirstObjectByType<Spawner>();
        if (gameSpawner == null)
        {
            UnityEngine.Debug.LogError("GameManager: Spawner script not found in the scene! Please add one.");
        }

        // Ensure all panels are initially off, except the menu.
        menuPanel?.SetActive(true);
        hudPanel?.SetActive(false);
        winPanel?.SetActive(false);
        losePanel?.SetActive(false);

        // Pause game at start (menu state).
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Called when the "Start Game" button is pressed from the menu.
    /// </summary>
    public void StartGame()
    {
        UnityEngine.Debug.Log("Starting New Game...");

        // Reset game state.
        currentScore = 0; // Accessing currentScore
        enemiesKilledCount = 0;
        currentLives = maxLives;
        Time.timeScale = 1f; // Resume game time.

        // Update UI.
        UpdateScoreUI();
        UpdateLivesUI();
        menuPanel?.SetActive(false);
        hudPanel?.SetActive(true);
        winPanel?.SetActive(false);
        losePanel?.SetActive(false);

        // Spawn or reset player.
        SetupPlayer();

        // Start spawning enemies.
        gameSpawner?.StartSpawning();
    }

    /// <summary>
    /// Sets up or respawns the player character.
    /// </summary>
    private void SetupPlayer()
    {
        if (playerPrefab == null || playerSpawnPoint == null)
        {
            UnityEngine.Debug.LogError("GameManager: Player Prefab or Player Spawn Point not assigned!");
            return;
        }

        // Destroy existing player instance if any.
        if (currentPlayerInstance != null)
        {
            Destroy(currentPlayerInstance);
        }

        // Instantiate new player.
        currentPlayerInstance = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
        playerHealthComponent = currentPlayerInstance.GetComponent<Health>();
        if (playerHealthComponent == null)
        {
            UnityEngine.Debug.LogError("GameManager: Player Prefab does not have a Health component!");
        }
    }


    /// <summary>
    /// Called by EnemyController when an enemy is killed.
    /// </summary>
    public void EnemyKilled()
    {
        enemiesKilledCount++;
        UnityEngine.Debug.Log($"Enemies Killed: {enemiesKilledCount}");
        CheckWinCondition();
    }

    /// <summary>
    /// Checks if the win condition has been met.
    /// </summary>
    private void CheckWinCondition()
    {
        if (enemiesKilledCount >= enemiesToWin)
        {
            GameWin();
        }
    }

    /// <summary>
    /// Called by Player's Health script when player health reaches 0.
    /// </summary>
    public void PlayerDied()
    {
        currentLives--;
        UnityEngine.Debug.Log($"Player Died! Lives remaining: {currentLives}");
        UpdateLivesUI();

        if (currentLives > 0)
        {
            // Respawn player after a short delay.
            StartCoroutine(RespawnPlayerAfterDelay(1.5f)); // 1.5 seconds delay before respawn.
        }
        else
        {
            // Game Over.
            GameOver();
        }
    }

    /// <summary>
    /// Coroutine to handle player respawn after a delay.
    /// </summary>
    /// <param name="delay">Delay before respawning.</param>
    /// <returns>IEnumerator for coroutine.</returns>
    private IEnumerator RespawnPlayerAfterDelay(float delay)
    {
        // Temporarily disable player input and visuals
        if (currentPlayerInstance != null)
        {
            currentPlayerInstance.SetActive(false); // Hide player
            // You might want to play a death animation/sound here
        }

        gameSpawner?.StopSpawning(); // Stop spawning during respawn.

        yield return new WaitForSeconds(delay);

        // Reset player health and position.
        if (currentPlayerInstance != null)
        {
            playerHealthComponent?.ResetHealth(); // Reset health to full.
            currentPlayerInstance.transform.position = playerSpawnPoint.position;
            currentPlayerInstance.transform.rotation = playerSpawnPoint.rotation;
            currentPlayerInstance.SetActive(true); // Show player
        }

        gameSpawner?.StartSpawning(); // Resume spawning after respawn.
    }


    /// <summary>
    /// Triggers the Game Over state.
    /// </summary>
    public void GameOver()
    {
        UnityEngine.Debug.Log("GAME OVER!");
        Time.timeScale = 0f; // Pause game.

        hudPanel?.SetActive(false);
        winPanel?.SetActive(false);
        losePanel?.SetActive(true);
        gameSpawner?.StopSpawning(); // Ensure spawning stops.
        currentPlayerInstance?.SetActive(false); // Hide player on game over.
    }

    /// <summary>
    /// Triggers the Game Win state.
    /// </summary>
    public void GameWin()
    {
        UnityEngine.Debug.Log("GAME WIN!");
        Time.timeScale = 0f; // Pause game.

        hudPanel?.SetActive(false);
        losePanel?.SetActive(false);
        winPanel?.SetActive(true);
        gameSpawner?.StopSpawning(); // Ensure spawning stops.
        currentPlayerInstance?.SetActive(false); // Hide player on game win.

        // Display final score on win screen.
        if (winScoreText != null)
        {
            winScoreText.text = $"Final Score: {currentScore}"; // Accessing currentScore
        }
    }

    /// <summary>
    /// Called by "Back to Menu" buttons.
    /// </summary>
    public void BackToMenu()
    {
        UnityEngine.Debug.Log("Returning to Menu...");
        Time.timeScale = 0f; // Ensure game is paused.

        // Deactivate all game-related elements.
        hudPanel?.SetActive(false);
        winPanel?.SetActive(false);
        losePanel?.SetActive(false);
        gameSpawner?.StopSpawning();

        // Destroy all active enemies (optional, but good for clean reset).
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy"); // Assuming enemies are tagged "Enemy"
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
        // Destroy player instance if it exists
        if (currentPlayerInstance != null)
        {
            Destroy(currentPlayerInstance);
            currentPlayerInstance = null;
        }


        menuPanel?.SetActive(true); // Show main menu.
        // If you have multiple scenes, you might reload the main menu scene here:
        // SceneManager.LoadScene("MainMenuSceneName");
    }

    /// <summary>
    /// Adds points to the current score and updates the UI.
    /// </summary>
    /// <param name="points">The amount of points to add.</param>
    public void AddScore(int points)
    {
        currentScore += points; // Accessing currentScore
        UpdateScoreUI();
        UnityEngine.Debug.Log($"Score: {currentScore}"); // Accessing currentScore
    }

    /// <summary>
    /// Resets the score to zero.
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0; // Accessing currentScore
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
            scoreTextDisplay.text = $"Score: {currentScore}"; // Accessing currentScore
        }
    }

    /// <summary>
    /// Updates the visual display of player lives (heart images).
    /// </summary>
    private void UpdateLivesUI()
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (i < currentLives)
            {
                heartImages[i].enabled = true; // Show heart if player has this life.
            }
            else
            {
                heartImages[i].enabled = false; // Hide heart if player lost this life.
            }
        }
    }
}
