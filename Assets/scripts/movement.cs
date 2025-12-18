using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class movement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed in units per second")]
    public float moveSpeed = 5f;

    [Tooltip("Sprint speed in units per second")]
    public float sprintSpeed = 8f;

    [Tooltip("How high the player can jump in units")]
    public float jumpHeight = 1.5f;

    [Tooltip("Downward force. Earth gravity is roughly -9.81")]
    public float gravity = -9.81f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Audio")]
    public AudioSource footstepSource;
    public AudioClip[] footstepSounds;
    [Tooltip("Time between footsteps when walking")]
    public float walkStepInterval = 0.5f;
    [Tooltip("Time between footsteps when sprinting")]
    public float sprintStepInterval = 0.3f;

    private InputSystem_Actions _inputActions;
    private CharacterController _controller;
    private Vector3 _velocity; // Tracks vertical speed (falling)
    private bool _isGrounded;
    private float _nextStepTime;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _controller = GetComponent<CharacterController>();
        if (footstepSource == null) footstepSource = GetComponent<AudioSource>();
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

        // Check for sprint
        float currentSpeed = moveSpeed;
        if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
        {
            currentSpeed = sprintSpeed;
        }

        // 3. Jump
        if (_inputActions.Player.Jump.triggered && _isGrounded)
        {
            // Physics formula for jump velocity: v = sqrt(h * -2 * g)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 4. Apply Gravity
        _velocity.y += gravity * Time.deltaTime;

        // 5. Apply Movement (Horizontal + Vertical combined)
        // We combine them into one Move call so CharacterController.velocity is accurate
        Vector3 finalMovement = (moveDirection * currentSpeed) + _velocity;
        _controller.Move(finalMovement * Time.deltaTime);

        // 6. Footsteps
        HandleFootsteps(currentSpeed);
    }

    private void HandleFootsteps(float currentSpeed)
    {
        // Check if we are actually moving (ignoring vertical movement)
        Vector3 horizontalVelocity = new Vector3(_controller.velocity.x, 0, _controller.velocity.z);
        float speed = horizontalVelocity.magnitude;

        // Debugging - Uncomment if sound isn't working
        Debug.Log($"Footsteps Debug: Grounded={_isGrounded}, Speed={speed}, Time={Time.time}, NextStep={_nextStepTime}");

        if (!_isGrounded) return;

        if (footstepSource == null)
        {
            Debug.LogWarning("Footstep Source is NULL on movement script!");
            return;
        }

        if (footstepSounds.Length == 0)
        {
            Debug.LogWarning("No Footstep Sounds assigned in movement script!");
            return;
        }

        if (speed < 0.1f) return;

        if (Time.time >= _nextStepTime)
        {
            // Play a random footstep sound
            AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            footstepSource.PlayOneShot(clip);

            Debug.Log("Playing Footstep Sound: " + clip.name);
            // We check if currentSpeed is close to sprintSpeed
            bool isSprinting = Mathf.Abs(currentSpeed - sprintSpeed) < 0.1f;
            float interval = isSprinting ? sprintStepInterval : walkStepInterval;

            _nextStepTime = Time.time + interval;
        }
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