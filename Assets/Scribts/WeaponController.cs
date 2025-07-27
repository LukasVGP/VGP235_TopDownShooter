using UnityEngine;
using System.Collections; // Required for Coroutines

/// <summary>
/// Manages weapon firing, muzzle flash effects, and bullet spawning.
/// This script should be attached to the player or a dedicated weapon GameObject.
/// </summary>
public class WeaponController : MonoBehaviour
{
    // --- Weapon Type Enumeration ---
    public enum WeaponType { Single, Shotgun }

    // --- Configurable Variables (visible in Inspector) ---
    [Header("Weapon Settings")]
    [SerializeField]
    private WeaponType currentWeaponType = WeaponType.Single; // Type of firing (single shot or shotgun spread).

    [SerializeField]
    private float fireRate = 0.5f; // Time between shots (seconds). Lower value = faster firing.

    [SerializeField]
    private GameObject bulletPrefab; // Assign your Bullet Prefab here.

    [SerializeField]
    private Transform muzzleEndPoint; // Assign an empty GameObject here, positioned at the end of the gun muzzle.

    [Header("Muzzle Flash Settings")]
    [SerializeField]
    private GameObject muzzleFlashPrefab; // Assign your Muzzle Flash Prefab here.
                                          // This should be a GameObject containing a SpriteRenderer for the flash.

    [SerializeField]
    private float muzzleFlashDuration = 0.1f; // How long the muzzle flash stays visible.

    [Header("Audio Settings")]
    [SerializeField]
    private AudioClip shootingSound; // Assign your shooting sound effect here.
    private AudioSource audioSource; // Reference to the AudioSource component.

    [Header("Shotgun Specific Settings")]
    [SerializeField]
    [Range(1, 20)] // Clamp value between 1 and 20 for reasonable shotgun pellets.
    private int shotgunPellets = 12; // Number of bullets for shotgun.

    [SerializeField]
    [Range(0f, 30f)] // Max spread angle in degrees.
    private float shotgunSpreadAngle = 10f; // Total angle of spread for shotgun pellets.

    // --- Private Internal State Variables ---
    private float nextFireTime = 0f; // Timer to control fire rate.

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Gets or adds an AudioSource component.
    /// </summary>
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false; // Don't play sound automatically.
            audioSource.spatialBlend = 0; // 2D sound.
        }

        // Ensure muzzle flash is initially off if it's a child GameObject.
        if (muzzleFlashPrefab != null)
        {
            // If muzzleFlashPrefab is a child of this GameObject, ensure it's inactive.
            // If it's a separate prefab, this check might not be directly applicable here.
            // For a prefab, we'll instantiate and then activate/deactivate.
        }
    }

    /// <summary>
    /// Update is called once per frame.
    /// Checks for player input to fire the weapon.
    /// </summary>
    void Update()
    {
        // Check for left mouse button click and fire rate cooldown.
        // Input.GetButton("Fire1") maps to left mouse button by default.
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            FireWeapon();
            nextFireTime = Time.time + fireRate; // Set next allowed fire time.
        }
    }

    /// <summary>
    /// Executes the weapon firing logic.
    /// Plays sound, shows muzzle flash, and spawns bullets.
    /// </summary>
    private void FireWeapon()
    {
        if (muzzleEndPoint == null)
        {
            UnityEngine.Debug.LogError("WeaponController: Muzzle End Point is not assigned! Cannot fire weapon.");
            return;
        }
        if (bulletPrefab == null)
        {
            UnityEngine.Debug.LogError("WeaponController: Bullet Prefab is not assigned! Cannot fire weapon.");
            return;
        }

        // 1. Play Shooting Sound
        if (shootingSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootingSound);
        }

        // 2. Show Muzzle Flash
        StartCoroutine(ShowMuzzleFlash());

        // 3. Spawn Bullets
        if (currentWeaponType == WeaponType.Single)
        {
            SpawnBullet(muzzleEndPoint.rotation); // Single bullet, uses player's current rotation.
        }
        else if (currentWeaponType == WeaponType.Shotgun)
        {
            SpawnShotgunPellets();
        }
    }

    /// <summary>
    /// Coroutine to display the muzzle flash for a short duration.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution.</returns>
    private IEnumerator ShowMuzzleFlash()
    {
        if (muzzleFlashPrefab != null && muzzleEndPoint != null)
        {
            // Instantiate the muzzle flash at the muzzle end point's position and rotation.
            // Parent it to the muzzle end point so it moves/rotates with the gun.
            GameObject flashInstance = Instantiate(muzzleFlashPrefab, muzzleEndPoint.position, muzzleEndPoint.rotation, muzzleEndPoint);

            // Ensure the flash is active (it should be by default if prefab is set up correctly).
            flashInstance.SetActive(true);

            // Wait for the specified duration.
            yield return new WaitForSeconds(muzzleFlashDuration);

            // Destroy the muzzle flash instance after its duration.
            Destroy(flashInstance);
        }
    }

    /// <summary>
    /// Spawns a single bullet with the given rotation.
    /// </summary>
    /// <param name="rotation">The rotation of the spawned bullet.</param>
    private void SpawnBullet(Quaternion rotation)
    {
        // Instantiate the bullet prefab at the muzzle end point's position and rotation.
        // The bullet's own script will handle its movement.
        Instantiate(bulletPrefab, muzzleEndPoint.position, rotation);
    }

    /// <summary>
    /// Spawns multiple pellets for a shotgun type weapon.
    /// </summary>
    private void SpawnShotgunPellets()
    {
        // Calculate the starting angle for the spread.
        // If total spread is 10 degrees, start at -5 and end at +5 relative to gun's forward.
        float startAngle = -shotgunSpreadAngle / 2f;
        float angleIncrement = shotgunSpreadAngle / (shotgunPellets - 1); // Angle between each pellet.

        // Loop to spawn each pellet.
        for (int i = 0; i < shotgunPellets; i++)
        {
            // Calculate the current pellet's angle relative to the gun's forward direction.
            float currentAngleOffset = startAngle + i * angleIncrement;

            // Get the gun's current forward direction (which is transform.right if sprite faces right).
            Vector2 gunForward = muzzleEndPoint.right; // Assuming muzzleEndPoint's right is its "forward" direction.

            // Rotate the gun's forward direction by the current angle offset.
            Quaternion pelletRotation = Quaternion.Euler(0, 0, transform.eulerAngles.z + currentAngleOffset);

            // Spawn the bullet with the calculated spread rotation.
            SpawnBullet(pelletRotation);
        }
    }
}