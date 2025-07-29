using UnityEngine;
using UnityEngine.UI; // Required for Image component
using TMPro; // Required for TextMeshPro UI elements

/// <summary>
/// Manages the health of a GameObject.
/// Can be attached to both players and enemies. Notifies GameManager on death.
/// </summary>
public class Health : MonoBehaviour
{
    // SerializeField makes private variables visible and editable in the Unity Inspector.
    [SerializeField]
    private float maxHealth = 100f; // The maximum health value for this entity.

    private float currentHealth; // The current health value.

    [Header("UI Display (Player Only)")]
    [SerializeField]
    [Tooltip("Assign the TextMeshProUGUI element to display health text (e.g., 'Health: 100/100').")]
    private TextMeshProUGUI healthTextDisplay; // Reference to a TextMeshProUGUI component in the UI.

    [SerializeField]
    [Tooltip("Assign the UI Image that will act as the health bar fill (the red bar).")]
    private Image healthBarFillImage; // Reference to the UI Image for the health bar fill.

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
        UnityEngine.Debug.Log($"{gameObject.name} Health Initialized: {currentHealth}/{maxHealth}. IsAlive: {IsAlive}");
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
        UnityEngine.Debug.Log($"{gameObject.name} Health: {currentHealth}/{maxHealth}. IsAlive: {IsAlive}");

        // If health drops to zero or below, the entity is no longer alive.
        if (!IsAlive)
        {
            UnityEngine.Debug.Log($"{gameObject.name} has been defeated!");
            // Notify other scripts on this GameObject (like EnemyController) about death.
            SendMessage("OnDeath", SendMessageOptions.DontRequireReceiver);

            // If this is the player, notify the GameManager directly.
            if (gameObject.CompareTag("Player"))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.PlayerDied();
                }
            }
        }
    }

    /// <summary>
    /// Heals the entity, increasing its health.
    /// </summary>
    /// <param name="amount">The amount of health to restore.</param>
    public void Heal(float amount)
    {
        // Only heal if not already at max health or if dead (for revival scenarios).
        if (currentHealth >= maxHealth) return;

        currentHealth += amount; // Increase health.

        // Ensure health doesn't exceed max health.
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        UpdateHealthUI(); // Update the UI display after healing.
        UnityEngine.Debug.Log($"{gameObject.name} Health Restored: {currentHealth}/{maxHealth}. IsAlive: {IsAlive}");
    }

    /// <summary>
    /// Resets the entity's health to full.
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        UnityEngine.Debug.Log($"{gameObject.name} Health Reset to Full.");
    }

    /// <summary>
    /// Updates the TextMeshPro UI element and the health bar fill.
    /// </summary>
    private void UpdateHealthUI()
    {
        // Update health text display
        if (healthTextDisplay != null)
        {
            healthTextDisplay.text = $"Health: {currentHealth:F0}/{maxHealth:F0}"; // :F0 formats to 0 decimal places.
        }

        // Update health bar fill amount
        if (healthBarFillImage != null)
        {
            healthBarFillImage.fillAmount = currentHealth / maxHealth;
        }
    }
}