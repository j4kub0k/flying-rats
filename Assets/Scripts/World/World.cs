using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{

    public HUD HUD;

    Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
    Dictionary<Vector3Int, ChunkGenerator> chunkRenderers = new Dictionary<Vector3Int, ChunkGenerator>();
    int seed;


    public Material Material;
    public GameObject playerPrefab;   
    Transform player;                 
    Vector3Int lastPlayerChunk = new Vector3Int(int.MaxValue, 0, int.MaxValue);


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        seed = SaveWorld.GetSeed();
        WorldGenerator.Initialize(seed);
        SpawnPlayer();
        UpdateChunks(lastPlayerChunk);
    }

    void Update()
    {
        Vector3Int playerChunk = WorldSettings.WorldToChunkCoord(player.position);
        playerChunk.y = 0;   

        if (playerChunk == lastPlayerChunk) return;
        lastPlayerChunk = playerChunk;
        UpdateChunks(playerChunk);
    }

    void SpawnPlayer()
    {
        int x = Random.Range(-1000, 1000);
        int z = Random.Range(-1000, 1000);

        float h = WorldGenerator.GetHeight(x, z);
        Vector3 spawnPos = new Vector3(x + 0.5f, Mathf.FloorToInt(h) + 2f, z + 0.5f);

        GameObject playerGO = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        player = playerGO.transform;
        playerGO.GetComponent<PlayerController>().world = this;
        HUD.playerController = playerGO.GetComponent<PlayerController>();

        lastPlayerChunk = WorldSettings.WorldToChunkCoord(spawnPos);
        lastPlayerChunk.y = 0;
    }
    void CreateChunk(Vector3Int chunkCoord)
    {


        if (!chunks.TryGetValue(chunkCoord, out Chunk chunk))
        {
            if (!SaveWorld.TryLoadChunk(chunkCoord, out chunk))
            {
                chunk = new Chunk(chunkCoord);
                WorldGenerator.GenerateChunk(chunk);
            }
            chunks[chunkCoord] = chunk;
        }

        GameObject chunkObject = new GameObject($"Chunk {chunkCoord.x},{chunkCoord.z}");
        chunkObject.transform.parent = this.transform;
        chunkObject.transform.position = new Vector3(
            chunkCoord.x * WorldSettings.ChunkWidth,
            chunkCoord.y * WorldSettings.ChunkHeight,
            chunkCoord.z * WorldSettings.ChunkWidth
        );
        ChunkGenerator renderer = chunkObject.AddComponent<ChunkGenerator>();
        renderer.GenerateMesh(chunk, Material);
        chunkRenderers[chunkCoord] = renderer;
    }

    public Chunk GetChunk(Vector3Int chunkCoord)
    {
        if (chunks.TryGetValue(chunkCoord, out Chunk chunk))
        {
            return chunk;
        }
        return null;
    }

    public void RebuildChunk(Vector3Int chunkCoord)
    {
        if (chunks.TryGetValue(chunkCoord, out Chunk chunk) &&
            chunkRenderers.TryGetValue(chunkCoord, out ChunkGenerator renderer))
        {
            renderer.GenerateMesh(chunk, Material);
        }
    }

    void UpdateChunks(Vector3Int playerChunk)
    {
        for (int x = -WorldSettings.ViewDistance; x <= WorldSettings.ViewDistance; x++)
            for (int z = -WorldSettings.ViewDistance; z <= WorldSettings.ViewDistance; z++)
            {
                Vector3Int coord = new Vector3Int(playerChunk.x + x, 0, playerChunk.z + z);
                if (!chunkRenderers.ContainsKey(coord))
                    CreateChunk(coord);
            }

       
        List<Vector3Int> toRemove = new List<Vector3Int>();
        foreach (var coord in chunkRenderers.Keys)
        {
            if (Mathf.Abs(coord.x - playerChunk.x) > WorldSettings.ViewDistance + 1 ||
                Mathf.Abs(coord.z - playerChunk.z) > WorldSettings.ViewDistance + 1)
                toRemove.Add(coord);
        }
        foreach (var coord in toRemove)
        {
            Destroy(chunkRenderers[coord].gameObject);
            chunkRenderers.Remove(coord);
   
        }
    }

    public void SaveWorldData()
    {
        SaveWorld.Save(seed, chunks);
    }


}