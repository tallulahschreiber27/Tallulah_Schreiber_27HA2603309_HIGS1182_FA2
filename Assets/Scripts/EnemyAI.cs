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

    [Header("Rigid Patrol Boundaries")]
    [Tooltip("The left (X min) and right (X max) turning points.")]
    [SerializeField] private Vector2 xPatrolRange = new Vector2(-198f, 173f);

    [Header("Infinite Respawn Settings")]
    [Tooltip("Drag your Enemy Drone prefab asset here so it can replace itself when destroyed!")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnZCoordinate = 150f;

    private bool isChasingPlayer = false;
    private bool movingRight = true;
    private bool isQuitting = false; // Failsafe to stop spawns when turning off the game
    private Rigidbody rb;

    private void Start()
    {
        // Section B requirement: Cached runtime component access references safely
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false; // Space environment configuration
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }

        // Auto-locate player target via tags if unassigned
        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTarget = playerObj.transform;
        }
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
            RigidPatrolLoop();
            return;
        }

        // Calculate straight line distance to player drone
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // State Machine Switch: Move straight down if player detected, otherwise pace left/right
        if (distanceToPlayer <= detectionRange)
        {
            if (!isChasingPlayer)
            {
                isChasingPlayer = true;
                Debug.Log("[AI Engine] Player spotted! Charging straight down.");
            }
            RigidDownwardsChaseLoop();
        }
        else
        {
            if (isChasingPlayer)
            {
                isChasingPlayer = false;
                Debug.Log("[AI Engine] Threat lost. Resuming horizontal border pacing.");
            }
            RigidPatrolLoop();
        }
    }

    /// <summary>
    /// Forces the drone to only move perfectly left and right along the X-axis
    /// </summary>
    private void RigidPatrolLoop()
    {
        Vector3 moveDirection = Vector3.zero;

        // Toggle direction handling based on boundary checkpoints
        if (movingRight)
        {
            moveDirection = Vector3.right;
            if (transform.position.x >= xPatrolRange.y)
            {
                movingRight = false;
            }
        }
        else
        {
            moveDirection = Vector3.left;
            if (transform.position.x <= xPatrolRange.x)
            {
                movingRight = true;
            }
        }

        if (rb != null)
        {
            rb.linearVelocity = moveDirection * patrolSpeed;
        }
    }

    /// <summary>
    /// Drops all side-to-side movement and rushes straight down the map matrix (negative Z-axis)
    /// </summary>
    /// <summary>
    /// Moves left and right to track the player's horizontal alignment while continuously pushing down (Z-axis).
    /// </summary>
    private void RigidDownwardsChaseLoop()
    {
        float xDirection = 0f;

        // Check if the player is to the right or left of the enemy drone (with a small 1-unit deadzone)
        if (playerTarget.position.x > transform.position.x + 1f)
        {
            xDirection = 1f; // Move Right
        }
        else if (playerTarget.position.x < transform.position.x - 1f)
        {
            xDirection = -1f; // Move Left
        }

        // Combine horizontal tracking with constant downward movement (Vector3.back is -Z)
        Vector3 moveDirection = new Vector3(xDirection, 0f, -1f).normalized;

        if (rb != null)
        {
            rb.linearVelocity = moveDirection * chaseSpeed;
        }

        // Keep the drone facing its movement velocity vector for crisp visual alignment
        if (moveDirection != Vector3.zero)
        {
            transform.forward = moveDirection;
        }
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

    /// <summary>
    /// INFINITE RESPAWN LOGIC: Triggered automatically the exact millisecond the player destroys this ship.
    /// </summary>
    private void OnDestroy()
    {
        // Failsafe checks: Don't spawn duplicates if exiting playmode or loading scenes
        if (isQuitting || !Application.isPlaying) return;

        if (enemyPrefab != null)
        {
            // Calculate a fresh random starting X coordinate at the top edge of your bounds
            float randomX = Random.Range(xPatrolRange.x, xPatrolRange.y);
            Vector3 respawnPosition = new Vector3(randomX, transform.position.y, spawnZCoordinate);

            // Instantiate a brand new duplicate back into the arena pool!
            GameObject newClone = Instantiate(enemyPrefab, respawnPosition, Quaternion.identity);

            // If this ship was already accelerated by a difficulty spike, pass the buff onto the new clone
            if (patrolSpeed > 4f)
            {
                EnemyAI newAI = newClone.GetComponent<EnemyAI>();
                if (newAI != null)
                {
                    newAI.patrolSpeed = this.patrolSpeed;
                    newAI.chaseSpeed = this.chaseSpeed;
                }
            }

            Debug.Log("[AI Engine] Hostile drone destroyed. Automated factory dispatched a replacement ship down the map layout.");
        }
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }
}
