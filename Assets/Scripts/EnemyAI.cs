using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Configuration")]
    [SerializeField] private float patrolSpeed = 4f;
    [SerializeField] private float chaseSpeed = 8f;

    [Header("Detection Settings")]
    [SerializeField] private Transform playerTarget;
    [Tooltip("The range at which the enemy drone can spot your drone.")]
    [SerializeField] private float detectionRange = 25f;

    [Header("Patrol Boundaries")]
    [Tooltip("Keep these limits matching your Spawner coordinates so they patrol inside the field.")]
    [SerializeField] private Vector2 xPatrolRange = new Vector2(-198f, 173f);
    [SerializeField] private Vector2 zPatrolRange = new Vector2(-157f, 168f);
    [SerializeField] private float waypointTolerance = 2f;

    private Vector3 currentWaypoint;
    private bool isChasingPlayer = false;
    private Rigidbody rb;

    private void Start()
    {
        // Section B requirement: Cached runtime component access references safely
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false; // Space environment configuration
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        // Auto-locate player target via tags if unassigned
        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTarget = playerObj.transform;
        }

        // Set initial random patrol point
        SetNewRandomWaypoint();
    }

    private void FixedUpdate()
    {
        HandleAIBehaviourStates();
    }

    /// <summary>
    /// Custom method separating state switching logic handling (Section B modular requirement)
    /// </summary>
    private void HandleAIBehaviourStates()
    {
        if (playerTarget == null)
        {
            PatrolMovementLoop();
            return;
        }

        // Calculate straight-line distance to player drone
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // State Machine Switch: Chase if inside radius, otherwise patrol
        if (distanceToPlayer <= detectionRange)
        {
            if (!isChasingPlayer)
            {
                isChasingPlayer = true;
                // Section B requirement: Trace state transitions to console
                Debug.Log("[AI Engine] Hostile Scavenger Drone Spotted Player! Shifting to Chase State.");
            }
            ChasePlayerLoop();
        }
        else
        {
            if (isChasingPlayer)
            {
                isChasingPlayer = false;
                Debug.Log("[AI Engine] Player lost in asteroid cloud. Reverting to Patrol Route.");
                SetNewRandomWaypoint();
            }
            PatrolMovementLoop();
        }
    }

    private void PatrolMovementLoop()
    {
        // Calculate move direction vector on flat space plane
        Vector3 moveDirection = (currentWaypoint - transform.position).normalized;
        moveDirection.y = 0f;

        // Move the enemy physics body smoothly over fixed frame steps
        if (rb != null)
        {
            rb.linearVelocity = moveDirection * patrolSpeed;
        }

        // Rotate to look toward waypoint smoothly
        if (moveDirection != Vector3.zero)
        {
            transform.forward = moveDirection;
        }

        // Check if waypoint reached to find another coordinate
        if (Vector3.Distance(transform.position, currentWaypoint) <= waypointTolerance)
        {
            SetNewRandomWaypoint();
        }
    }

    private void ChasePlayerLoop()
    {
        Vector3 moveDirection = (playerTarget.position - transform.position).normalized;
        moveDirection.y = 0f;

        if (rb != null)
        {
            rb.linearVelocity = moveDirection * chaseSpeed;
        }

        if (moveDirection != Vector3.zero)
        {
            transform.forward = moveDirection;
        }
    }

    private void SetNewRandomWaypoint()
    {
        float randomX = Random.Range(xPatrolRange.x, xPatrolRange.y);
        float randomZ = Random.Range(zPatrolRange.x, zPatrolRange.y);
        currentWaypoint = new Vector3(randomX, transform.position.y, randomZ);
    }

    /// <summary>
    /// Section A / B Collision Check Requirement.
    /// Triggers Game Over instantly if the enemy slams into the player drone.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("[Combat] Collision Event: Enemy drone compromised player ship hull structure!");

            // Calls the global singleton instance from your GameManager script safely!
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }
    }

    /// <summary>
    /// Custom method for Section A Difficulty Balancing. Allows ramping speed values over time.
    /// </summary>
    public void IncreaseAISpeed(float factor)
    {
        patrolSpeed *= factor;
        chaseSpeed *= factor;
        Debug.Log($"[AI Engine] Enemy systems overclocked. New Chase Speed: {chaseSpeed}");
    }

    // Visual anchor overlay: Draws detection circle directly in your Unity editor view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, currentWaypoint);
    }
}
