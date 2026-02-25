using System.Collections.Generic;
using UnityEngine;


// Manages the inventory panel on the left side of the screen.
// Tiles bought from the shop are added here.
// Players drag tiles from here onto the grid.

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform     inventorySlotParent;  // left panel content area
    [SerializeField] private InventorySlot inventorySlotPrefab;

    // Lookup by TileDefinition
    private Dictionary<TileDefinition, InventorySlot> _slots =
        new Dictionary<TileDefinition, InventorySlot>();

    
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    //Called by ShopSlot when a tile is purchased
    public void AddTile(TileDefinition tile)
    {
        if (_slots.TryGetValue(tile, out InventorySlot existing))
        {
            existing.AddToCount(1);
            return;
        }

        // First time this tile type has been added — create a new slot
        InventorySlot slot = Instantiate(inventorySlotPrefab, inventorySlotParent);
        slot.Init(tile);
        slot.AddToCount(1);
        _slots[tile] = slot;
    }

    //Called by GridCell when a tile is removed from the grid 
    public void ReturnTile(TileDefinition tile)
    {
        if (tile == null) return;
        if (_slots.TryGetValue(tile, out InventorySlot slot))
            slot.AddToCount(1);
        else
            AddTile(tile); // edge case: slot doesn't exist yet, create it
    }

    // Called by DragHandler to decrement count after placement 
    public void OnTilePlaced(TileDefinition tile)
    {
        if (_slots.TryGetValue(tile, out InventorySlot slot))
            slot.OnTilePlaced();
    }

    public InventorySlot GetSlot(TileDefinition tile)
    {
        _slots.TryGetValue(tile, out InventorySlot slot);
        return slot;
    }
}
