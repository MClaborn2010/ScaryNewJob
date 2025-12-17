using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Item Properties")]
    public string itemName = "New Item";

    [TextArea]
    public string description = "Item Description";

    public Sprite icon;

    public float weight = 1.0f;
    public int value = 10;
    public bool isStackable = false;

    [Header("Hold Settings")]
    [Tooltip("Position offset when held in hand.")]
    public Vector3 holdPosition = Vector3.zero;

    [Tooltip("Rotation offset (Euler angles) when held in hand.")]
    public Vector3 holdRotation = Vector3.zero;

    // Optional: Add an ID or Type if needed later
    // public int itemID;
    // public ItemType itemType;
}
