using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{

    Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
    Dictionary<Vector3Int, ChunkGenerator> chunkRenderers = new Dictionary<Vector3Int, ChunkGenerator>();


    public Material Material;
    public Transform player;
    Vector3Int lastPlayerChunk = new Vector3Int(int.MaxValue, 0, int.MaxValue);


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        WorldGenerator.Initialize(Random.Range(int.MinValue, int.MaxValue));
        lastPlayerChunk = WorldSettings.WorldToChunkCoord(player.position);
        lastPlayerChunk.y = 0;
        UpdateChunks(lastPlayerChunk);
    }

    void Update()
    {
        Vector3Int playerChunk = WorldSettings.WorldToChunkCoord(player.position);
        playerChunk.y = 0;   // svet je 2D mrieûka chunkov, v˝öku ignoruj

        if (playerChunk == lastPlayerChunk) return;
        lastPlayerChunk = playerChunk;
        UpdateChunks(playerChunk);
    }


    void CreateChunk(Vector3Int chunkCoord)
    {

        if (!chunks.TryGetValue(chunkCoord, out Chunk chunk))
        {
            chunk = new Chunk(chunkCoord);
            WorldGenerator.GenerateChunk(chunk);
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
        // spawn: vöetko v dosahu, Ëo nem· renderer
        for (int x = -WorldSettings.ViewDistance; x <= WorldSettings.ViewDistance; x++)
            for (int z = -WorldSettings.ViewDistance; z <= WorldSettings.ViewDistance; z++)
            {
                Vector3Int coord = new Vector3Int(playerChunk.x + x, 0, playerChunk.z + z);
                if (!chunkRenderers.ContainsKey(coord))
                    CreateChunk(coord);
            }

        // despawn: renderery mimo dosah + 1
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
            // chunks[coord] z·merne NEMAZAç ó d·ta preûÌvaj˙
        }
    }
}