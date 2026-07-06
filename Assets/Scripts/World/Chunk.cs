using UnityEngine;

/// <summary>
/// Raw block data for one 16x64x16 chunk, stored as a flat array of block types.
/// Plain C# class (not a MonoBehaviour) so it can exist without a GameObject
/// and survive renderer despawning. IsModified marks chunks that need saving.
/// </summary>
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

    int GetIndex(Vector3Int localPos) => localPos.x + WorldSettings.ChunkWidth * (localPos.y + WorldSettings.ChunkHeight * localPos.z); // Flat 1D index for the 3D position, laid out x -> y -> z.

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
        if (data.Length != blocks.Length)  // Guard against corrupted or truncated save files.
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
