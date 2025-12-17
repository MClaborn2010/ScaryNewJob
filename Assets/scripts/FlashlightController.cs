using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightController : MonoBehaviour
{
    [Header("Battery Settings")]
    [Tooltip("Current battery level.")]
    public float currentBattery = 100f;

    [Tooltip("Maximum battery capacity.")]
    public float maxBattery = 100f;

    [Tooltip("How much battery is drained per second when on.")]
    public float drainRate = 1.0f;

    [Tooltip("Reference to the Light component.")]
    public Light flashlightLight;

    [HideInInspector]
    public bool isHeld = false;

    private bool _isOn = false;
    private float _logTimer;
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
        if (flashlightLight == null)
        {
            flashlightLight = GetComponentInChildren<Light>();
        }

        // Ensure light state matches _isOn
        if (flashlightLight != null)
        {
            flashlightLight.enabled = _isOn;
        }

        Debug.Log($"Flashlight Initialized. Battery: {currentBattery}, Light Component Found: {flashlightLight != null}");
    }

    private void Update()
    {
        // Check for Input (assuming 'F' is mapped to an action, or we check key directly for now if action doesn't exist)
        if (isHeld && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            ToggleLight();
        }

        // Debug Logging
        _logTimer += Time.deltaTime;
        if (_logTimer >= 1.0f)
        {
            Debug.Log($"Flashlight Status - Battery: {currentBattery:F1}/{maxBattery} | IsOn: {_isOn} | Charging: {currentBattery < maxBattery}");
            _logTimer = 0f;
        }

        if (_isOn)
        {
            if (currentBattery > 0)
            {
                currentBattery -= drainRate * Time.deltaTime;
                if (currentBattery <= 0)
                {
                    currentBattery = 0;
                    ToggleLight(false);
                }
            }
        }
    }

    public void ToggleLight()
    {
        if (currentBattery > 0)
        {
            ToggleLight(!_isOn);
        }
        else
        {
            Debug.Log("Cannot toggle light: Battery is empty.");
        }
    }

    public void ToggleLight(bool state)
    {
        _isOn = state;
        if (flashlightLight != null)
        {
            flashlightLight.enabled = _isOn;
        }
        Debug.Log($"Flashlight Toggled: {_isOn}. Battery: {currentBattery}");
    }

    public void Charge(float amount)
    {
        currentBattery += amount;
        if (currentBattery > maxBattery)
        {
            currentBattery = maxBattery;
        }
        // Optional: Debug log to see charging
        // Debug.Log($"Charging: {currentBattery}/{maxBattery}");
    }
}
