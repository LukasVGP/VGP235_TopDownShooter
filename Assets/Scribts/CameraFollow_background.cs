using UnityEngine;

/// <summary>
/// Controls a 2D camera to smoothly follow a target GameObject (e.g., the player).
/// The camera follows horizontally (X-axis) but maintains a fixed vertical (Y-axis) position.
/// This script should be attached to the Main Camera.
/// </summary>
public class CameraFollow_background : MonoBehaviour // Renamed class to match new file name
{
    [Header("Target Settings")]
    [SerializeField]
    [Tooltip("Drag the Player GameObject here. The camera will follow this target.")]
    private Transform target; // The player's Transform to follow.

    [Header("Follow Settings")]
    [SerializeField]
    [Range(0.1f, 10f)]
    [Tooltip("How smoothly the camera follows the target. Lower values are smoother.")]
    private float smoothSpeed = 0.125f; // The smoothness of the camera's movement.

    [SerializeField]
    [Tooltip("The offset from the target's position. Z-axis controls the camera's depth.")]
    private Vector3 offset = new Vector3(0f, 0f, -10f); // Offset from the target (x, y, z).
                                                        // Z-value should be negative for 2D top-down.

    private float fixedYPosition; // The Y-position the camera will maintain.

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Used to store the initial fixed Y-position of the camera.
    /// </summary>
    void Awake()
    {
        // Store the camera's initial Y position. This will be the fixed vertical position.
        fixedYPosition = transform.position.y;
    }

    /// <summary>
    /// LateUpdate is called once per frame, after all Update functions have been called.
    /// This is ideal for camera movement to ensure the target has already moved for the current frame.
    /// </summary>
    void LateUpdate()
    {
        // Only follow if a target is assigned.
        if (target == null)
        {
            // Try to find the player if not assigned, as they are spawned by GameManager.
            GameObject playerGameObject = GameObject.FindWithTag("Player");
            if (playerGameObject != null)
            {
                target = playerGameObject.transform;
                // Once found, set the initial camera position immediately to avoid a jump.
                // Maintain fixedYPosition and apply offset.
                transform.position = new Vector3(target.position.x + offset.x, fixedYPosition + offset.y, offset.z);
            }
            else
            {
                // Log a warning if player is still not found.
                UnityEngine.Debug.LogWarning("CameraFollow_background: Player GameObject not found! Ensure player is tagged 'Player' and exists in the scene.");
                return; // Exit if no target.
            }
        }

        // Calculate the desired position of the camera.
        // Only update X based on target.X, keep Y fixed (using fixedYPosition + offset.y),
        // and use offset.Z for camera depth.
        Vector3 desiredPosition = new Vector3(target.position.x + offset.x, fixedYPosition + offset.y, offset.z);

        // Smoothly interpolate between the camera's current position and the desired position.
        // The 10f multiplier makes the Lerp more responsive with Time.deltaTime.
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime * 10f);

        // Apply the smoothed position to the camera.
        transform.position = smoothedPosition;
    }
}
