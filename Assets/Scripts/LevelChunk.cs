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

    public void SpawnObjects()
    {
        int numberOfObjectsToSpawn = Random.Range(1, 4); // Randomly spawn 1 to 3 objects

        for (int i = 0; i < numberOfObjectsToSpawn; i++)
        {
            // Randomly select an object to spawn
            GameObject objectToSpawn = spawnableObjects[Random.Range(0, spawnableObjects.Length)];

            // Randomly determine a position within the chunk's bounds
            float randomX = Random.Range(topLeftSpawnPoint.position.x, bottomRightSpawnPoint.position.x);
            float randomZ = Random.Range(topLeftSpawnPoint.position.z, bottomRightSpawnPoint.position.z);
            Vector3 spawnPosition = new Vector3(randomX, 0f, randomZ);

            // Instantiate the object at the determined position
            Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
        }
    }
}
