using UnityEngine;


/// <summary>
/// Fixed-size item inventory with stacking and slot selection.
/// Displayed by HUD as the hotbar; selection drives which block the player builds.
/// </summary>
public class Inventory : MonoBehaviour
{

    public int MaxSize = 10;
    public Item[] items;
    public int selectedIndex = 0;

    void Awake()
    {
        items = new Item[MaxSize];
    }

    public void AddItem(Item item)
    {
       for (int i = 0; i < items.Length; i++)
        {

            if(items[i] != null && items[i].IsSameItem(item))
            {
                if(items[i].Count + item.Count > items[i].MaxStackSize)
                {
                    int remaining = items[i].Count + item.Count - items[i].MaxStackSize;
                    items[i].Count = items[i].MaxStackSize;
                    item.Count = remaining;
                    continue;
                }
                items[i].Count += item.Count;
                return;
            }
            if (items[i] == null)
            {
                items[i] = item;
                item.Index = i;
                return;
            }
        }
    }

    public Item GetSelectedItem()
    {
        return items[selectedIndex];
    }

    public void Remove(int index)
    {
        if (index >= 0 && index < items.Length)
        {
            items[index] = null;
        }
    }

    public void Scroll(int direction)
    {
        selectedIndex += direction;
        selectedIndex = Mathf.Clamp(selectedIndex, 0, MaxSize - 1);

    }

    public void SelectSlot(int index)
    {
        if (index >= 0 && index < MaxSize)
        {
            selectedIndex = index;
        }
    }
}
