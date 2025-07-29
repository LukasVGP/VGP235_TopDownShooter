using UnityEngine;
using System.Collections; // Required for Coroutines
using System.Collections.Generic; // Required for List

/// <summary>
/// Defines a single wave of enemies.
/// </summary>
[System.Serializable]
public class WaveConfig
{
    public string waveName = "Wave 1";
    public GameObject enemyPrefab; // The specific enemy prefab to spawn in this wave.
    public int numberOfEnemies = 5; // How many enemies to spawn in this wave.
    public float spawnInterval = 1f; // Time between individual enemy spawns within this wave.
}

/// <summary>
/// Manages spawning waves of enemies.
/// </summary>
public class Spawner : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField]
    [Tooltip("Assign empty GameObjects as spawn points for enemies.")]
    private List<Transform> spawnPoints = new List<Transform>(); // List of possible spawn locations.

    [Header("Wave Configuration")]
    [SerializeField]
    private List<WaveConfig> waves = new List<WaveConfig>(); // List of waves to spawn.
    [SerializeField]
    private float timeBetweenWaves = 5f; // Time delay between finishing one wave and starting the next.

    private int currentWaveIndex = 0;
    private int enemiesSpawnedInCurrentWave = 0;
    private int enemiesKilledInCurrentWave = 0; // Track enemies killed to know when wave is clear.

    private bool isSpawning = false;
    private Coroutine spawnCoroutine; // Reference to the ongoing spawn coroutine.

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Ensures spawn points are assigned.
    /// </summary>
    void Awake()
    {
        if (spawnPoints.Count == 0)
        {
            UnityEngine.Debug.LogWarning("Spawner: No spawn points assigned! Please assign Transform GameObjects to the Spawn Points list.");
        }
    }

    /// <summary>
    /// Starts the enemy spawning process from the beginning.
    /// </summary>
    public void StartSpawning()
    {
        if (isSpawning)
        {
            StopSpawning(); // Stop any existing spawning first.
        }

        isSpawning = true;
        currentWaveIndex = 0;
        enemiesSpawnedInCurrentWave = 0;
        enemiesKilledInCurrentWave = 0;
        spawnCoroutine = StartCoroutine(SpawnWavesRoutine());
        UnityEngine.Debug.Log("Spawner: Starting spawning waves.");
    }

    /// <summary>
    /// Stops the current spawning process.
    /// </summary>
    public void StopSpawning()
    {
        if (isSpawning && spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        isSpawning = false;
        UnityEngine.Debug.Log("Spawner: Spawning stopped.");
    }

    /// <summary>
    /// The main coroutine that manages spawning waves.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution.</returns>
    private IEnumerator SpawnWavesRoutine()
    {
        while (currentWaveIndex < waves.Count)
        {
            WaveConfig currentWaveConfig = waves[currentWaveIndex];
            UnityEngine.Debug.Log($"Spawner: Starting Wave {currentWaveIndex + 1} - {currentWaveConfig.waveName}");

            enemiesSpawnedInCurrentWave = 0;
            enemiesKilledInCurrentWave = 0; // Reset killed count for new wave.

            // Spawn enemies for the current wave.
            for (int i = 0; i < currentWaveConfig.numberOfEnemies; i++)
            {
                if (!isSpawning) yield break; // Stop if spawning is disabled externally.

                SpawnEnemy(currentWaveConfig.enemyPrefab);
                enemiesSpawnedInCurrentWave++;
                yield return new WaitForSeconds(currentWaveConfig.spawnInterval);
            }

            UnityEngine.Debug.Log($"Spawner: Finished spawning Wave {currentWaveIndex + 1}. Waiting for enemies to be killed.");

            // Wait until all enemies from the current wave are killed.
            // This relies on EnemyController notifying GameManager, and GameManager then notifying Spawner.
            // For simplicity, we'll wait until all spawned enemies are killed.
            // A more robust system might track active enemies in the scene.
            while (enemiesKilledInCurrentWave < currentWaveConfig.numberOfEnemies)
            {
                yield return null; // Wait a frame.
            }

            UnityEngine.Debug.Log($"Spawner: All enemies in Wave {currentWaveIndex + 1} killed.");

            currentWaveIndex++; // Move to the next wave.

            if (currentWaveIndex < waves.Count)
            {
                UnityEngine.Debug.Log($"Spawner: Waiting {timeBetweenWaves} seconds before next wave.");
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }

        UnityEngine.Debug.Log("Spawner: All waves completed.");
        // All waves are done, GameManager will handle win condition based on total enemies killed.
    }

    /// <summary>
    /// Spawns a single enemy at a random spawn point.
    /// </summary>
    /// <param name="enemyPrefab">The prefab of the enemy to spawn.</param>
    private void SpawnEnemy(GameObject enemyPrefab)
    {
        if (spawnPoints.Count == 0)
        {
            UnityEngine.Debug.LogError("Spawner: Cannot spawn enemy, no spawn points assigned!");
            return;
        }

        Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        GameObject spawnedEnemy = Instantiate(enemyPrefab, randomSpawnPoint.position, randomSpawnPoint.rotation);
        spawnedEnemy.tag = "Enemy"; // Ensure spawned enemies have the "Enemy" tag for FindGameObjectsWithTag.

        // Optionally, if you want the Spawner to directly track killed enemies,
        // you could add a method here that enemies call on death.
        // For now, EnemyController notifies GameManager, and GameManager notifies Spawner.
    }

    /// <summary>
    /// Called by GameManager when an enemy is killed.
    /// </summary>
    public void NotifyEnemyKilled()
    {
        enemiesKilledInCurrentWave++;
        UnityEngine.Debug.Log($"Spawner: Enemy killed in current wave. Total killed in wave: {enemiesKilledInCurrentWave}");
    }
}
