using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

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

    [Header("UI")]
    [Tooltip("Assign a TextMeshProUGUI component to display item info.")]
    public TextMeshProUGUI itemInfoText;

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

    private Item currentItem; // Track current item to avoid spamming UI updates

    private void HandleInteraction()
    {
        // Create a ray from the camera's position forward
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        bool hitSomething;

        // Ensure we ignore the "UI" layer (usually layer 5) to prevent raycasting against the text itself
        // We combine the user's mask with a mask that ignores the UI layer
        int layerMask = interactionLayerMask.value;
        if (layerMask == 0) layerMask = Physics.DefaultRaycastLayers;
        layerMask = layerMask & ~(1 << 5); // Bitwise AND with NOT UI layer (5)

        hitSomething = Physics.Raycast(ray, out hit, interactionDistance, layerMask);

        Item item = null;

        if (hitSomething)
        {
            // Check if the object we hit has an Item component
            item = hit.collider.GetComponent<Item>();

            // If no item found directly, check if it's a FlashlightStand holding an item
            if (item == null)
            {
                FlashlightStand stand = hit.collider.GetComponent<FlashlightStand>();
                if (stand != null && stand.flashlightObject != null)
                {
                    item = stand.flashlightObject.GetComponent<Item>();
                }
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

        // Update UI only if changed
        if (itemInfoText != null)
        {
            if (item != currentItem)
            {
                currentItem = item;
                if (item != null)
                {
                    itemInfoText.text = $"<size=70%>{item.description}</size>";
                    if (!itemInfoText.gameObject.activeSelf) itemInfoText.gameObject.SetActive(true);
                }
                else
                {
                    if (itemInfoText.gameObject.activeSelf) itemInfoText.gameObject.SetActive(false);
                }
            }
        }
    }
}
