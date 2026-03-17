using System.Collections.Generic;
using UnityEngine;


// This class manages all shop slots present in the shop panel

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Tile Definitions (assign in Inspector)")]
    [SerializeField] private TileDefinition[] tileDefinitions;

    [Header("References")]
    [SerializeField] private Transform shopSlotParent;
    [SerializeField] private ShopSlot  shopSlotPrefab;

    private List<ShopSlot> _slots = new List<ShopSlot>();

    
    private void Awake() // destroys any other instances of this class
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() // creates a shop slot for each tile definition
    {
        foreach (TileDefinition def in tileDefinitions)
        {
            ShopSlot slot = Instantiate(shopSlotPrefab, shopSlotParent);
            slot.Init(def);
            _slots.Add(slot);
        }
    }

    //Called by GameManager when a new goal is reached
    public void RefreshUnlocks(int goalsCompleted)
    {
        foreach (var slot in _slots)
            slot.CheckUnlock(goalsCompleted);
    }
}