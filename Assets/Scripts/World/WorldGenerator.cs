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
        int octaves = 4;
        float persistence = 0.5f;
        float lacunarity = 2f;
        float baseScale = 0.02f;

        float amplitude = 1f;
        float frequency = 1f;
        float noiseHeight = 0f;
        float amplitudeSum = 0f; 

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (x + seedOffsetX) * baseScale * frequency;
            float sampleZ = (z + seedOffsetZ) * baseScale * frequency;

            float noiseValue = Mathf.PerlinNoise(sampleX, sampleZ);
            noiseHeight += noiseValue * amplitude;

            amplitudeSum += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        noiseHeight /= amplitudeSum;
        noiseHeight = Mathf.InverseLerp(0.3f, 0.7f, noiseHeight); 
        return Mathf.Lerp(WorldSettings.MinTerrainHeight, WorldSettings.MaxTerrainHeight, noiseHeight);
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
