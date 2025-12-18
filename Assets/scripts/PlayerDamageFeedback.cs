using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerDamageFeedback : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("Assign the UI Image/Panel that is colored red here.")]
    public Image bloodPanel;
    
    [Tooltip("How fast the blood fades away.")]
    public float flashSpeed = 2f;

    private void Start()
    {
        if (bloodPanel != null)
        {
            // Ensure it starts invisible
            Color c = bloodPanel.color;
            c.a = 0f;
            bloodPanel.color = c;
        }
        else
        {
            Debug.LogWarning("PlayerDamageFeedback: No Blood Panel assigned!");
        }
    }

    public void TakeHit()
    {
        if (bloodPanel != null)
        {
            // Set alpha to visible immediately
            Color c = bloodPanel.color;
            c.a = 0.5f; // 50% opacity red
            bloodPanel.color = c;
            
            // Stop any existing fade and start a new one
            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }
    }

    IEnumerator FadeOut()
    {
        while (bloodPanel.color.a > 0.01f)
        {
            Color c = bloodPanel.color;
            c.a = Mathf.Lerp(c.a, 0f, flashSpeed * Time.deltaTime);
            bloodPanel.color = c;
            yield return null;
        }
        
        // Ensure it's fully invisible at the end
        Color finalColor = bloodPanel.color;
        finalColor.a = 0f;
        bloodPanel.color = finalColor;
    }
}
