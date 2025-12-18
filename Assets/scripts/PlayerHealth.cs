using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Death Settings")]
    [Tooltip("Assign the Game Over UI Panel here.")]
    public GameObject gameOverPanel;

    [Tooltip("Assign the movement script here.")]
    public movement playerMovement;

    [Tooltip("Assign the MouseLook script here.")]
    public MouseLook mouseLook;

    [Tooltip("Assign the Main Camera here to animate it dropping.")]
    public Transform playerCamera;

    public float deathDropSpeed = 2f;

    private PlayerDamageFeedback damageFeedback;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        damageFeedback = GetComponent<PlayerDamageFeedback>();

        // Auto-find scripts if not assigned
        if (playerMovement == null) playerMovement = GetComponent<movement>();
        if (mouseLook == null) mouseLook = GetComponentInChildren<MouseLook>();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"Player took {amount} damage. Current Health: {currentHealth}");

        // Trigger visual feedback
        if (damageFeedback != null)
        {
            damageFeedback.TakeHit();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player has died!");

        // 1. Disable Inputs/Movement
        if (playerMovement != null) playerMovement.enabled = false;
        if (mouseLook != null) mouseLook.enabled = false;

        // 2. Unlock Cursor so player can click buttons (if any)

        // 2. Unlock Cursor so player can click buttons (if any)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. Start Death Animation
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // Drop Camera to floor (Y = 0.2)
        if (playerCamera != null)
        {
            Vector3 startPos = playerCamera.localPosition;
            Vector3 targetPos = new Vector3(startPos.x, 0.2f, startPos.z);

            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * deathDropSpeed;
                playerCamera.localPosition = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }
        }

        // Show Game Over Panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // Optional: Fade in if it has a CanvasGroup
            CanvasGroup cg = gameOverPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                while (cg.alpha < 1f)
                {
                    cg.alpha += Time.deltaTime;
                    yield return null;
                }
                cg.alpha = 1f;
            }
        }
    }
}
