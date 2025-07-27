using UnityEngine;

/// <summary>
/// Controls the behavior of an enemy in a 2D top-down game,
/// including movement towards the player and dealing damage on contact.
/// </summary>
public class EnemyController : MonoBehaviour
{
    // --- Configurable Variables (visible in Inspector) ---
    [SerializeField]
    private float enemyMoveSpeed = 3f; // Speed at which the enemy moves towards the player.

    [SerializeField]
    private float attackDamage = 10f; // Amount of damage the enemy deals per hit.

    [SerializeField]
    private float damageRate = 0.5f; // How often the enemy can deal damage (in seconds).

    // --- Private Internal State Variables ---
    private bool isPlayerInContact = false; // True if the player is currently touching the enemy.
    private float nextDamageTime = 0f;      // Timer for when the enemy can next deal damage.
    private Health playerHealth;            // Reference to the player's Health script.

    /// <summary>
    /// Update is called once per frame.
    /// Handles enemy movement and continuous damage dealing while in contact with the player.
    /// </summary>
    void Update()
    {
        // Decrement the damage cooldown timer.
        nextDamageTime -= Time.deltaTime;

        // --- Damage Logic ---
        // If the player is in contact, the damage cooldown is over,
        // the player's health script is found, and the player is alive:
        if (isPlayerInContact && nextDamageTime <= 0 && playerHealth != null && playerHealth.IsAlive)
        {
            playerHealth.DoDamage(attackDamage); // Deal damage to the player.
            nextDamageTime = damageRate;         // Reset the damage cooldown.
        }

        // --- Movement Logic ---
        // Only move towards the player if the player's health script is found and the player is alive.
        if (playerHealth != null && playerHealth.IsAlive)
        {
            // Calculate the direction from the enemy to the player.
            Vector2 directionToPlayer = (playerHealth.transform.position - transform.position).normalized;

            // Move the enemy towards the player.
            // Vector2.MoveTowards is good for moving an object from its current position towards a target position.
            transform.position = Vector2.MoveTowards(transform.position, playerHealth.transform.position, enemyMoveSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Called when another collider enters a trigger collider attached to this GameObject.
    /// Used to detect when the player enters the enemy's collision area.
    /// </summary>
    /// <param name="other">The other Collider2D involved in this collision.</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the entering collider belongs to the GameObject tagged "Player".
        // Using CompareTag is more efficient than accessing .tag directly.
        if (other.CompareTag("Player"))
        {
            isPlayerInContact = true; // Player is now in contact.
            // Try to get the Health component from the player GameObject.
            playerHealth = other.GetComponent<Health>();
            if (playerHealth == null)
            {
                UnityEngine.Debug.LogError("EnemyController: Player GameObject is tagged 'Player' but does not have a Health component!");
            }
            UnityEngine.Debug.Log("Player entered enemy contact zone.");
        }
    }

    /// <summary>
    /// Called when another collider exits a trigger collider attached to this GameObject.
    /// Used to detect when the player leaves the enemy's collision area.
    /// </summary>
    /// <param name="other">The other Collider2D involved in this collision.</param>
    void OnTriggerExit2D(Collider2D other)
    {
        // Check if the exiting collider belongs to the GameObject tagged "Player".
        if (other.CompareTag("Player"))
        {
            isPlayerInContact = false; // Player is no longer in contact.
            playerHealth = null;       // Clear the reference to player health.
            UnityEngine.Debug.Log("Player exited enemy contact zone.");
        }
    }
}