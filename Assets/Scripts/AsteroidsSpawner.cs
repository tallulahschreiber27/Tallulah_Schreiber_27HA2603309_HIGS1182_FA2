using System.Collections.Generic;
using UnityEngine;

public class AsteroidsSpawner : MonoBehaviour
{
    [Header("Prefab Assignment")]
    [SerializeField] private GameObject asteroidPrefab;

    [Header("Spawning Count")]
    [Tooltip("The exact number of asteroids that should always be present in the game.")]
    [SerializeField] private int maxAsteroidsCount = 15;

    [Header("Spawning Boundaries")]
    [Tooltip("The minimum and maximum X coordinates taken from your inspector bounds.")]
    [SerializeField] private Vector2 xSpawnRange = new Vector2(-198f, 173f);
    [Tooltip("The minimum and maximum Z coordinates taken from your inspector bounds.")]
    [SerializeField] private Vector2 zSpawnRange = new Vector2(-157f, 168f);

    [Header("Player Safety Buffer")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float safeDistanceZone = 10f; 

    [Header("Scatter Settings")]
    [SerializeField] private Vector2 speedRange = new Vector2(5f, 15f); 
    [SerializeField] private float maxSidewaysScatter = 5f;

    private List<GameObject> activeAsteroids = new List<GameObject>();

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        MaintainAsteroidPopulation();
    }

    private void Update()
    {
        //Monitors the list for missing references (destroyed by the box collider physical boundary or player laser)
        MonitorActiveAsteroids();

        //Replenish missing items to keep a steady flow
        MaintainAsteroidPopulation();
    }

    private void MonitorActiveAsteroids()
    {
        for (int i = activeAsteroids.Count - 1; i >= 0; i--)
        {
            // If the asteroid object is null, it was successfully destroyed by the physical box collider boundary or weapon
            if (activeAsteroids[i] == null)
            {
                activeAsteroids.RemoveAt(i);
            }
        }
    }

    private void MaintainAsteroidPopulation()
    {
        while (activeAsteroids.Count < maxAsteroidsCount)
        {
            SpawnSingleAsteroid();
        }
    }

    public void SpawnSingleAsteroid()
    {
        Vector3 spawnPosition = Vector3.zero;
        bool validPositionFound = false;
        int attempts = 0;
        int maxAttempts = 20;

        while (!validPositionFound && attempts < maxAttempts)
        {
            float randomX = Random.Range(xSpawnRange.x, xSpawnRange.y);
            float randomZ = Random.Range(zSpawnRange.x, zSpawnRange.y);
            spawnPosition = new Vector3(randomX, 0f, randomZ);

            if (playerTransform != null)
            {
                float distanceToPlayer = Vector3.Distance(spawnPosition, playerTransform.position);
                if (distanceToPlayer >= safeDistanceZone)
                {
                    validPositionFound = true;
                }
            }
            else
            {
                validPositionFound = true;
            }

            attempts++;
        }

        GameObject spawnedAsteroid = Instantiate(asteroidPrefab, spawnPosition, Quaternion.identity);
        activeAsteroids.Add(spawnedAsteroid);

        Rigidbody asteroidRb = spawnedAsteroid.GetComponent<Rigidbody>();
        if (asteroidRb != null)
        {
            float randomSpeed = Random.Range(speedRange.x, speedRange.y);
            float randomHorizontalScatter = Random.Range(-maxSidewaysScatter, maxSidewaysScatter);

            float directionModifier = Random.value > 0.5f ? 1f : -1f;
            asteroidRb.linearVelocity = new Vector3(randomHorizontalScatter, 0f, randomSpeed * directionModifier);
        }
    }
}
