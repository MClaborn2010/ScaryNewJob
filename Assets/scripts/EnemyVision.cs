using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [Header("Settings")]
    public float viewDistance = 10f;
    public float viewAngle = 90f;
    public Vector3 eyeOffset = new Vector3(0, 1.5f, 0); // Higher up to avoid ground/self

    [Header("Debug")]
    public bool canSeePlayer;
    public string debugStatus;

    private Transform player;

    void Start()
    {
        // Find player by tag
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;
        else Debug.LogError("EnemyVision: No object with tag 'Player' found!");
    }

    void Update()
    {
        if (!player) return;

        canSeePlayer = false;
        debugStatus = "Searching...";

        // 1. Distance Check
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > viewDistance)
        {
            debugStatus = "Player too far";
            return;
        }

        // 2. Angle Check
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, dirToPlayer) > viewAngle / 2)
        {
            debugStatus = "Player outside angle";
            return;
        }

        // 3. Line of Sight (Raycast)
        // Start from our eye position
        Vector3 startPos = transform.position + eyeOffset;
        // End at player's center (approximate)
        Vector3 endPos = player.position + Vector3.up * 1.0f;

        // Perform Linecast
        RaycastHit hit;
        if (Physics.Linecast(startPos, endPos, out hit))
        {
            // Did we hit the player?
            if (hit.transform.CompareTag("Player"))
            {
                canSeePlayer = true;
                debugStatus = "I SEE YOU!";
                Debug.DrawLine(startPos, endPos, Color.green);
                Debug.Log("I SEE YOU!");
            }
            else
            {
                debugStatus = "Blocked by: " + hit.transform.name;
                Debug.DrawLine(startPos, hit.point, Color.red);
            }
        }
        else
        {
            // Linecast didn't hit ANYTHING? 
            // This usually means the player has no collider, or we are inside the player.
            // But if we didn't hit anything, and we are aiming AT the player...
            // It implies the path is clear, but we didn't hit the player's collider.
            debugStatus = "Path clear, but no hit (Missing Collider?)";
            Debug.DrawLine(startPos, endPos, Color.yellow);
        }
    }

    void OnDrawGizmos()
    {
        // Draw the view radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        // Draw the eye position
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position + eyeOffset, 0.1f);

        // Draw View Angle Lines
        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

        Gizmos.DrawLine(transform.position + eyeOffset, transform.position + eyeOffset + viewAngleA * viewDistance);
        Gizmos.DrawLine(transform.position + eyeOffset, transform.position + eyeOffset + viewAngleB * viewDistance);
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
