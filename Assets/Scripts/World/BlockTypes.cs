using UnityEngine;




public enum BlockType
{
    Air,
    Gray,
    Green,
    White,
}

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
        if (y < WorldSettings.GrayMaxHeight)
            return BlockType.Gray; // Stone
        else if (y < WorldSettings.GreenMaxHeight)
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

}
