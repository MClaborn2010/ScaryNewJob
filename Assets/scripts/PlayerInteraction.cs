using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("How far the player can reach to interact with items.")]
    public float interactionDistance = 3.0f;

    [Tooltip("Layers that block the interaction ray (e.g., walls).")]
    public LayerMask interactionLayerMask;

    [Header("References")]
    [Tooltip("The transform representing the player's hand.")]
    public Transform playerHand;

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

    private void Update()
    {
        HandleInteraction();
    }

    private void HandleInteraction()
    {
        // Create a ray from the camera's position forward
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        bool hitSomething;
        if (interactionLayerMask.value != 0)
        {
            hitSomething = Physics.Raycast(ray, out hit, interactionDistance, interactionLayerMask);
        }
        else
        {
            hitSomething = Physics.Raycast(ray, out hit, interactionDistance);
        }

        if (hitSomething)
        {
            // Check if the object we hit has an Item component
            Item item = hit.collider.GetComponent<Item>();

            // If no item found directly, check if it's a FlashlightStand holding an item
            if (item == null)
            {
                FlashlightStand stand = hit.collider.GetComponent<FlashlightStand>();
                if (stand != null && stand.flashlightObject != null)
                {
                    item = stand.flashlightObject.GetComponent<Item>();
                }
            }

            if (item != null)
            {
                // Log the description
                Debug.Log($"Looking at: {item.itemName} - {item.description}");
            }

            // Check for Interaction Input
            if (_inputActions.Player.Interact.WasPressedThisFrame())
            {
                // Check for FlashlightStand on the object or its parents
                FlashlightStand stand = hit.collider.GetComponentInParent<FlashlightStand>();
                if (stand != null)
                {
                    if (playerHand != null)
                    {
                        stand.Interact(playerHand);
                    }
                    else
                    {
                        Debug.LogError("Player Hand not assigned in PlayerInteraction script!");
                    }
                }
            }
        }
    }
}
