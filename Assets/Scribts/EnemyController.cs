using System.Collections; // Required for Coroutines
using UnityEngine;

/// <summary>
/// Controls the behavior of a zombie enemy in a 2D top-down game.
/// Handles movement towards the player, damage to the player on contact,
/// and plays different moaning sounds based on player proximity.
/// </summary>
public class EnemyController : MonoBehaviour
{
    // --- Configurable Variables (visible in Inspector) ---
    [Header("Movement Settings")]
    [SerializeField]
    private float moveSpeed = 2f; // Speed at which the zombie moves towards the player.

    [Header("Combat Settings")]
    [SerializeField]
    private float playerDamageAmount = 20f; // Amount of health player loses per touch.
                                            // This is 20% of 100 max health.

    [SerializeField]
    private float damageRate = 1.0f; // How often the zombie can deal damage to the player (in seconds).
    private float nextDamageTime;    // Timer for next damage application.

    [Header("Audio Settings")]
    [SerializeField]
    private AudioClip idleMoanSound; // Sound played when player is not in proximity.
    [SerializeField]
    private AudioClip excitedAttackMoanSound; // Sound played when player is in proximity/attacking.
    [SerializeField]
    private float moanInterval = 5f; // How often the idle moan plays.
    [SerializeField]
    private float proximityRange = 3f; // Distance at which zombie plays excited moan.

    private AudioSource audioSource; // Reference to the AudioSource component.
    private float nextMoanTime;      // Timer for next idle moan.

    [Header("Sprite Settings")]
    [SerializeField]
    private Sprite walkingSprite; // Assign the zombie walking sprite here.
    [SerializeField]
    private Sprite deathSprite;   // Assign the zombie death sprite here.
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component.

    // --- Private Internal References ---
    private Transform playerTransform; // Reference to the player's Transform.
    private Health playerHealth;       // Reference to the player's Health script.
    private Health enemyHealth;        // Reference to this zombie's own Health script.

    private bool isPlayerInProximity = false; // True if player is within proximity range.
    private bool isPlayerInContact = false;   // True if player is currently touching the zombie.


    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes references and sets up AudioSource.
    /// </summary>
    void Awake()
    {
        // Find the player GameObject by its tag.
        GameObject playerGameObject = GameObject.FindWithTag("Player");
        if (playerGameObject != null)
        {
            playerTransform = playerGameObject.transform;
            playerHealth = playerGameObject.GetComponent<Health>();
        }
        else
        {
            UnityEngine.Debug.LogError("EnemyController: Player GameObject not found! Ensure player is tagged 'Player'.");
        }

        // Get this zombie's own Health component.
        enemyHealth = GetComponent<Health>();
        if (enemyHealth == null)
        {
            UnityEngine.Debug.LogError("EnemyController: No Health component found on the Enemy GameObject!");
        }

        // Get or add an AudioSource component.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0; // 2D sound.
        }

        // Get SpriteRenderer and set initial sprite.
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        if (walkingSprite != null)
        {
            spriteRenderer.sprite = walkingSprite;
        }

        nextDamageTime = 0f;
        nextMoanTime = Time.time + Random.Range(moanInterval / 2, moanInterval * 1.5f); // Randomize initial moan.
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
            // --- Movement Logic: Move towards player if player is alive ---
            if (playerTransform != null && playerHealth != null && playerHealth.IsAlive)
            {
                // Calculate direction to player.
                Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;

                // Move towards the player.
                transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);

                // Rotate to face the player (optional, depending on sprite orientation).
                // If your zombie sprite faces right by default, and you want it to face the player:
                // float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
                // transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            // --- Damage Logic: Deal damage to player if in contact ---
            nextDamageTime -= Time.deltaTime; // Decrement damage cooldown.
            if (isPlayerInContact && nextDamageTime <= 0 && playerHealth != null && playerHealth.IsAlive)
            {
                playerHealth.DoDamage(playerDamageAmount);
                nextDamageTime = damageRate; // Reset cooldown.
            }

            // --- Audio Logic: Play moans based on proximity ---
            UpdateMoaningSounds();
        }
        else // Zombie is dead
        {
            // If zombie just died, set death sprite and destroy after a delay.
            if (spriteRenderer.sprite != deathSprite) // Check to set only once
            {
                if (deathSprite != null)
                {
                    spriteRenderer.sprite = deathSprite;
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
        }
    }

    /// <summary>
    /// Manages moaning sounds based on player proximity.
    /// </summary>
    private void UpdateMoaningSounds()
    {
        if (audioSource == null || playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

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
            UnityEngine.Debug.Log("Player entered zombie contact zone.");
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
            UnityEngine.Debug.Log("Player exited zombie contact zone.");
        }
    }
}