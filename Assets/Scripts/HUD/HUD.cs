using UnityEngine;
using UnityEngine.UIElements;

public class HUD : MonoBehaviour
{
    public PlayerController playerController;

    VisualElement barBackground;
    VisualElement barFill;
    VisualElement[] slots;
    VisualElement[] swatches;
    Label[] counts;
    bool isInventoryVisible = true;

    public float SlotSize = 48f;
    public float SlotSpacing = 4f;
    public float BarWidth = 200f;
    public float BarHeight = 12f;

    public float CrosshairSize = 16f;      
    public float CrosshairThickness = 2f;
    VisualElement root;

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        
        barBackground = new VisualElement();
        barBackground.style.position = Position.Absolute;
        barBackground.style.width = BarWidth;
        barBackground.style.height = BarHeight;
        barBackground.style.left = Length.Percent(50);
        barBackground.style.top = Length.Percent(55);
        barBackground.style.marginLeft = -BarWidth / 2f;   
        barBackground.style.backgroundColor = new Color(0f, 0f, 0f, 0.5f);
        barBackground.style.display = DisplayStyle.None;   
       
        barFill = new VisualElement();
        barFill.style.width = Length.Percent(0);
        barFill.style.height = Length.Percent(100);
        barFill.style.backgroundColor = new Color(1f, 1f, 1f, 0.9f);

        barBackground.Add(barFill);
        root.Add(barBackground);

        if (playerController == null)
        {
            return;
        }
        CreateCrosshair(root);
        CreateHotBar(root);
        isInventoryVisible = true;
    }

    void Update()
    {
        // If we have a playerController but the UI hasn't been built yet, build it.
        if (playerController != null && slots == null)
        {
            CreateCrosshair(root);
            CreateHotBar(root);
            isInventoryVisible = true;
        }

        if (playerController == null) return;
        if (!isInventoryVisible) return;

        FillBar();
        ShowInventory();
    }

    void CreateCrosshair(VisualElement root)
    {
        Color color = new Color(1f, 1f, 1f, 0.8f);

        // vodorovná èiarka
        VisualElement horizontal = new VisualElement();
        horizontal.style.position = Position.Absolute;
        horizontal.style.width = CrosshairSize;
        horizontal.style.height = CrosshairThickness;
        horizontal.style.left = Length.Percent(50);
        horizontal.style.top = Length.Percent(50);
        horizontal.style.marginLeft = -CrosshairSize / 2f;
        horizontal.style.marginTop = -CrosshairThickness / 2f;
        horizontal.style.backgroundColor = color;

        // zvislá èiarka
        VisualElement vertical = new VisualElement();
        vertical.style.position = Position.Absolute;
        vertical.style.width = CrosshairThickness;
        vertical.style.height = CrosshairSize;
        vertical.style.left = Length.Percent(50);
        vertical.style.top = Length.Percent(50);
        vertical.style.marginLeft = -CrosshairThickness / 2f;
        vertical.style.marginTop = -CrosshairSize / 2f;
        vertical.style.backgroundColor = color;

        root.Add(horizontal);
        root.Add(vertical);
    }

    void CreateHotBar(VisualElement root)
    {

        int slotCount = playerController.Inventory.MaxSize;

        slots = new VisualElement[slotCount];
        swatches = new VisualElement[slotCount];
        counts = new Label[slotCount];

        float totalWidth = slotCount * SlotSize + (slotCount - 1) * SlotSpacing;

        VisualElement container = new VisualElement();
        container.style.position = Position.Absolute;
        container.style.bottom = 16f;
        container.style.left = Length.Percent(50);
        container.style.marginLeft = -totalWidth / 2f;
        container.style.flexDirection = FlexDirection.Row;

        for (int i = 0; i < slotCount; i++)
        {
            VisualElement slot = new VisualElement();
            slot.style.width = SlotSize;
            slot.style.height = SlotSize;
            slot.style.marginRight = (i < slotCount - 1) ? SlotSpacing : 0f;
            slot.style.backgroundColor = new Color(0f, 0f, 0f, 0.5f);
            SetBorder(slot, new Color(0.2f, 0.2f, 0.2f, 0.8f), 2f);

            // farebný swatch bloku – vyplní slot s malým odsadením
            VisualElement swatch = new VisualElement();
            swatch.style.position = Position.Absolute;
            swatch.style.left = 6f;
            swatch.style.top = 6f;
            swatch.style.right = 6f;
            swatch.style.bottom = 6f;
            swatch.style.backgroundColor = Color.clear;

            // poèet kusov – pravý dolný roh
            Label count = new Label();
            count.style.position = Position.Absolute;
            count.style.right = 2f;
            count.style.bottom = 2f;
            count.style.color = Color.white;
            count.style.fontSize = 14f;
            count.style.unityTextOutlineColor = Color.black;   // obrys, nech je èitate¾ný aj na bielom snowe
            count.style.unityTextOutlineWidth = 1f;
            count.text = "";

            slot.Add(swatch);
            slot.Add(count);
            container.Add(slot);

            slots[i] = slot;
            swatches[i] = swatch;
            counts[i] = count;
        }

        root.Add(container);
    }

    static void SetBorder(VisualElement el, Color color, float width)
    {
        el.style.borderLeftColor = color;
        el.style.borderRightColor = color;
        el.style.borderTopColor = color;
        el.style.borderBottomColor = color;
        el.style.borderLeftWidth = width;
        el.style.borderRightWidth = width;
        el.style.borderTopWidth = width;
        el.style.borderBottomWidth = width;
    }


    private void OnDestroy()
    {
        if (barBackground != null)
        {
            barBackground.RemoveFromHierarchy();
        }
    }


    void FillBar()
    {
        if (playerController == null) return;

        float progress = playerController.MiningProgress;

        if (progress > 0f)
        {
            barBackground.style.display = DisplayStyle.Flex;
            barFill.style.width = Length.Percent(Mathf.Clamp01(progress) * 100f);
        }
        else
        {
            barBackground.style.display = DisplayStyle.None;
        }

    }

    void ShowInventory()
    {
        Inventory inventory = playerController.Inventory;
        if(inventory == null) return;
        for (int i = 0; i < slots.Length; i++)
        {
            Item item = inventory.items[i];

            if (item is BlockItem blockItem)
            {
                swatches[i].style.backgroundColor = BlockTypeHelper.GetColor(blockItem.blockType);
                counts[i].text = item.Count.ToString();
            }
            else
            {
                swatches[i].style.backgroundColor = Color.clear;
                counts[i].text = "";
            }

            bool selected = (i == inventory.selectedIndex);
            SetBorder(slots[i],
                selected ? Color.white : new Color(0.2f, 0.2f, 0.2f, 0.8f),
                selected ? 3f : 2f);
        }
    }
}
