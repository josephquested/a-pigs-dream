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

    GameObject[] levelChunkPrefabs;
    int chunksSpawned = 0;

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
        }
        
        GameObject chunk = Instantiate(chunkToSpawn, new Vector3(0, 0, nextChunkZ), Quaternion.identity);
        spawnedChunks.Add(chunk);
        nextChunkZ += chunkSize;
        chunksSpawned++;
    }
}
