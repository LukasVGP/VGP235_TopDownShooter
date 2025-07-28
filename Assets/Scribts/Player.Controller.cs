using UnityEngine; // Required for Unity functionalities like MonoBehaviour, Input, Transform, Time

/// <summary>
/// Controls the movement and aiming of the player character in a 2D top-down game.
/// This script handles forward movement in the mouse cursor's direction, player rotation towards the mouse cursor,
/// and custom crosshair display using Cursor.SetCursor, visible only when the right mouse button is pressed.
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

    [SerializeField]
    [Tooltip("Adjust this value to align the player's gun with the aiming direction. " +
             "If your character's gun points 'up' when rotation is 0, try -90. If it points 'right', try 0.")]
    private float rotationOffset = -90f; // Default adjustment for sprites initially facing 'up' (Y-axis).
                                         // You will likely need to tweak this value in the Inspector.

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

        // Initially, hide the custom crosshair and show the default cursor.
        SetCursorVisibility(false);

        UnityEngine.Debug.Log("PlayerController Start method called. Player movement and aiming are now active.");
    }

    /// <summary>
    /// Called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        // Ensure cursor is in its default hidden state when the script is enabled.
        SetCursorVisibility(false);
    }

    /// <summary>
    /// Called when the object becomes disabled or inactive.
    /// </summary>
    void OnDisable()
    {
        // When the script is disabled, revert to default cursor behavior.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Update is called once per frame.
    /// Handles player movement and aiming/rotation.
    /// </summary>
    void Update()
    {
        // Check if the right mouse button is held down to show the crosshair.
        // We invert the boolean here to counteract the observed editor behavior.
        bool showCrosshair = !Input.GetMouseButton(1); // Inverted: If button is held (true), this becomes false.
                                                       // If button is NOT held (false), this becomes true.
        SetCursorVisibility(showCrosshair); // Update cursor visibility based on mouse button state.

        // Only allow movement and aiming if the player is alive.
        if (playerHealth != null && playerHealth.IsAlive)
        {
            // --- Player Aiming/Rotation Logic (always happens) ---
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

            Vector2 directionToMouse = (mouseWorldPosition - transform.position).normalized;
            float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
            angle += rotationOffset;

            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);


            // --- Player Movement Logic (moves relative to aimed direction) ---
            float horizontalInput = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right Arrow
            float verticalInput = Input.GetAxisRaw("Vertical");     // W/S or Up/Down Arrow

            Vector3 movement = (transform.right * verticalInput + transform.up * horizontalInput).normalized;
            transform.position += movement * moveSpeed * Time.deltaTime;
        }
    }

    /// <summary>
    /// Called when the application gains or loses focus.
    /// Useful for managing cursor visibility and lock state in the editor.
    /// </summary>
    /// <param name="hasFocus">True if the application has focus, false otherwise.</param>
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            // When the application gains focus, re-evaluate cursor visibility based on current mouse button state.
            // We invert the logic here to counteract the observed editor behavior.
            bool showCrosshair = !Input.GetMouseButton(1); // Check right mouse button
            SetCursorVisibility(showCrosshair);
        }
        else
        {
            // When the application loses focus, show the default cursor so user can interact with editor.
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    /// <summary>
    /// Sets the custom cursor or default cursor based on the provided boolean.
    /// </summary>
    /// <param name="showCustom">If true, shows the custom crosshair; otherwise, shows the default cursor.</param>
    private void SetCursorVisibility(bool showCustom)
    {
        if (showCustom && crosshairTexture != null)
        {
            Cursor.SetCursor(crosshairTexture, crosshairHotspot, CursorMode.Auto);
            Cursor.visible = false; // Hide default cursor
            Cursor.lockState = CursorLockMode.None; // Ensure cursor is not locked
        }
        else
        {
            // If not showing custom, or if custom texture is not assigned, show default cursor.
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            // Only log warning once to avoid spamming console
            if (crosshairTexture == null && Time.frameCount == 1)
            {
                UnityEngine.Debug.LogWarning("PlayerController: No crosshair texture assigned. Default mouse cursor will be used.");
            }
        }
    }
}