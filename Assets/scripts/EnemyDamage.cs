using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [Tooltip("Cooldown in seconds between hits to prevent spamming")]
    public float damageCooldown = 1.0f;
    [Tooltip("Amount of damage to deal per hit")]
    public int damageAmount = 10;

    private float lastHitTime;

    // Handle physical collisions
    private void OnCollisionEnter(Collision collision)
    {
        TryHitPlayer(collision.gameObject);
    }

    // Handle trigger overlaps
    private void OnTriggerEnter(Collider other)
    {
        TryHitPlayer(other.gameObject);
    }

    // Also check while staying in trigger (for continuous damage logic if needed, but here just for reliability)
    private void OnTriggerStay(Collider other)
    {
        TryHitPlayer(other.gameObject);
    }

    private void TryHitPlayer(GameObject target)
    {
        // Check cooldown
        if (Time.time < lastHitTime + damageCooldown) return;

        if (target.CompareTag("Player"))
        {
            // Look for the PlayerHealth script on the player
            PlayerHealth health = target.GetComponent<PlayerHealth>();

            // If not found on the object hit, check parent (sometimes collider is on a child)
            if (health == null)
            {
                health = target.GetComponentInParent<PlayerHealth>();
            }

            if (health != null)
            {
                health.TakeDamage(damageAmount);
                lastHitTime = Time.time;
                Debug.Log("Enemy hit the Player!");
            }
        }
    }
}
