using UnityEngine;




public enum BlockType
{
    Air,
    Gray,
    Green,
    White,
}
/// <summary>
/// Block type definitions and their per-type properties (color, mining time,
/// height placement). Enum-based storage keeps chunk data compact and trivially
/// serializable; switch-based lookups are fine at this block count.
/// </summary>
public static class BlockTypeHelper
{

    public static Color GetColor(BlockType blockType)
    {
        return blockType switch
        {
            BlockType.Air => Color.clear,
            BlockType.Gray => Color.gray,
            BlockType.Green => Color.green,
            BlockType.White => Color.white,
            _ => Color.magenta, // Default color for unknown block types
        };
    }

    public static BlockType GetBlockTypeFromHeight(int y)
    {
        if (y < HeightLimit(BlockType.Gray))
            return BlockType.Gray; // Stone
        else if (y < HeightLimit(BlockType.Green))
            return BlockType.Green; // Grass
        else
            return BlockType.White; // Snow
    }


   public static float TimeToDestroy(BlockType blockType)
    {
        return blockType switch
        {
            BlockType.Air => 0f,
            BlockType.Gray => 2f,    // rock - longest to mine
            BlockType.Green => 1f,   // grass - normal
            BlockType.White => 0.5f, // snow - fastest
            _ => 1f, // Default time for unknown block types
        };
 
    }


    public static bool Unbreakable(BlockType blockType)
    {
        return blockType switch
        {
            BlockType.Air => true,
            BlockType.Gray => false,
            BlockType.Green => false,
            BlockType.White => false,
            _ => false, // Default for unknown block types
        };
    }


    public static int HeightLimit(BlockType blockType)
    {
        return blockType switch
        {
            BlockType.Air => 0,
            BlockType.Gray => 20,
            BlockType.Green => 32,
            BlockType.White => WorldSettings.MaxTerrainHeight,
            _ => WorldSettings.MaxTerrainHeight, // Default for unknown block types
        };
    }

}
