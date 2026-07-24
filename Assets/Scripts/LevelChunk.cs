using UnityEngine;

public class LevelChunk : MonoBehaviour
{
    // -- SYSTEM -- //

    void Start()
    {
        SpawnObjects();
    }

    // -- OBJECT SPAWNING -- //

    public Transform topLeftSpawnPoint;
    public Transform bottomRightSpawnPoint;
    public GameObject[] spawnableObjects;
    public int minObjectsToSpawn = 0;
    public int maxObjectsToSpawn = 2;
    public bool randomizeYRotation = true;
    public float spawnYOffset = 0f;

    public void SpawnObjects()
    {
        if (spawnableObjects.Length == 0)
            return;

        int minSpawn = Mathf.Max(0, minObjectsToSpawn);
        int maxSpawn = Mathf.Max(minSpawn, maxObjectsToSpawn);
        int numberOfObjectsToSpawn = Random.Range(minSpawn, maxSpawn + 1);

        for (int i = 0; i < numberOfObjectsToSpawn; i++)
        {
            // Randomly select an object to spawn
            GameObject objectToSpawn = spawnableObjects[Random.Range(0, spawnableObjects.Length)];

            // Randomly determine a position within the chunk's bounds
            float randomX = Random.Range(topLeftSpawnPoint.position.x, bottomRightSpawnPoint.position.x);
            float randomZ = Random.Range(topLeftSpawnPoint.position.z, bottomRightSpawnPoint.position.z);
            Vector3 spawnPosition = new Vector3(randomX, spawnYOffset, randomZ);
            Vector3 prefabEuler = objectToSpawn.transform.rotation.eulerAngles;
            float yRotation = randomizeYRotation ? Random.Range(0f, 360f) : prefabEuler.y;
            Quaternion spawnRotation = Quaternion.Euler(prefabEuler.x, yRotation, prefabEuler.z);

            // Parent spawned objects to this chunk so cleanup happens with the chunk.
            Instantiate(objectToSpawn, spawnPosition, spawnRotation, transform);
        }
    }

    public void SpawnApple(GameObject applePrefab)
    {
        if (applePrefab == null)
            return;

        // Randomly determine a position within the chunk's bounds
        float randomX = Random.Range(topLeftSpawnPoint.position.x, bottomRightSpawnPoint.position.x);
        float randomZ = Random.Range(topLeftSpawnPoint.position.z, bottomRightSpawnPoint.position.z);
        Vector3 spawnPosition = new Vector3(randomX, spawnYOffset, randomZ);
        Vector3 prefabEuler = applePrefab.transform.rotation.eulerAngles;
        float yRotation = randomizeYRotation ? Random.Range(0f, 360f) : prefabEuler.y;
        Quaternion spawnRotation = Quaternion.Euler(prefabEuler.x, yRotation, prefabEuler.z);

        // Parent spawned apples to this chunk so cleanup happens with the chunk.
        Instantiate(applePrefab, spawnPosition, spawnRotation, transform);
    }
}
