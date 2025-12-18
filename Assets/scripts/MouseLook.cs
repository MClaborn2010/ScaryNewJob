using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Sensitivity of the mouse movement.")]
    public float mouseSensitivity = 15f;

    [Tooltip("Assign the Player object here so it rotates left/right.")]
    public Transform playerBody;

    private float _xRotation = 0f;
    private InputSystem_Actions _inputActions;
    private int _framesSkipped = 0;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Player.Disable();
    }

    private void Start()
    {
        // Initialize rotation to current camera rotation to prevent snapping
        // We handle the 0-360 wrapping to get a range of -180 to 180
        Vector3 currentRot = transform.localRotation.eulerAngles;
        _xRotation = currentRot.x;
        if (_xRotation > 180) _xRotation -= 360;

        // Lock the cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Skip the first few frames to avoid the "mouse centering" jump
        if (_framesSkipped < 5)
        {
            _framesSkipped++;
            return;
        }

        // Read the mouse delta (change in position)
        Vector2 mouseDelta = _inputActions.Player.Look.ReadValue<Vector2>();

        // Adjust for sensitivity
        // FIX: We do NOT multiply by Time.deltaTime here. 
        // Mouse delta is a distance (pixels), not a speed. 
        // Multiplying by deltaTime makes the camera jump if the frame rate drops (like when UI spawns).
        // We multiply by a small constant (e.g. 0.02) to keep the Inspector sensitivity values comfortable.
        float mouseX = mouseDelta.x * mouseSensitivity * 0.02f;
        float mouseY = mouseDelta.y * mouseSensitivity * 0.02f;

        // 1. Vertical Rotation (Looking Up/Down)
        // We subtract mouseY because positive Y mouse movement means looking UP, 
        // which is a negative rotation around the X axis.
        _xRotation -= mouseY;

        // Clamp the rotation so we can't look behind us (neck snapping)
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        // Apply the rotation to the Camera (Local Rotation)
        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        // 2. Horizontal Rotation (Looking Left/Right)
        // We rotate the entire Player Body, not just the camera
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}
