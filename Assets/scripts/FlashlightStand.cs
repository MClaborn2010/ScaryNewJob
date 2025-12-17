using UnityEngine;

public class FlashlightStand : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Flashlight GameObject to move.")]
    public GameObject flashlightObject;

    [Tooltip("The position where the flashlight should sit on the stand.")]
    public Transform standPosition;

    [Header("Stand Settings")]
    [Tooltip("Position offset relative to the stand position.")]
    public Vector3 standOffset = Vector3.zero;

    [Tooltip("Rotation offset relative to the stand position.")]
    public Vector3 standRotation = Vector3.zero;

    [Header("Charging Settings")]
    [Tooltip("Amount of battery to charge per second.")]
    public float chargeAmount = 10.0f;

    private bool _isPlayerHolding = false;
    private Vector3 _targetWorldScale;
    private Rigidbody _flashlightRb;
    private FlashlightController _flashlightController;

    private void Start()
    {
        // If standPosition is not assigned, use this object's transform
        if (standPosition == null)
        {
            standPosition = transform;
        }

        if (flashlightObject != null)
        {
            // Capture the initial world scale (assuming it looks correct at Start)
            _targetWorldScale = flashlightObject.transform.lossyScale;

            // Get Rigidbody and set to kinematic to prevent physics issues
            _flashlightRb = flashlightObject.GetComponent<Rigidbody>();
            if (_flashlightRb != null)
            {
                _flashlightRb.isKinematic = true;
            }

            // Get FlashlightController
            _flashlightController = flashlightObject.GetComponent<FlashlightController>();

            // Ensure flashlight starts at the stand position if not already
            if (!_isPlayerHolding)
            {
                PlaceOnStand();
            }
        }
    }

    private void Update()
    {
        // Charge the flashlight if it's on the stand
        if (!_isPlayerHolding && _flashlightController != null)
        {
            _flashlightController.Charge(chargeAmount * Time.deltaTime);
        }
    }

    public void Interact(Transform playerHand)
    {
        if (flashlightObject == null)
        {
            Debug.LogWarning("Flashlight Object not assigned in FlashlightStand!");
            return;
        }

        if (!_isPlayerHolding)
        {
            // Pick up
            flashlightObject.transform.SetParent(playerHand);

            // Restore world scale
            RestoreWorldScale(playerHand);

            // Apply Item hold settings if available
            Item item = flashlightObject.GetComponent<Item>();
            if (item != null)
            {
                flashlightObject.transform.localPosition = item.holdPosition;
                flashlightObject.transform.localRotation = Quaternion.Euler(item.holdRotation);
            }
            else
            {
                flashlightObject.transform.localPosition = Vector3.zero;
                flashlightObject.transform.localRotation = Quaternion.identity;
            }

            _isPlayerHolding = true;
            if (_flashlightController != null)
            {
                _flashlightController.isHeld = true;
            }
            Debug.Log("Picked up Flashlight");
        }
        else
        {
            // Place back
            PlaceOnStand();
            _isPlayerHolding = false;
            if (_flashlightController != null)
            {
                _flashlightController.isHeld = false;
            }
            Debug.Log("Placed Flashlight back on stand");
        }
    }

    private void PlaceOnStand()
    {
        flashlightObject.transform.SetParent(standPosition);
        flashlightObject.transform.localPosition = standOffset;
        flashlightObject.transform.localRotation = Quaternion.Euler(standRotation);
        RestoreWorldScale(standPosition);
    }

    private void RestoreWorldScale(Transform parent)
    {
        // Calculate the necessary local scale to maintain the target world scale
        // localScale = targetWorldScale / parentWorldScale
        // Note: This assumes roughly axis-aligned scaling. 
        // If the parent is rotated and non-uniformly scaled, skewing is unavoidable in Unity.

        Vector3 parentScale = parent.lossyScale;

        // Avoid division by zero
        if (parentScale.x == 0) parentScale.x = 0.001f;
        if (parentScale.y == 0) parentScale.y = 0.001f;
        if (parentScale.z == 0) parentScale.z = 0.001f;

        flashlightObject.transform.localScale = new Vector3(
            _targetWorldScale.x / parentScale.x,
            _targetWorldScale.y / parentScale.y,
            _targetWorldScale.z / parentScale.z
        );
    }
}
