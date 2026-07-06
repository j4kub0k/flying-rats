using UnityEngine;

public class Chunk
{
    

    public Vector3Int coord;
    BlockType[] blocks;
    public bool IsModified  = false;


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
       

    }

    int GetIndex(Vector3Int localPos) => localPos.x + WorldSettings.ChunkWidth * (localPos.y + WorldSettings.ChunkHeight * localPos.z);

    bool IsInBounds(Vector3Int localPos) => localPos.x >= 0 && localPos.x < WorldSettings.ChunkWidth &&
                                         localPos.y >= 0 && localPos.y < WorldSettings.ChunkHeight &&
                                         localPos.z >= 0 && localPos.z < WorldSettings.ChunkWidth;

    public byte[] Serialize()
    {
        byte[] data = new byte[blocks.Length];
        for (int i = 0; i < blocks.Length; i++)
        {
            data[i] = (byte)blocks[i];
        }
        return data;
    }

    public Chunk Deserialize(byte[] data)
    {
        if (data.Length != blocks.Length)
        {
            return this;
        }
        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i] = (BlockType)data[i];
        }
        return this;
    }
}
