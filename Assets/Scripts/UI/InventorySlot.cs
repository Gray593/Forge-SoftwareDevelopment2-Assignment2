using TMPro;
using UnityEngine;
using UnityEngine.UI;


// Represents one tile type in the inventory panel on the left.
// Shows how many of that tile the player currently owns.
// Spawns a ghost copy when dragged onto the grid.

public class InventorySlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] public Image     iconImage;
    [SerializeField] public TMP_Text  countText;
    [SerializeField] public TMP_Text  nameText;

    public TileDefinition Tile          { get; private set; }
    public int            Count         { get; private set; }
    public bool           CanDrag       => Count > 0;

    
    public void Init(TileDefinition tile)
    {
        Tile = tile;
        if (iconImage) iconImage.sprite = tile.icon;
        if (nameText)  nameText.text    = tile.displayName;
        RefreshUI();

        // Wire up drag handler on the icon
        DragHandler drag = iconImage.GetComponent<DragHandler>();
        if (drag != null)
            drag.InitFromInventory(tile, this);
    }

    //Count Management
    public void AddToCount(int amount = 1)
    {
        Count += amount;
        RefreshUI();
    }

    public void RemoveFromCount(int amount = 1)
    {
        Count = Mathf.Max(0, Count - amount);
        RefreshUI();
    }

    //Called by DragHandler after successful grid placement
    public void OnTilePlaced()
    {
        RemoveFromCount(1);
    }

    //UI
    private void RefreshUI()
    {
        if (countText) countText.text = $"x{Count}";
        if (iconImage) iconImage.color = Count > 0
            ? Color.white
            : new Color(1f, 1f, 1f, 0.3f); // dim when empty
    }
}
