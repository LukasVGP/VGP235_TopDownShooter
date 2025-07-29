using UnityEngine;

/// <summary>
/// Controls the behavior of a projectile (bullet) in the game.
/// Handles movement, damage to enemies, and self-destruction after a certain distance.
/// </summary>
public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float speed = 20f; // How fast the bullet travels.

    // Damage and MaxDistance are now set by the WeaponController when the bullet is spawned.
    public float damage = 0f; // Initialized to 0, will be set by WeaponController.
    public float maxDistance = 0f; // Initialized to 0, will be set by WeaponController.

    [Header("Visuals")]
    [SerializeField]
    private Sprite bulletSprite; // Assign the bullet's visual sprite here in the Inspector.
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component.

    [Header("Audio Settings")]
    [SerializeField]
    private AudioClip bulletSound; // Assign your bullet sound effect here.
    private AudioSource audioSource; // Reference to the AudioSource component.

    private Vector2 startPosition; // The position where the bullet was spawned.

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes the bullet's starting position, sets up the AudioSource,
    /// and assigns the sprite to the SpriteRenderer.
    /// </summary>
    void Awake()
    {
        startPosition = transform.position;

        // Get or add a SpriteRenderer component to this GameObject.
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // Assign the bullet sprite if it's set in the Inspector.
        if (bulletSprite != null)
        {
            spriteRenderer.sprite = bulletSprite;
        }
        else
        {
            UnityEngine.Debug.LogWarning($"Bullet on {gameObject.name}: No bullet sprite assigned. Using default or no sprite.");
        }


        // Get or add an AudioSource component to this GameObject.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false; // Don't play sound automatically on awake.
            audioSource.spatialBlend = 0; // Set to 2D sound.
        }

        // Play the bullet sound immediately when the bullet is created.
        if (bulletSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(bulletSound);
        }
    }

    /// <summary>
    /// Update is called once per frame.
    /// Handles bullet movement and checks if it has exceeded its maximum travel distance.
    /// </summary>
    void Update()
    {
        // Move the bullet forward based on its current rotation.
        transform.position += transform.right * speed * Time.deltaTime;

        // Check if the bullet has traveled beyond its max distance.
        if (Vector2.Distance(startPosition, transform.position) >= maxDistance)
        {
            Destroy(gameObject); // Destroy the bullet if it travels too far.
        }
    }

    /// <summary>
    /// Called when the Collider2D other enters the trigger (2D physics only).
    /// Used to detect collision with enemies and apply damage.
    /// </summary>
    /// <param name="other">The other Collider2D involved in this collision.</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object has a Health component (e.g., an enemy).
        Health targetHealth = other.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.DoDamage(damage); // Apply damage to the target.
            Destroy(gameObject); // Destroy the bullet after hitting something with health.
        }
        // Optionally, destroy the bullet if it hits something else like a wall,
        // but for now, it only destroys on health target hit or max distance.
    }
}
