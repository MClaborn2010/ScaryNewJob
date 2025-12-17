using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RandomRoam : MonoBehaviour
{
    [Header("Roaming Settings")]
    [Tooltip("How far the agent can roam from its current position.")]
    public float roamRadius = 10f;

    [Tooltip("How long to wait at each destination before moving again.")]
    public float waitTime = 1f;

    private NavMeshAgent _agent;
    private float _timer;
    private EnemyVision _vision;
    private Transform _player;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _vision = GetComponent<EnemyVision>();
    }

    private void Start()
    {
        // Start moving immediately
        _timer = waitTime;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;
    }

    private void Update()
    {
        // Safety check: Ensure agent is active and on the NavMesh
        if (!_agent.isOnNavMesh || !_agent.isActiveAndEnabled) return;

        // 1. Check for Vision (Chase Behavior)
        if (_vision != null && _vision.canSeePlayer && _player != null)
        {
            _agent.SetDestination(_player.position);
            _timer = 0; // Reset wait timer so we don't pause if we lose sight
            return; // Skip roaming logic
        }

        // 2. Roaming Behavior
        // Check if we've reached the destination
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            _timer += Time.deltaTime;

            if (_timer >= waitTime)
            {
                Vector3 newPos = RandomNavSphere(transform.position, roamRadius, -1);
                _agent.SetDestination(newPos);
                _timer = 0;
            }
        }
    }

    // Helper function to find a random point on the NavMesh
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;

        NavMeshHit navHit;

        // SamplePosition checks for the nearest point on the NavMesh within the range
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }
}
