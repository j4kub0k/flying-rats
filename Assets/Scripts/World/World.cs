using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{

    Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

    public int WorldSizeInChunks = 10; // Size of the world in chunks

    public Material Material; // Material for the chunks


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        WorldGenerator.Initialize(Random.Range(int.MinValue, int.MaxValue));
        GenerateWorld();
    }

    void GenerateWorld()
    {
        int half = WorldSizeInChunks / 2;

        for (int x = -half; x <= half; x++)
        {
            for (int z = -half; z <= half; z++)
            {
                Vector3Int chunkCoord = new Vector3Int(x, 0, z);
                CreateChunk(chunkCoord);
            }
        }
    }


    void CreateChunk(Vector3Int chunkCoord)
    {
        Chunk chunk = new Chunk(chunkCoord);
        WorldGenerator.GenerateChunk(chunk);
        chunks[chunkCoord] = chunk;

        // Vytvor GameObject na vykreslenie tohto chunku
        GameObject chunkObject = new GameObject($"Chunk {chunkCoord.x},{chunkCoord.z}");
        chunkObject.transform.parent = this.transform;
        chunkObject.transform.position = new Vector3(
            chunkCoord.x * WorldSettings.ChunkWidth,
            chunkCoord.y * WorldSettings.ChunkHeight,
            chunkCoord.z * WorldSettings.ChunkWidth
        );
        ChunkGenerator renderer = chunkObject.AddComponent<ChunkGenerator>();
        renderer.GenerateMesh(chunk, Material); // alebo ako sa tvoja metóda volá
    }
}
