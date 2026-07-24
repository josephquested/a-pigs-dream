using UnityEngine;
using System.Collections.Generic;

public class LevelController : MonoBehaviour
{
    // -- SYSTEM -- //

    GameObject pig;

    void Start()
    {
        pig = GameObject.FindGameObjectWithTag("Pig");
        
        // Load all level chunk prefabs from Resources folder
        levelChunkPrefabs = Resources.LoadAll<GameObject>("LevelChunks");
        if (levelChunkPrefabs.Length == 0)
        {
            Debug.LogError("No level chunks found in Assets/Resources/LevelChunks/");
            return;
        }
        
        // Spawn initial chunks
        for (int i = 0; i < chunksAhead; i++)
        {
            SpawnChunk();
        }
    }

    void Update()
    {
        UpdateLevelGeneration();
    }

    // -- LEVEL -- //

    public int chunksAhead = 10;
    public GameObject blankLevelChunkPrefab;
    public float appleSpawnChance = 50f;
    public GameObject applePrefab;
    public GameObject edgeChunkPrefab; 

    GameObject[] levelChunkPrefabs;
    int chunksSpawned = 0;
    GameObject lastSpawnedMainChunkPrefab;

    List<GameObject> spawnedChunks = new List<GameObject>();
    float nextChunkZ = 0f;
    float chunkSize = 5f;

    void UpdateLevelGeneration()
    {
        float playerZ = pig.transform.position.z;
        
        // Spawn new chunks ahead of the player
        while (nextChunkZ < playerZ + (chunksAhead * chunkSize))
        {
            SpawnChunk();
        }
        
        // Delete chunks behind the player
        for (int i = spawnedChunks.Count - 1; i >= 0; i--)
        {
            if (spawnedChunks[i].transform.position.z < playerZ - (chunkSize * 2))
            {
                Destroy(spawnedChunks[i]);
                spawnedChunks.RemoveAt(i);
            }
        }
    }

    void SpawnChunk()
    {
        GameObject chunkToSpawn;
        
        // Use blank chunk for first 3, then random
        if (chunksSpawned < 3)
        {
            chunkToSpawn = blankLevelChunkPrefab;
        }
        else
        {
            chunkToSpawn = levelChunkPrefabs[Random.Range(0, levelChunkPrefabs.Length)];

            // Avoid repeating the same non-blank chunk twice in a row.
            if (lastSpawnedMainChunkPrefab != null
                && lastSpawnedMainChunkPrefab != blankLevelChunkPrefab
                && chunkToSpawn == lastSpawnedMainChunkPrefab)
            {
                List<GameObject> eligibleChunks = new List<GameObject>();
                foreach (GameObject prefab in levelChunkPrefabs)
                {
                    if (prefab != lastSpawnedMainChunkPrefab || prefab == blankLevelChunkPrefab)
                    {
                        eligibleChunks.Add(prefab);
                    }
                }

                if (eligibleChunks.Count > 0)
                {
                    chunkToSpawn = eligibleChunks[Random.Range(0, eligibleChunks.Count)];
                }
            }
        }
        
        GameObject chunk = Instantiate(chunkToSpawn, new Vector3(0, 0, nextChunkZ), Quaternion.identity);
        spawnedChunks.Add(chunk);
        
        // Spawn edge chunks on left and right
        if (edgeChunkPrefab != null)
        {
            GameObject leftEdge = Instantiate(edgeChunkPrefab, new Vector3(-10f, 0, nextChunkZ), Quaternion.identity);
            GameObject rightEdge = Instantiate(edgeChunkPrefab, new Vector3(10f, 0, nextChunkZ), Quaternion.Euler(0, 180f, 0));
            spawnedChunks.Add(leftEdge);
            spawnedChunks.Add(rightEdge);
        }
        
        // Determine if apple should spawn based on chance
        bool shouldSpawnApple = Random.Range(0f, 100f) < appleSpawnChance;
        LevelChunk levelChunk = chunk.GetComponent<LevelChunk>();
        if (levelChunk != null && shouldSpawnApple)
        {
            levelChunk.SpawnApple(applePrefab);
        }
        
        nextChunkZ += chunkSize;
        chunksSpawned++;
        lastSpawnedMainChunkPrefab = chunkToSpawn;
    }
}
