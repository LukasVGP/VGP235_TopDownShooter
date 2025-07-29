using UnityEngine;
using System.Collections; // Required for Coroutines

/// <summary>
/// Controls the behavior of a zombie enemy in a 2D top-down game.
/// Handles movement towards the player (only when in proximity), damage to the player on contact,
/// plays different moaning sounds based on player proximity, awards points on death,
/// and provides visual/audio feedback when hit by bullets.
/// </summary>
public class EnemyController : MonoBehaviour
{
    // --- Configurable Variables (visible in Inspector) ---
    [Header("Movement Settings")]
    [SerializeField]
    private float moveSpeed = 2f; // Speed at which the zombie moves towards the player.
    [SerializeField]
    [Tooltip("Adjust this value to align the zombie's sprite 'front' with its movement direction.")]
    private float rotationOffset = -90f; // Default adjustment for sprites initially facing 'up' (Y-axis).
                                         // You will likely need to tweak this value in the Inspector.
    private Transform playerTransform; // Reference to the player's Transform.
    private bool isKnockedBack = false; // Flag to indicate if the zombie is currently being knocked back.

    [Header("Combat Settings")]
    [SerializeField]
    private float playerDamageAmount = 25f; // Amount of health player loses per touch (25% of 100 max health).
    [SerializeField]
    private float damageRate = 1.0f; // How often the zombie can deal damage to the player (in seconds).
    private float nextDamageTime;    // Timer for next damage application.

    private Health playerHealth;       // Reference to the player's Health script.
    private Health enemyHealth;        // Reference to this zombie's own Health script.
    private bool isPlayerInContact = false;   // True if player is currently touching the zombie.

    [Header("Hit Feedback")]
    [SerializeField]
    private AudioClip hitSound; // Sound played when the zombie is hit by a bullet.
    [SerializeField]
    private float knockbackForce = 0.5f; // How far the zombie is pushed back when hit.
    [SerializeField]
    private float knockbackDuration = 0.1f; // How long the knockback effect lasts.

    [Header("Audio Settings")]
    [SerializeField]
    private AudioClip idleMoanSound; // Sound played when player is not in proximity.
    [SerializeField]
    private AudioClip excitedAttackMoanSound; // Sound played when player is in proximity/attacking.
    [SerializeField]
    private float moanInterval = 5f; // How often the idle moan plays.
    [SerializeField]
    private float proximityRange = 3f; // Distance at which zombie starts moving towards player and plays excited moan.
    private AudioSource audioSource; // Reference to the AudioSource component.
    private float nextMoanTime;      // Timer for next idle moan.

    [Header("Sprite Settings")]
    [SerializeField]
    private Sprite walkingSprite; // Assign the zombie walking sprite here.
    [SerializeField]
    private Sprite deathSprite;   // Assign the zombie death sprite here.
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component.

    [Header("Score Settings")]
    [SerializeField]
    private int scoreValue = 10; // Points awarded when this zombie dies.
    private GameManager gameManager;   // Reference to the GameManager script.


    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes references and sets up AudioSource.
    /// </summary>
    void Awake()
    {
        // --- Get References ---
        // Get this zombie's own Health component.
        enemyHealth = GetComponent<Health>();
        if (enemyHealth == null)
        {
            UnityEngine.Debug.LogError($"{gameObject.name}: No Health component found on the Enemy GameObject! Please add one.");
        }
        else
        {
            UnityEngine.Debug.Log($"{gameObject.name} Health component found. Initial IsAlive: {enemyHealth.IsAlive}");
        }

        // Get SpriteRenderer and set initial sprite.
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            UnityEngine.Debug.LogWarning($"{gameObject.name}: Added missing SpriteRenderer component.");
        }
        if (walkingSprite != null)
        {
            spriteRenderer.sprite = walkingSprite;
        }
        else
        {
            UnityEngine.Debug.LogWarning($"{gameObject.name}: Walking Sprite not assigned. Zombie might be invisible.");
        }

        // Get or add an AudioSource component.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0; // 2D sound.
        }

        // --- Initialize Timers ---
        nextDamageTime = 0f;
        nextMoanTime = Time.time + Random.Range(moanInterval / 2, moanInterval * 1.5f); // Randomize initial moan.
    }

    /// <summary>
    /// Called immediately after Awake, when the script is enabled.
    /// Used here to find other GameObjects, as they might not be fully initialized in Awake.
    /// </summary>
    void Start()
    {
        // Find the player GameObject by its tag.
        GameObject playerGameObject = GameObject.FindWithTag("Player");
        if (playerGameObject != null)
        {
            playerTransform = playerGameObject.transform;
            playerHealth = playerGameObject.GetComponent<Health>();
            if (playerHealth == null)
            {
                UnityEngine.Debug.LogError("EnemyController: Player GameObject is tagged 'Player' but has no Health component!");
            }
        }
        else
        {
            UnityEngine.Debug.LogError("EnemyController: Player GameObject not found! Ensure player is tagged 'Player'.");
        }

        // Find the GameManager in the scene.
        if (GameManager.Instance != null) // Use GameManager singleton
        {
            gameManager = GameManager.Instance;
        }
        else
        {
            UnityEngine.Debug.LogWarning("EnemyController: GameManager Instance not found! Ensure a GameObject with GameManager script is in the scene.");
        }
    }


    /// <summary>
    /// Update is called once per frame.
    /// Handles zombie movement, damage to player, and sound playback.
    /// </summary>
    void Update()
    {
        // Only perform actions if the zombie is alive.
        if (enemyHealth != null && enemyHealth.IsAlive)
        {
            // Calculate distance to player once per frame.
            float distanceToPlayer = Mathf.Infinity;
            if (playerTransform != null)
            {
                distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            }

            bool isPlayerInProximity = (distanceToPlayer <= proximityRange);


            // --- Movement Logic: Move towards player ONLY if player is alive AND in proximity AND not knocked back ---
            if (playerTransform != null && playerHealth != null && playerHealth.IsAlive && isPlayerInProximity && !isKnockedBack)
            {
                // Calculate direction to player.
                Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;

                // Move towards the player.
                transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);

                // Rotate to face the player (using the rotation offset).
                float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
            }

            // --- Damage Logic: Deal damage to player if in contact ---
            nextDamageTime -= Time.deltaTime; // Decrement damage cooldown.
            if (isPlayerInContact && nextDamageTime <= 0 && playerHealth != null && playerHealth.IsAlive)
            {
                playerHealth.DoDamage(playerDamageAmount);
                nextDamageTime = damageRate; // Reset cooldown.
            }

            // --- Audio Logic: Play moans based on proximity ---
            UpdateMoaningSounds(distanceToPlayer);
        }
        else // Zombie is dead
        {
            // Call OnDeath handler (ensures death logic runs only once).
            OnDeath();
        }
    }

    /// <summary>
    /// Public method for other scripts (like Bullet.cs) to call when this enemy is hit.
    /// </summary>
    /// <param name="damageAmount">The amount of damage to take.</param>
    /// <param name="hitPosition">The world position where the hit occurred (e.g., bullet's position).</param>
    public void TakeHit(float damageAmount, Vector2 hitPosition)
    {
        if (enemyHealth == null || !enemyHealth.IsAlive) return; // Can't take hit if no health component or already dead.

        enemyHealth.DoDamage(damageAmount); // Apply damage to the zombie's health.

        // Play hit sound.
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // Apply knockback.
        Vector2 knockbackDirection = (Vector2)transform.position - hitPosition;
        knockbackDirection.Normalize(); // Ensure it's a unit vector.
        StartCoroutine(ApplyKnockback(knockbackDirection));
    }


    /// <summary>
    /// Coroutine to apply a temporary knockback effect to the zombie.
    /// </summary>
    /// <param name="direction">The normalized direction to apply knockback.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    private IEnumerator ApplyKnockback(Vector2 direction)
    {
        isKnockedBack = true; // Set flag to temporarily stop normal movement.
        float timer = 0f;
        Vector2 initialPosition = transform.position;
        Vector2 targetPosition = initialPosition + direction * knockbackForce;

        while (timer < knockbackDuration)
        {
            // Move towards the target knockback position.
            transform.position = Vector2.Lerp(initialPosition, targetPosition, timer / knockbackDuration);
            timer += Time.deltaTime;
            yield return null; // Wait for the next frame.
        }

        // Ensure zombie is at the end of its knockback path.
        transform.position = targetPosition;
        isKnockedBack = false; // Allow normal movement to resume.
    }


    /// <summary>
    /// Handles the zombie's death logic. Called when enemyHealth.IsAlive becomes false.
    /// </summary>
    private bool hasDied = false; // Flag to ensure death logic runs only once.
    public void OnDeath() // Made public so Health.cs can call it via SendMessage
    {
        if (hasDied) return; // Only run death logic once.
        hasDied = true;

        UnityEngine.Debug.Log($"{gameObject.name} has been defeated!");

        // Award score.
        if (gameManager != null)
        {
            gameManager.AddScore(scoreValue);
            gameManager.EnemyKilled(); // Notify GameManager that an enemy was killed.
        }
        else
        {
            UnityEngine.Debug.LogWarning("GameManager not found, score not awarded.");
        }

        // Set death sprite.
        if (spriteRenderer.sprite != deathSprite) // Only change sprite if it's not already the death sprite
        {
            if (deathSprite != null)
            {
                spriteRenderer.sprite = deathSprite;
            }
            else
            {
                UnityEngine.Debug.LogWarning($"{gameObject.name}: Death Sprite not assigned.");
            }
        }


        // Stop movement
        moveSpeed = 0;
        // Disable colliders to prevent further interaction
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        // Stop sounds
        if (audioSource != null) audioSource.Stop();

        // Destroy the zombie GameObject after a short delay to show death sprite.
        Destroy(gameObject, 2f); // Destroy after 2 seconds.
    }


    /// <summary>
    /// Manages moaning sounds based on player proximity.
    /// </summary>
    /// <param name="distanceToPlayer">The current distance from the zombie to the player.</param>
    private void UpdateMoaningSounds(float distanceToPlayer)
    {
        if (audioSource == null || playerTransform == null) return;

        if (distanceToPlayer <= proximityRange)
        {
            // Player is in proximity, play excited moan if not already playing or recently played.
            if (excitedAttackMoanSound != null && !audioSource.isPlaying)
            {
                audioSource.PlayOneShot(excitedAttackMoanSound);
                nextMoanTime = Time.time + excitedAttackMoanSound.length + Random.Range(0.5f, 1.5f); // Cooldown for excited moan.
            }
        }
        else // Player is far away, play idle moan on interval.
        {
            if (Time.time >= nextMoanTime)
            {
                if (idleMoanSound != null)
                {
                    audioSource.PlayOneShot(idleMoanSound);
                }
                nextMoanTime = Time.time + moanInterval + Random.Range(-1f, 1f); // Next idle moan, with slight variation.
            }
        }
    }


    /// <summary>
    /// Called when another collider enters a trigger collider attached to this GameObject.
    /// Used to detect when the player enters the zombie's contact area.
    /// </summary>
    /// <param name="other">The other Collider2D involved in this collision.</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInContact = true;
            // Ensure playerHealth reference is still valid.
            if (playerHealth == null) playerHealth = other.GetComponent<Health>();
            UnityEngine.Debug.Log($"{gameObject.name}: Player entered contact zone.");
        }
    }

    /// <summary>
    /// Called when another collider exits a trigger collider attached to this GameObject.
    /// Used to detect when the player leaves the zombie's contact area.
    /// </summary>
    /// <param name="other">The other Collider2D involved in this collision.</param>
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInContact = false;
            UnityEngine.Debug.Log($"{gameObject.name}: Player exited contact zone.");
        }
    }
}
