using UnityEngine;


public enum ItemType
{
   Block,
   Weapon,
   Consumable,
   Tool,
}
public abstract class Item
{
    public ItemType itemType;
    public PlayerController player;
    public int MaxStackSize = 1;
    public int Count = 1;
    public int Index = 0;
    public Inventory inventory;

    public Item(PlayerController player, int count)
    {
        this.player = player;
        this.Count = count;
        inventory = player.Inventory;
    }


    abstract public void Use();

    abstract public string GetItemName();


    abstract public void AddToInventory();
    public void RemoveFromInventory()
    {
        inventory.Remove(Index);
    }

    virtual public bool IsSameItem(Item other)
    {
        return this.itemType == other.itemType;
    }

}
