using UnityEngine;
using System.Collections.Generic;

public class EnemyVision : MonoBehaviour
{
    [Header("Settings")]
    public float viewDistance = 10f;
    public float viewAngle = 90f;
    public Vector3 eyeOffset = new Vector3(0, 1.5f, 0); // Higher up to avoid ground/self

    [Header("Visualization")]
    public LayerMask obstacleMask;
    public float meshResolution = 1f; // Rays per degree
    private MeshFilter viewMeshFilter;
    private Mesh viewMesh;

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

        // Create visualization child
        GameObject viewObj = new GameObject("ViewVisualization");
        viewObj.transform.SetParent(transform);
        viewObj.transform.localPosition = Vector3.zero;
        viewObj.transform.localRotation = Quaternion.identity;

        viewMeshFilter = viewObj.AddComponent<MeshFilter>();
        MeshRenderer viewRenderer = viewObj.AddComponent<MeshRenderer>();

        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        // Create a simple transparent material
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(1, 1, 0, 0.3f); // Yellow transparent
        viewRenderer.material = mat;
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
    }

    private void LateUpdate()
    {
        DrawFieldOfView();
    }

    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = -viewAngle / 2 + stepAngleSize * i;
            // DirFromAngle handles the rotation if we say angleIsGlobal=false
            Vector3 dir = DirFromAngle(angle, false);

            // Raycast from Eye (World Space)
            Vector3 eyeWorldPos = transform.position + eyeOffset;

            RaycastHit hit;
            // Note: We use obstacleMask here so the cone stops at walls
            if (Physics.Raycast(eyeWorldPos, dir, out hit, viewDistance, obstacleMask))
            {
                viewPoints.Add(transform.InverseTransformPoint(hit.point));
            }
            else
            {
                viewPoints.Add(transform.InverseTransformPoint(eyeWorldPos + dir * viewDistance));
            }
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = eyeOffset; // Local eye position
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = viewPoints[i];

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
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
