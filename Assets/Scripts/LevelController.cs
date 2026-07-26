using UnityEngine;
using System.Collections.Generic;

public class LevelController : MonoBehaviour
{
    // -- SYSTEM -- //

    GameObject pig;
    bool isFirstPlayRun;
    int firstPlayBlankChunksRemaining;

    public bool IsFirstPlayRun => isFirstPlayRun;

    void Awake()
    {
        isFirstPlayRun = GameFlowState.ConsumeShouldPlayTutorialOnGameLoad();

        firstPlayBlankChunksRemaining = isFirstPlayRun ? Mathf.Max(0, firstPlayTutorialBlankChunks) : 0;
    }

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

        GameObject[] loadedDecorations = Resources.LoadAll<GameObject>("Decorations");
        if (loadedDecorations.Length > 0)
        {
            decorationObjects = loadedDecorations;
        }
        else if (decorationObjects == null || decorationObjects.Length == 0)
        {
            Debug.LogWarning("No decorations found in Assets/Resources/Decorations/ and decorationObjects is empty.");
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

    public void DisableLevelAndPig()
    {
        if (pig != null)
        {
            pig.SetActive(false);
        }

        for (int i = 0; i < spawnedChunks.Count; i++)
        {
            GameObject chunk = spawnedChunks[i];
            if (chunk != null)
            {
                chunk.SetActive(false);
            }
        }

        enabled = false;
    }

    // -- LEVEL -- //

    public int chunksAhead = 10;
    public int firstPlayTutorialBlankChunks = 10;
    public GameObject blankLevelChunkPrefab;
    public GameObject waterChunkPrefab;
    public GameObject fireflyParticles;
    [Header("Apple Spawning")]
    [Range(0f, 100f)] public float earlyAppleSpawnChance = 10f;
    [Range(0f, 100f)] public float lateAppleSpawnChance = 50f;
    [Min(0.1f)] public float appleSpawnRampExponent = 1f;
    public GameObject applePrefab;
    public GameObject[] decorationObjects;
    public int minDecorationsToSpawn = 0;
    public int maxDecorationsToSpawn = 2;
    public GameObject edgeChunkPrefab;

    public GameObject rightTurningChunkPrefab;
    public GameObject leftTurningChunkPrefab;
    [Range(0f, 100f)] public float turnChunkChance = 20f;
    public int minChunksBetweenRightTurns = 8;
    public int blankChunksAfterRightTurn = 1;

    public int guaranteedInitialBlankChunks = 8;
    public float chunksBehindToKeep = 2f;
    [Range(0f, 1f)] public float startingBlankChunkChance = 0.9f;
    [Range(0f, 1f)] public float minimumBlankChunkChance = 0.1f;
    public float difficultyRampSpeed = 0.04f;

    GameObject[] levelChunkPrefabs;
    int chunksSpawned = 0;
    GameObject lastSpawnedMainChunkPrefab;
    int chunksSpawnedAtLastTurn = int.MinValue / 2;
    int pendingTurnDirection = 0;
    int forcedBlankChunksAfterTurnRemaining = 0;
    float currentLaneCenterX = 0f;

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
            if (spawnedChunks[i].transform.position.z < playerZ - (chunkSize * chunksBehindToKeep))
            {
                Destroy(spawnedChunks[i]);
                spawnedChunks.RemoveAt(i);
            }
        }
    }

    void SpawnChunk()
    {
        GameObject chunkToSpawn = SelectChunkPrefab();
        bool isRightTurnChunk = rightTurningChunkPrefab != null && chunkToSpawn == rightTurningChunkPrefab;
        bool isLeftTurnChunk = leftTurningChunkPrefab != null && chunkToSpawn == leftTurningChunkPrefab;
        bool isTurnChunk = isRightTurnChunk || isLeftTurnChunk;
        int turnDirection = isRightTurnChunk ? 1 : (isLeftTurnChunk ? -1 : 0);

        float spawnX = currentLaneCenterX;
        
        GameObject chunk = Instantiate(chunkToSpawn, new Vector3(spawnX, 0, nextChunkZ), Quaternion.identity);
        spawnedChunks.Add(chunk);

        if (fireflyParticles != null)
        {
            GameObject fireflies = Instantiate(fireflyParticles, chunk.transform);
            fireflies.transform.localPosition = Vector3.zero;
            fireflies.transform.localRotation = Quaternion.identity;
        }

        LevelChunk levelChunk = chunk.GetComponent<LevelChunk>();
        bool isWaterChunk = levelChunk != null && levelChunk.isWaterChunk;

        if (!isWaterChunk)
        {
            SpawnDecorations(levelChunk, chunk.transform);
        }
        
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
            float leftEdgeX = spawnX - chunkSize;
            float rightEdgeX = spawnX + chunkSize;

            if (isTurnChunk)
            {
                if (turnDirection > 0)
                {
                    rightEdgeX = spawnX + (2f * chunkSize);
                }
                else if (turnDirection < 0)
                {
                    leftEdgeX = spawnX - (2f * chunkSize);
                }
            }

            GameObject leftEdge = Instantiate(sideChunkPrefab, new Vector3(leftEdgeX, 0, nextChunkZ), Quaternion.identity);
            GameObject rightEdge = Instantiate(sideChunkPrefab, new Vector3(rightEdgeX, 0, nextChunkZ), Quaternion.Euler(0, 180f, 0));
            spawnedChunks.Add(leftEdge);
            spawnedChunks.Add(rightEdge);
        }

        // Transition into the turned lane by placing a blank connector chunk at +/-X on the same Z.
        if (isTurnChunk)
        {
            chunksSpawnedAtLastTurn = chunksSpawned;
            pendingTurnDirection = 0;
            forcedBlankChunksAfterTurnRemaining = Mathf.Max(0, blankChunksAfterRightTurn);

            if (blankLevelChunkPrefab != null)
            {
                GameObject transitionBlank = Instantiate(
                    blankLevelChunkPrefab,
                    new Vector3(spawnX + (chunkSize * turnDirection), 0, nextChunkZ),
                    Quaternion.identity
                );
                spawnedChunks.Add(transitionBlank);
            }
            else
            {
                Debug.LogWarning("Turn chunk spawned but blankLevelChunkPrefab is missing, so no transition blank was placed.");
            }

            currentLaneCenterX += chunkSize * turnDirection;
        }
        
        // Determine if apple should spawn based on reverse blank-chunk difficulty progression.
        bool shouldSpawnApple = Random.Range(0f, 100f) < GetCurrentAppleSpawnChance();
        if (levelChunk != null && shouldSpawnApple)
        {
            levelChunk.SpawnApple(applePrefab);
        }
        
        nextChunkZ += chunkSize;
        chunksSpawned++;
        lastSpawnedMainChunkPrefab = chunkToSpawn;
    }

    void SpawnDecorations(LevelChunk levelChunk, Transform parentChunk)
    {
        if (decorationObjects == null || decorationObjects.Length == 0)
            return;

        if (levelChunk == null || levelChunk.topLeftSpawnPoint == null || levelChunk.bottomRightSpawnPoint == null)
            return;

        int minSpawn = Mathf.Max(0, minDecorationsToSpawn);
        int maxSpawn = Mathf.Max(minSpawn, maxDecorationsToSpawn);
        int decorationCount = Random.Range(minSpawn, maxSpawn + 1);

        float minX = Mathf.Min(levelChunk.topLeftSpawnPoint.position.x, levelChunk.bottomRightSpawnPoint.position.x);
        float maxX = Mathf.Max(levelChunk.topLeftSpawnPoint.position.x, levelChunk.bottomRightSpawnPoint.position.x);
        float minZ = Mathf.Min(levelChunk.topLeftSpawnPoint.position.z, levelChunk.bottomRightSpawnPoint.position.z);
        float maxZ = Mathf.Max(levelChunk.topLeftSpawnPoint.position.z, levelChunk.bottomRightSpawnPoint.position.z);

        for (int i = 0; i < decorationCount; i++)
        {
            GameObject decorationToSpawn = decorationObjects[Random.Range(0, decorationObjects.Length)];
            if (decorationToSpawn == null)
                continue;

            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);
            Vector3 spawnPosition = new Vector3(randomX, 0f, randomZ);
            Instantiate(decorationToSpawn, spawnPosition, decorationToSpawn.transform.rotation, parentChunk);
        }
    }

    GameObject SelectChunkPrefab()
    {
        if (firstPlayBlankChunksRemaining > 0)
        {
            if (blankLevelChunkPrefab != null)
            {
                firstPlayBlankChunksRemaining--;
                return blankLevelChunkPrefab;
            }

            firstPlayBlankChunksRemaining = 0;
            Debug.LogWarning("First-play tutorial blank chunks requested, but blankLevelChunkPrefab is not assigned.");
        }

        if (blankLevelChunkPrefab != null && chunksSpawned < Mathf.Max(0, guaranteedInitialBlankChunks))
        {
            return blankLevelChunkPrefab;
        }

        if (forcedBlankChunksAfterTurnRemaining > 0)
        {
            if (blankLevelChunkPrefab != null)
            {
                forcedBlankChunksAfterTurnRemaining--;
                return blankLevelChunkPrefab;
            }

            forcedBlankChunksAfterTurnRemaining = 0;
        }

        int safeMinChunksBetweenRightTurns = Mathf.Max(0, minChunksBetweenRightTurns);
        bool turnOffCooldown = (chunksSpawned - chunksSpawnedAtLastTurn) > safeMinChunksBetweenRightTurns;

        if (pendingTurnDirection != 0 && turnOffCooldown)
        {
            int resolvedPendingDirection;
            GameObject pendingTurnPrefab = GetTurnPrefabForDirection(pendingTurnDirection, out resolvedPendingDirection);
            if (pendingTurnPrefab != null)
            {
                pendingTurnDirection = resolvedPendingDirection;
                return pendingTurnPrefab;
            }

            pendingTurnDirection = 0;
        }

        if (pendingTurnDirection != 0 && !turnOffCooldown)
        {
            pendingTurnDirection = 0;
        }

        bool canSpawnTurn = HasAnyTurnPrefab()
            && turnOffCooldown
            && Random.Range(0f, 100f) < turnChunkChance;
        if (canSpawnTurn)
        {
            int selectedTurnDirection;
            GameObject selectedTurnPrefab;
            bool hasChosenTurn = TryChooseRandomTurn(out selectedTurnDirection, out selectedTurnPrefab);
            if (!hasChosenTurn)
            {
                pendingTurnDirection = 0;
            }
            else
            {
                bool previousChunkWasBlank = lastSpawnedMainChunkPrefab == blankLevelChunkPrefab;
                if (!previousChunkWasBlank && blankLevelChunkPrefab != null)
                {
                    pendingTurnDirection = selectedTurnDirection;
                    return blankLevelChunkPrefab;
                }

                return selectedTurnPrefab;
            }
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

    bool HasAnyTurnPrefab()
    {
        return rightTurningChunkPrefab != null || leftTurningChunkPrefab != null;
    }

    GameObject GetTurnPrefabForDirection(int preferredDirection, out int resolvedDirection)
    {
        resolvedDirection = 0;

        if (preferredDirection >= 0 && rightTurningChunkPrefab != null)
        {
            resolvedDirection = 1;
            return rightTurningChunkPrefab;
        }

        if (preferredDirection <= 0 && leftTurningChunkPrefab != null)
        {
            resolvedDirection = -1;
            return leftTurningChunkPrefab;
        }

        if (rightTurningChunkPrefab != null)
        {
            resolvedDirection = 1;
            return rightTurningChunkPrefab;
        }

        if (leftTurningChunkPrefab != null)
        {
            resolvedDirection = -1;
            return leftTurningChunkPrefab;
        }

        return null;
    }

    bool TryChooseRandomTurn(out int direction, out GameObject turnPrefab)
    {
        direction = 0;
        turnPrefab = null;

        bool hasRight = rightTurningChunkPrefab != null;
        bool hasLeft = leftTurningChunkPrefab != null;

        if (hasRight && hasLeft)
        {
            direction = Random.value < 0.5f ? -1 : 1;
            turnPrefab = direction > 0 ? rightTurningChunkPrefab : leftTurningChunkPrefab;
            return true;
        }

        if (hasRight)
        {
            direction = 1;
            turnPrefab = rightTurningChunkPrefab;
            return true;
        }

        if (hasLeft)
        {
            direction = -1;
            turnPrefab = leftTurningChunkPrefab;
            return true;
        }

        return false;
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

    float GetCurrentAppleSpawnChance()
    {
        float safeEarlyChance = Mathf.Clamp(earlyAppleSpawnChance, 0f, 100f);
        float safeLateChance = Mathf.Clamp(lateAppleSpawnChance, 0f, 100f);

        float safeMinBlankChance = Mathf.Clamp01(minimumBlankChunkChance);
        float safeStartBlankChance = Mathf.Clamp01(startingBlankChunkChance);
        if (safeStartBlankChance < safeMinBlankChance)
        {
            safeStartBlankChance = safeMinBlankChance;
        }

        float currentBlankChance = GetCurrentBlankChance();
        float blankChanceRange = safeStartBlankChance - safeMinBlankChance;
        float reverseProgress = blankChanceRange <= Mathf.Epsilon
            ? Mathf.Clamp01(chunksSpawned * Mathf.Max(0f, difficultyRampSpeed))
            : Mathf.Clamp01((safeStartBlankChance - currentBlankChance) / blankChanceRange);

        float shapedProgress = Mathf.Pow(reverseProgress, Mathf.Max(0.1f, appleSpawnRampExponent));
        return Mathf.Lerp(safeEarlyChance, safeLateChance, shapedProgress);
    }
}
