using UnityEngine;


/// <summary>
/// Inventory item representing a stack of placeable blocks of one type.
/// Using it attempts to build the block and decrements the stack.
/// </summary>
public class BlockItem : Item
{
    public BlockType blockType;
    public override void Use()
    {
        if (Count > 0 && player.Build(blockType))
        {
            Count--;
        }
        if (Count <= 0)
        {
            RemoveFromInventory();
        }
    }

    public BlockItem(PlayerController player, BlockType blockType, int count = 1) : base(player, count)
    {
        this.blockType = blockType;
        itemType = ItemType.Block;
        MaxStackSize = 64;


    }

    public override string GetItemName()
    {
        return blockType.ToString();
    }

    public override void AddToInventory()
    {
        player.Inventory.AddItem(this);
    }

    public override bool IsSameItem(Item other)
    {
        if (base.IsSameItem(other))
        {
            
            
                return this.blockType == ((BlockItem)other).blockType;
            
            
        }
        return false;

    }
}


