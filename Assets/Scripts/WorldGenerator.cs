using UnityEngine;

public static class WorldGenerator
{
    static float seedOffsetX;
    static float seedOffsetZ;


    public static void Initialize(int seed)
    {
        Random.InitState(seed);
        seedOffsetX = Random.Range(-100000f, 100000f);
        seedOffsetZ = Random.Range(-100000f, 100000f);
    }


    public static float GetHeight(int x, int z)
    {
        float scale = 0.1f; // Adjust the scale for more or less variation
        float height = Mathf.PerlinNoise((x + seedOffsetX) * scale, (z + seedOffsetZ) * scale);
        return height * WorldSettings.ChunkHeight; // Scale to chunk height
    }


    public static void GenerateChunk(Chunk chunk)
    {
        Vector3Int chunkCoord = chunk.coord;
        for (int x = 0; x < WorldSettings.ChunkWidth; x++)
        {
            for (int z = 0; z < WorldSettings.ChunkWidth; z++)
            {
                int worldX = chunkCoord.x * WorldSettings.ChunkWidth + x;
                int worldZ = chunkCoord.z * WorldSettings.ChunkWidth + z;
                float height = GetHeight(worldX, worldZ);
                int blockHeight = Mathf.FloorToInt(height);
                for (int y = 0; y < blockHeight; y++)
                {
                    Vector3Int localPos = new Vector3Int(x, y, z);
                    BlockType blockType = BlockTypeHelper.GetBlockTypeFromHeight(y);
                    chunk.SetBlock(localPos, blockType);
                }
            }
        }
    }


   

}
