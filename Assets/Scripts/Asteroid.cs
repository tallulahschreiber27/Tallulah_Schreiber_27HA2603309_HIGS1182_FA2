using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Base speed at which the asteroid drifts down the screen.")]
    [SerializeField] private float driftSpeed = 5f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;

            float randomSpeed = Random.Range(driftSpeed * 0.7f, driftSpeed * 1.3f);

            float randomHorizontalDrift = Random.Range(-1.5f, 1.5f);

            Vector3 uniqueDirection = new Vector3(randomHorizontalDrift, 0f, -1f).normalized;
            rb.linearVelocity = uniqueDirection * randomSpeed; // Unity 6 production standard

            float randomSpin = Random.Range(-50f, 50f);
            rb.angularVelocity = new Vector3(0f, randomSpin, 0f);
        }
        else
        {
            Debug.LogError($"Asteroid Error: Rigidbody component is missing on {gameObject.name}!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered the trigger has the tag "Player"
        if (other.CompareTag("Player"))
        {
            Debug.Log(" 3D Asteroid hit the Player!");

            // TODO: Call the GameManager to load the Game Over screen here!

            Destroy(other.gameObject); // Wipe out the player drone
            Destroy(gameObject);       // Destroy this asteroid on impact
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Clean up when the asteroid safely exits your large screen boundary box
        if (other.CompareTag("GameplayBoundary"))
        {
            Debug.Log($"[Cleanup] '{gameObject.name}' left the arena and cleanly self-destructed.");
            Destroy(gameObject);
        }
    }
}
