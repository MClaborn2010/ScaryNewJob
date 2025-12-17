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
        // Lock the cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Read the mouse delta (change in position)
        Vector2 mouseDelta = _inputActions.Player.Look.ReadValue<Vector2>();

        // Adjust for sensitivity and frame time
        // Note: The New Input System usually returns values that are already frame-rate independent 
        // if using "Value" type, but multiplying by Time.deltaTime is still common practice 
        // to make "sensitivity" feel like the old system. 
        // However, raw mouse delta is usually pixels. 
        // Let's stick to a simple multiplier.
        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

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
