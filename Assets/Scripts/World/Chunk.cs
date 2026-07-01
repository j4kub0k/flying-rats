using UnityEngine;

public class Chunk
{
    

    public Vector3Int coord;
    BlockType[] blocks;


    public Chunk(Vector3Int coord)
    {
        this.coord = coord;
        blocks = new BlockType[WorldSettings.ChunkWidth * WorldSettings.ChunkHeight * WorldSettings.ChunkWidth];
    }


    public BlockType GetBlock(Vector3Int localPos)
    {
       if (IsInBounds(localPos))
        {
            return blocks[GetIndex(localPos)];
        }
        return BlockType.Air; // Placeholder return value
    }

    public void SetBlock(Vector3Int localPos, BlockType blockType)
    {
       if (IsInBounds(localPos))
        {
            blocks[GetIndex(localPos)] = blockType;
        }
       else
        {
           Debug.LogWarning($"Local position {localPos} is out of bounds for chunk at {coord}");
        }

    }

    int GetIndex(Vector3Int localPos) => localPos.x + WorldSettings.ChunkWidth * (localPos.y + WorldSettings.ChunkHeight * localPos.z);

    bool IsInBounds(Vector3Int localPos) => localPos.x >= 0 && localPos.x < WorldSettings.ChunkWidth &&
                                         localPos.y >= 0 && localPos.y < WorldSettings.ChunkHeight &&
                                         localPos.z >= 0 && localPos.z < WorldSettings.ChunkWidth;


}
