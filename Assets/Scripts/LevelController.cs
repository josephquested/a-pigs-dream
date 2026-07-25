using UnityEngine;
using System.Collections.Generic;
using TMPro;

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
        UpdateBlankChanceDisplay();
    }

    // -- LEVEL -- //

    public int chunksAhead = 10;
    public GameObject blankLevelChunkPrefab;
    public GameObject waterChunkPrefab;
    public float appleSpawnChance = 50f;
    public GameObject applePrefab;
    public GameObject edgeChunkPrefab;
    public TextMeshProUGUI blankChunkChanceText;
    public int guaranteedInitialBlankChunks = 8;
    [Range(0f, 1f)] public float startingBlankChunkChance = 0.9f;
    [Range(0f, 1f)] public float minimumBlankChunkChance = 0.1f;
    public float difficultyRampSpeed = 0.04f;

    GameObject[] levelChunkPrefabs;
    int chunksSpawned = 0;
    GameObject lastSpawnedMainChunkPrefab;

    List<GameObject> spawnedChunks = new List<GameObject>();
    float nextChunkZ = 0f;
    float chunkSize = 10f;

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
        GameObject chunkToSpawn = SelectChunkPrefab();
        
        GameObject chunk = Instantiate(chunkToSpawn, new Vector3(0, 0, nextChunkZ), Quaternion.identity);
        spawnedChunks.Add(chunk);

        LevelChunk levelChunk = chunk.GetComponent<LevelChunk>();
        bool isWaterChunk = levelChunk != null && levelChunk.isWaterChunk;
        
        // Spawn side chunks on left and right.
        // Water chunks: 30% water edges, 70% normal edges.
        bool useWaterEdges = isWaterChunk && Random.value < 0.3f;
        GameObject sideChunkPrefab = useWaterEdges ? waterChunkPrefab : edgeChunkPrefab;
        if (sideChunkPrefab == null)
        {
            sideChunkPrefab = useWaterEdges ? edgeChunkPrefab : waterChunkPrefab;
        }

        if (sideChunkPrefab != null)
        {
            GameObject leftEdge = Instantiate(sideChunkPrefab, new Vector3(-10f, 0, nextChunkZ), Quaternion.identity);
            GameObject rightEdge = Instantiate(sideChunkPrefab, new Vector3(10f, 0, nextChunkZ), Quaternion.Euler(0, 180f, 0));
            spawnedChunks.Add(leftEdge);
            spawnedChunks.Add(rightEdge);
        }
        
        // Determine if apple should spawn based on chance
        bool shouldSpawnApple = Random.Range(0f, 100f) < appleSpawnChance;
        if (levelChunk != null && shouldSpawnApple)
        {
            levelChunk.SpawnApple(applePrefab);
        }
        
        nextChunkZ += chunkSize;
        chunksSpawned++;
        lastSpawnedMainChunkPrefab = chunkToSpawn;
    }

    GameObject SelectChunkPrefab()
    {
        if (blankLevelChunkPrefab != null && chunksSpawned < Mathf.Max(0, guaranteedInitialBlankChunks))
        {
            return blankLevelChunkPrefab;
        }

        float currentBlankChance = GetCurrentBlankChance();
        bool shouldUseBlank = blankLevelChunkPrefab != null && Random.value < currentBlankChance;

        if (shouldUseBlank)
        {
            return blankLevelChunkPrefab;
        }

        List<GameObject> candidateChunks = new List<GameObject>();
        foreach (GameObject prefab in levelChunkPrefabs)
        {
            if (prefab == null || prefab == blankLevelChunkPrefab)
                continue;

            // Avoid repeating the same non-blank chunk twice in a row.
            if (lastSpawnedMainChunkPrefab != null
                && lastSpawnedMainChunkPrefab != blankLevelChunkPrefab
                && prefab == lastSpawnedMainChunkPrefab)
            {
                continue;
            }

            candidateChunks.Add(prefab);
        }

        if (candidateChunks.Count == 0)
        {
            return blankLevelChunkPrefab != null ? blankLevelChunkPrefab : levelChunkPrefabs[Random.Range(0, levelChunkPrefabs.Length)];
        }

        return candidateChunks[Random.Range(0, candidateChunks.Count)];
    }

    float GetCurrentBlankChance()
    {
        float safeMinBlankChance = Mathf.Clamp01(minimumBlankChunkChance);
        float safeStartBlankChance = Mathf.Clamp01(startingBlankChunkChance);
        if (safeStartBlankChance < safeMinBlankChance)
        {
            safeStartBlankChance = safeMinBlankChance;
        }

        float progression = Mathf.Clamp01(chunksSpawned * Mathf.Max(0f, difficultyRampSpeed));
        return Mathf.Lerp(safeStartBlankChance, safeMinBlankChance, progression);
    }

    void UpdateBlankChanceDisplay()
    {
        if (blankChunkChanceText == null)
            return;

        float chancePercent = GetCurrentBlankChance() * 100f;
        blankChunkChanceText.text = "Blank Chunk Chance: " + chancePercent.ToString("F1") + "%";
    }
}
