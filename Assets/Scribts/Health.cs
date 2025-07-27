using UnityEngine;
using TMPro; // Required for TextMeshPro UI elements

/// <summary>
/// Manages the health of a GameObject.
/// Can be attached to both players and enemies.
/// </summary>
public class Health : MonoBehaviour
{
    // SerializeField makes private variables visible and editable in the Unity Inspector.
    // This allows designers to easily tweak health values without changing code.
    [SerializeField]
    private float maxHealth = 100f; // The maximum health value for this entity.

    // Reference to a TextMeshProUGUI component in the UI to display current health.
    [SerializeField]
    private TextMeshProUGUI healthTextDisplay;

    private float currentHealth; // The current health value.

    /// <summary>
    /// Property to check if the entity is currently alive (health > 0).
    /// Read-only property.
    /// </summary>
    public bool IsAlive => currentHealth > 0;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes the current health to the maximum health.
    /// </summary>
    void Awake()
    {
        currentHealth = maxHealth; // Start with full health.
        UpdateHealthUI(); // Update the UI display immediately.
        UnityEngine.Debug.Log($"{gameObject.name} Health Initialized: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// Applies damage to the entity's health.
    /// </summary>
    /// <param name="amount">The amount of damage to apply.</param>
    public void DoDamage(float amount)
    {
        if (!IsAlive) return; // If already dead, do nothing.

        currentHealth -= amount; // Reduce health by the damage amount.

        // Ensure health doesn't go below zero.
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        UpdateHealthUI(); // Update the UI display after taking damage.
        UnityEngine.Debug.Log($"{gameObject.name} Health: {currentHealth}/{maxHealth}");

        // If health drops to zero or below, the entity is no longer alive.
        if (!IsAlive)
        {
            UnityEngine.Debug.Log($"{gameObject.name} has been defeated!");
            // Here you would typically add logic for death, e.g., playing an animation, destroying the GameObject.
            // Example: Destroy(gameObject); // Uncomment to destroy the GameObject on death.
        }
    }

    /// <summary>
    /// Heals the entity, increasing its health.
    /// </summary>
    /// <param name="amount">The amount of health to restore.</param>
    public void Heal(float amount)
    {
        if (!IsAlive) return; // Cannot heal if already defeated (unless specific game logic allows revival).

        currentHealth += amount; // Increase health.

        // Ensure health doesn't exceed max health.
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        UpdateHealthUI(); // Update the UI display after healing.
        UnityEngine.Debug.Log($"{gameObject.name} Health Restored: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// Updates the TextMeshPro UI element with the current health value.
    /// </summary>
    private void UpdateHealthUI()
    {
        if (healthTextDisplay != null)
        {
            // Format the health display to show current/max health.
            healthTextDisplay.text = $"Health: {currentHealth:F0}/{maxHealth:F0}"; // :F0 formats to 0 decimal places.
        }
    }
}