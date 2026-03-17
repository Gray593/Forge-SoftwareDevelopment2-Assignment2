using TMPro;
using UnityEngine;
using UnityEngine.UI;


// This class represents one inventory slot in the inventory panel 

public class InventorySlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] public Image     iconImage;
    [SerializeField] public TMP_Text  countText;
    [SerializeField] public TMP_Text  nameText;

    public TileDefinition Tile          { get; private set; }
    public int            Count         { get; private set; }
    public bool           CanDrag       => Count > 0;

    //sets the slot properties to that of the tile inside it
    public void Init(TileDefinition tile)
    {
        Tile = tile;
        if (iconImage) iconImage.sprite = tile.icon;
        if (nameText)  nameText.text    = tile.displayName;
        RefreshUI();

        DragHandler drag = iconImage.GetComponent<DragHandler>();
        if (drag != null)
            drag.InitFromInventory(tile, this);
    }

    // Increments count when a new version of the tile in this slot is bought
    public void AddToCount(int amount = 1)
    {
        Count += amount;
        RefreshUI();
    }
    // Decrements count 
    public void RemoveFromCount(int amount = 1)
    {
        Count = Mathf.Max(0, Count - amount);
        RefreshUI();
    }

    // Decrements count when a tile is placed 
    public void OnTilePlaced()
    {
        RemoveFromCount(1);
    }

    //refreshes the UI when action has taken place involving this tile
    private void RefreshUI()
    {
        if (countText) countText.text = $"x{Count}";
        if (iconImage) iconImage.color = Count > 0
            ? Color.white
            : new Color(1f, 1f, 1f, 0.3f); // dim when empty
    }
}
