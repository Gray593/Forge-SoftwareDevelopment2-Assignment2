using System.Collections.Generic;
using UnityEngine;


// Manages the inventory panel as a whole
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform     inventorySlotParent;  // left panel content area
    [SerializeField] private InventorySlot inventorySlotPrefab;

    // Lookup by TileDefinition
    private Dictionary<TileDefinition, InventorySlot> _slots =
        new Dictionary<TileDefinition, InventorySlot>();

    
    private void Awake() // Destroys any other instances of itself
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // This function adds a tile to the inventory when it is purchased from the shop
    public void AddTile(TileDefinition tile)
    {
        if (_slots.TryGetValue(tile, out InventorySlot existing))
        {
            existing.AddToCount(1);
            return;
        }

        // Creates a new slot if this is the first time purchasing this type of tile
        InventorySlot slot = Instantiate(inventorySlotPrefab, inventorySlotParent);
        slot.Init(tile);
        slot.AddToCount(1);
        _slots[tile] = slot;
    }

    // Is called when a tile is returned to the inventory to increment count and create a slot if it doesn't already have one 
    public void ReturnTile(TileDefinition tile)
    {
        if (tile == null) return;
        if (_slots.TryGetValue(tile, out InventorySlot slot))
            slot.AddToCount(1);
        else
            AddTile(tile);
    }

    // Decrements the count after a tile has been placed
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
