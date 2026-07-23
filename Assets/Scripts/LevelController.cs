using UnityEngine;
using System.Collections.Generic;

public class LevelController : MonoBehaviour
{
    // -- SYSTEM -- //

    GameObject pig;

    void Start()
    {
        pig = GameObject.FindGameObjectWithTag("Pig");
        
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

    public GameObject levelChunkPrefab;
    public int chunksAhead = 10;

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
            if (spawnedChunks[i].transform.position.z < playerZ - chunkSize)
            {
                Destroy(spawnedChunks[i]);
                spawnedChunks.RemoveAt(i);
            }
        }
    }

    void SpawnChunk()
    {
        GameObject chunk = Instantiate(levelChunkPrefab, new Vector3(0, 0, nextChunkZ), Quaternion.identity);
        spawnedChunks.Add(chunk);
        nextChunkZ += chunkSize;
    }
}
