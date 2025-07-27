using UnityEngine; // Required for Unity functionalities like MonoBehaviour, Input, Transform, Time

/// <summary>
/// Controls the movement and aiming of the player character in a 2D top-down game.
/// This script handles WASD/arrow key movement, player rotation towards the mouse cursor,
/// and custom crosshair display.
/// </summary>
public class PlayerController : MonoBehaviour
{
    // --- Configurable Variables (visible in Inspector) ---
    [SerializeField]
    private float moveSpeed = 10f; // Speed at which the player moves.

    [SerializeField]
    private float rotationSpeed = 720f; // Speed at which the player rotates (degrees per second).
                                        // A higher value means faster, snappier rotation.

    [SerializeField]
    private Texture2D crosshairTexture; // Assign your custom crosshair image here in the Inspector.

    [SerializeField]
    private Vector2 crosshairHotspot = new Vector2(16, 16); // The pixel offset from the top-left
                                                            // of the crosshair texture that acts as its "hotspot" (center).
                                                            // For a 32x32 pixel crosshair, (16,16) is the center. Adjust for your image size.

    // --- Private Internal References ---
    private Health playerHealth; // Reference to the player's Health component.

    /// <summary>
    /// Start is called once before the first frame update.
    /// Used for initial setup, like hiding the default cursor and getting component references.
    /// </summary>
    void Start()
    {
        // Get the Health component attached to this same GameObject.
        playerHealth = GetComponent<Health>();
        if (playerHealth == null)
        {
            UnityEngine.Debug.LogError("PlayerController: No Health component found on the Player GameObject!");
        }

        // --- Custom Cursor Setup ---
        // Check if a crosshair texture is assigned.
        if (crosshairTexture != null)
        {
            // Set the custom cursor. CursorMode.Auto uses software rendering.
            Cursor.SetCursor(crosshairTexture, crosshairHotspot, CursorMode.Auto);
        }
        else
        {
            UnityEngine.Debug.LogWarning("PlayerController: No crosshair texture assigned. Default mouse cursor will be used.");
        }

        // Optionally hide the default cursor entirely if you don't want any cursor at all
        // or if your custom cursor is handled by a UI image instead of Cursor.SetCursor.
        // Cursor.visible = false; // Uncomment this if Cursor.SetCursor isn't enough or you use a UI image for crosshair.

        UnityEngine.Debug.Log("PlayerController Start method called. Player movement and aiming are now active.");
    }

    /// <summary>
    /// Update is called once per frame.
    /// Handles player movement and aiming/rotation.
    /// </summary>
    void Update()
    {
        // Only allow movement and aiming if the player is alive.
        if (playerHealth != null && playerHealth.IsAlive)
        {
            // --- Player Movement Logic ---
            // (Existing movement code remains the same)

            // Get horizontal and vertical input for WASD/Arrow keys
            float horizontalInput = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right Arrow
            float verticalInput = Input.GetAxisRaw("Vertical");     // W/S or Up/Down Arrow

            // Create a movement vector based on input.
            // Vector3.right is (1,0,0), Vector3.up is (0,1,0). For 2D top-down on XY plane.
            Vector3 movement = new Vector3(horizontalInput, verticalInput, 0f).normalized;

            // Apply movement to the player's position.
            // Time.deltaTime ensures movement is frame-rate independent.
            transform.position += movement * moveSpeed * Time.deltaTime;


            // --- Player Aiming/Rotation Logic ---
            // Get the mouse position in screen coordinates.
            Vector3 mouseScreenPosition = Input.mousePosition;

            // Convert the mouse screen position to world coordinates.
            // The Z-coordinate needs to be set to the player's Z-coordinate for correct projection.
            // If your game is strictly 2D on the XY plane, player.transform.position.z is usually 0.
            // Make sure your camera's Z-position is negative (e.g., -10) to look down on the XY plane.
            mouseScreenPosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

            // Calculate the direction vector from the player to the mouse position.
            Vector2 directionToMouse = (mouseWorldPosition - transform.position).normalized;

            // Calculate the angle in degrees. Atan2 returns the angle in radians between the X-axis and a point (y, x).
            // We convert it to degrees and adjust for Unity's coordinate system (0 degrees is usually positive X,
            // positive rotation is counter-clockwise).
            float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;

            // Adjust the angle for your sprite's default orientation.
            // If your sprite is facing "right" by default, no adjustment might be needed.
            // If it's facing "up" by default, you might need to subtract 90 degrees (angle - 90).
            // Experiment with this value. For many top-down sprites facing "up", -90 is common.
            // For sprites facing "right", 0 or -90 might work depending on how you want the "front" to align.
            angle -= 90f; // Common adjustment if your sprite's "forward" is initially "up" (Y-axis).

            // Create a target rotation quaternion from the calculated angle.
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

            // Smoothly rotate the player towards the target rotation.
            // Slerp interpolates between two rotations. Time.deltaTime ensures frame-rate independence.
            // rotationSpeed controls how fast the rotation happens.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}