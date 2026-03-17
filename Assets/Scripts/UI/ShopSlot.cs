using TMPro;
using UnityEngine;
using UnityEngine.UI;


// The Shop Slot class represents one slot in the shop panel 

public class ShopSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image       iconImage;
    [SerializeField] private TMP_Text    nameText;
    [SerializeField] private TMP_Text    costText;
    [SerializeField] private TMP_Text    unlockText;
    [SerializeField] private Button      buyButton;
    [SerializeField] private GameObject  lockedOverlay;

    public TileDefinition Tile     { get; private set; }

    private bool _unlocked = false;

    //The Init function assigns all the properties of a tile to its specific slot and creates an event listener for when the 
    // tile is bought
    public void Init(TileDefinition tile)
    {
        Tile = tile;

        iconImage.sprite = tile.icon;
        nameText.text    = tile.displayName;
        costText.text    = $"£{tile.shopCost:0}";

        _unlocked = tile.unlockAtGoal == 0;
        RefreshUI();

        buyButton.onClick.AddListener(OnBuyClicked);
    }

    // This function checks if the tile in this slot can be unlocked yet and therefore purchased
    public void CheckUnlock(int goalsCompleted)
    {
        if (!_unlocked && goalsCompleted >= Tile.unlockAtGoal)
        {
            _unlocked = true;
            RefreshUI();
            NotificationManager.Instance?.ShowNotification($"{Tile.displayName} unlocked!");
        }
    }

    // This function is called when the user tries to buy a tile and determines if they can or not 
    private void OnBuyClicked()
    {
        if (!_unlocked) return;
        if (!GameManager.Instance.SpendBalance(Tile.shopCost)) return;

        InventoryManager.Instance?.AddTile(Tile);
        AudioManager.Instance?.PlayPurchase();
    }

    // Similar to the function in the inventory slot class this function refreshes the ui after an action has taken place
    private void RefreshUI()
    {
        if (lockedOverlay) lockedOverlay.SetActive(!_unlocked);
        buyButton.interactable = _unlocked;

        if (unlockText)
            unlockText.text = _unlocked ? "" : $"Unlock at Goal {Tile.unlockAtGoal}";
    }
}