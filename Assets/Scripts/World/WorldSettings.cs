using UnityEngine;

public static class WorldSettings
{
    public const int ChunkWidth = 16;
    public const int ChunkHeight = 64;
    public const int GrayMaxHeight = 20;
    public const int GreenMaxHeight = 32;
    public const int MaxTerrainHeight = 40;
    public const int MinMineableHeight = 1;




    public static Vector3Int WorldToChunkCoord(Vector3 worldPos)
    {
        int chunkX = Mathf.FloorToInt(worldPos.x / ChunkWidth);
        int chunkY = Mathf.FloorToInt(worldPos.y / ChunkHeight);
        int chunkZ = Mathf.FloorToInt(worldPos.z / ChunkWidth);
        return new Vector3Int(chunkX, chunkY, chunkZ);
    }


    public static Vector3Int WorldToLocalCoord(Vector3Int worldPos)
    {
        int localX = Mathf.FloorToInt(worldPos.x) % ChunkWidth;
        int localY = Mathf.FloorToInt(worldPos.y) % ChunkHeight;
        int localZ = Mathf.FloorToInt(worldPos.z) % ChunkWidth;
        // Handle negative coordinates
        if (localX < 0) localX += ChunkWidth;
        if (localY < 0) localY += ChunkHeight;
        if (localZ < 0) localZ += ChunkWidth;
        return new Vector3Int(localX, localY, localZ);
    }



}
