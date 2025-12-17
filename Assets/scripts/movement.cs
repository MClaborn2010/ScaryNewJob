using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class movement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed in units per second")]
    public float moveSpeed = 5f;

    [Tooltip("How high the player can jump in units")]
    public float jumpHeight = 1.5f;

    [Tooltip("Downward force. Earth gravity is roughly -9.81")]
    public float gravity = -9.81f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private InputSystem_Actions _inputActions;
    private CharacterController _controller;
    private Vector3 _velocity; // Tracks vertical speed (falling)
    private bool _isGrounded;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        _inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Player.Disable();
    }

    private void Update()
    {
        // Safety check: Don't try to move if the controller is disabled
        if (_controller == null || !_controller.enabled) return;

        // 1. Custom Ground Check
        // Physics.CheckSphere creates a small invisible sphere at the feet. 
        // If it hits anything on the "Ground" layer, we are grounded.
        // This is much more reliable than _controller.isGrounded.
        if (groundCheck != null)
        {
            _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        else
        {
            // Fallback if user forgot to assign groundCheck
            _isGrounded = _controller.isGrounded;
        }

        // Reset velocity if on ground
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        // 2. Horizontal Movement (WASD)
        Vector2 inputVector = _inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 moveDirection = (transform.right * inputVector.x) + (transform.forward * inputVector.y);
        _controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        // 3. Jump
        if (_inputActions.Player.Jump.triggered && _isGrounded)
        {
            // Physics formula for jump velocity: v = sqrt(h * -2 * g)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 4. Apply Gravity
        _velocity.y += gravity * Time.deltaTime;

        // 5. Apply Vertical Movement
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}