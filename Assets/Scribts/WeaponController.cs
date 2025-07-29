using UnityEngine;
using System.Collections; // Required for Coroutines
using System.Collections.Generic; // Required for List

/// <summary>
/// Defines the configuration for a single weapon type.
/// [System.Serializable] makes this class visible in the Unity Inspector.
/// </summary>
[System.Serializable]
public class WeaponConfig
{
    public string weaponName = "New Weapon"; // Name for display/identification.
    public WeaponController.WeaponType weaponType = WeaponController.WeaponType.Single;

    [Header("Visuals & Audio")]
    public GameObject bulletPrefab; // The bullet prefab for this weapon.
    public GameObject muzzleFlashPrefab; // The muzzle flash prefab for this weapon.
    public AudioClip shootingSound; // The shooting sound for this weapon.
    public Sprite playerWeaponSprite; // The player's sprite when this weapon is equipped.
    [Tooltip("Assign the specific MuzzleEndPoint child GameObject for this weapon.")]
    public Transform muzzleEndPointOverride; // Specific muzzle end point for this weapon.
    public float muzzleFlashDuration = 0.1f; // How long the muzzle flash stays visible for this weapon.


    [Header("Combat Stats")]
    public float damage = 10f; // Damage dealt by this weapon's bullets.
    public float fireRate = 0.5f; // Time between shots (seconds).
    [Range(0.1f, 1.0f)]
    [Tooltip("Bullet travel distance as a fraction of the map width (e.g., 0.25 for 1/4, 0.75 for 3/4).")]
    public float bulletRangeFraction = 0.5f; // How far the bullet travels as a fraction of map width.

    [Header("Shotgun Specific (if applicable)")]
    [Range(1, 20)] // Clamp value between 1 and 20 for reasonable shotgun pellets.
    public int shotgunPellets = 6; // Number of bullets for shotgun.
    [Range(0f, 30f)] // Max spread angle in degrees.
    public float shotgunSpreadAngle = 10f; // Total angle of spread for shotgun pellets.
}


/// <summary>
/// Manages weapon firing, muzzle flash effects, bullet spawning, and weapon switching.
/// This script should be attached to the player GameObject.
/// </summary>
public class WeaponController : MonoBehaviour
{
    // --- Weapon Type Enumeration ---
    public enum WeaponType { Single, Shotgun }

    // --- Configurable Variables (visible in Inspector) ---
    [Header("Weapon Configurations")]
    [SerializeField]
    private List<WeaponConfig> weapons = new List<WeaponConfig>(); // List of all available weapon configurations.

    [SerializeField]
    private int startingWeaponIndex = 0; // Index of the weapon to start with (0 for first weapon in list).

    [Header("Game World Dimensions (for bullet range calculation)")]
    [SerializeField]
    [Tooltip("The approximate width of your playable map area in Unity units.")]
    private float mapWidth = 20f; // Example: If your camera view is 10 units high, width might be 10 * aspect ratio.
    [SerializeField]
    [Tooltip("The approximate height of your playable map area in Unity units.")]
    private float mapHeight = 15f; // Example: If your camera orthographic size is 5, height is 10 units.

    // --- Private Internal State Variables ---
    private int currentWeaponIndex; // Index of the currently active weapon in the 'weapons' list.
    private WeaponConfig currentWeapon; // Reference to the currently active weapon's configuration.
    private float nextFireTime = 0f; // Timer to control fire rate.
    private AudioSource audioSource; // Reference to the AudioSource component.
    private SpriteRenderer playerSpriteRenderer; // Reference to the player's SpriteRenderer.


    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes references and sets up AudioSource.
    /// </summary>
    void Awake()
    {
        // Get the player's SpriteRenderer to update their visual.
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        if (playerSpriteRenderer == null)
        {
            UnityEngine.Debug.LogError("WeaponController: No SpriteRenderer found on the Player GameObject! Cannot update player sprite.");
        }

        // Get or add an AudioSource component to this GameObject.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false; // Don't play sound automatically.
            audioSource.spatialBlend = 0; // 2D sound.
        }

        // Set the initial weapon.
        SwitchWeapon(startingWeaponIndex);
    }

    /// <summary>
    /// Update is called once per frame.
    /// Checks for player input to fire the weapon and switch weapons.
    /// </summary>
    void Update()
    {
        // --- Weapon Firing Logic ---
        // Check for left mouse button click and fire rate cooldown.
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            FireWeapon();
            nextFireTime = Time.time + currentWeapon.fireRate; // Use current weapon's fire rate.
        }

        // --- Weapon Switching Logic ---
        // Example: Use number keys 1, 2, 3 to switch weapons.
        // Input.GetKeyDown checks for a key press only once.
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Alpha1 is the '1' key above QWERTY.
        {
            SwitchWeapon(0); // Switch to the first weapon in the list (Handgun).
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchWeapon(1); // Switch to the second weapon (Rifle).
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwitchWeapon(2); // Switch to the third weapon (Shotgun).
        }
        // Add more AlphaX checks for more weapons if needed.
    }

    /// <summary>
    /// Switches the currently active weapon.
    /// </summary>
    /// <param name="weaponIndex">The index of the weapon in the 'weapons' list to switch to.</param>
    private void SwitchWeapon(int weaponIndex)
    {
        if (weaponIndex >= 0 && weaponIndex < weapons.Count)
        {
            currentWeaponIndex = weaponIndex;
            currentWeapon = weapons[currentWeaponIndex]; // Set the current weapon config.

            // Update player's sprite to match the new weapon.
            if (playerSpriteRenderer != null && currentWeapon.playerWeaponSprite != null)
            {
                playerSpriteRenderer.sprite = currentWeapon.playerWeaponSprite;
            }
            else if (playerSpriteRenderer != null)
            {
                UnityEngine.Debug.LogWarning($"WeaponController: Player sprite for weapon '{currentWeapon.weaponName}' is not assigned. Player sprite might not update visually.");
            }

            UnityEngine.Debug.Log($"Switched to weapon: {currentWeapon.weaponName}");
        }
        else
        {
            UnityEngine.Debug.LogWarning($"WeaponController: Invalid weapon index {weaponIndex}. Not switching weapon.");
        }
    }

    /// <summary>
    /// Executes the weapon firing logic.
    /// Plays sound, shows muzzle flash, and spawns bullets.
    /// </summary>
    private void FireWeapon()
    {
        // Use the muzzleEndPointOverride from the current weapon config.
        Transform activeMuzzleEndPoint = currentWeapon.muzzleEndPointOverride;

        if (activeMuzzleEndPoint == null)
        {
            UnityEngine.Debug.LogError($"WeaponController: Muzzle End Point for {currentWeapon.weaponName} is not assigned! Cannot fire weapon.");
            return;
        }
        if (currentWeapon.bulletPrefab == null)
        {
            UnityEngine.Debug.LogError($"WeaponController: Bullet Prefab for {currentWeapon.weaponName} is not assigned! Cannot fire weapon.");
            return;
        }

        // 1. Play Shooting Sound
        if (currentWeapon.shootingSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(currentWeapon.shootingSound);
        }

        // 2. Show Muzzle Flash
        StartCoroutine(ShowMuzzleFlash(activeMuzzleEndPoint));

        // 3. Spawn Bullets
        float calculatedMaxDistance = Mathf.Max(mapWidth, mapHeight) * currentWeapon.bulletRangeFraction; // Use larger dimension for range.

        if (currentWeapon.weaponType == WeaponType.Single)
        {
            // For single shot, the bullet's rotation is the same as the muzzle end point's.
            GameObject bulletInstance = Instantiate(currentWeapon.bulletPrefab, activeMuzzleEndPoint.position, activeMuzzleEndPoint.rotation);
            // Pass the damage and maxDistance to the bullet.
            Bullet bulletScript = bulletInstance.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.damage = currentWeapon.damage;
                bulletScript.maxDistance = calculatedMaxDistance;
            }
        }
        else if (currentWeapon.weaponType == WeaponType.Shotgun)
        {
            SpawnShotgunPellets(activeMuzzleEndPoint, calculatedMaxDistance);
        }
    }

    /// <summary>
    /// Coroutine to display the muzzle flash for a short duration.
    /// </summary>
    /// <param name="muzzlePoint">The transform of the muzzle end point.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    private IEnumerator ShowMuzzleFlash(Transform muzzlePoint)
    {
        if (currentWeapon.muzzleFlashPrefab != null && muzzlePoint != null)
        {
            // Instantiate the muzzle flash at the muzzle end point's position and rotation.
            // Parent it to the muzzle end point so it moves/rotates with the gun.
            GameObject flashInstance = Instantiate(currentWeapon.muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation, muzzlePoint);

            // Ensure the flash is active (it should be by default if prefab is set up correctly).
            flashInstance.SetActive(true);

            // Wait for the specified duration.
            yield return new WaitForSeconds(currentWeapon.muzzleFlashDuration); // Use current weapon's muzzle flash duration.

            // Destroy the muzzle flash instance after its duration.
            Destroy(flashInstance);
        }
    }

    /// <summary>
    /// Spawns multiple pellets for a shotgun type weapon.
    /// </summary>
    /// <param name="muzzlePoint">The transform of the muzzle end point.</param>
    /// <param name="calculatedMaxDistance">The calculated max travel distance for the bullets.</param>
    private void SpawnShotgunPellets(Transform muzzlePoint, float calculatedMaxDistance)
    {
        // Calculate the starting angle for the spread.
        // If total spread is 10 degrees, start at -5 and end at +5 relative to gun's forward.
        float startAngle = -currentWeapon.shotgunSpreadAngle / 2f;
        float angleIncrement = currentWeapon.shotgunSpreadAngle / (currentWeapon.shotgunPellets - 1); // Angle between each pellet.

        for (int i = 0; i < currentWeapon.shotgunPellets; i++)
        {
            float currentAngleOffset = startAngle + i * angleIncrement;
            // The muzzlePoint.rotation is the player's current aiming rotation.
            // We apply the spread offset to this rotation.
            Quaternion pelletRotation = muzzlePoint.rotation * Quaternion.Euler(0, 0, currentAngleOffset);

            GameObject bulletInstance = Instantiate(currentWeapon.bulletPrefab, muzzlePoint.position, pelletRotation);
            // Pass the damage and maxDistance to the bullet.
            Bullet bulletScript = bulletInstance.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.damage = currentWeapon.damage;
                bulletScript.maxDistance = calculatedMaxDistance;
            }
        }
    }
}
