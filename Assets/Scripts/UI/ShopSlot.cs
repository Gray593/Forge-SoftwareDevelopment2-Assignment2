using TMPro;
using UnityEngine;
using UnityEngine.UI;


// Represents one tile type in the shop.
// Handles purchasing and lock state only.
// Inventory tracking is handled by InventoryManager.

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

    //Init
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

    //Unlock
    public void CheckUnlock(int goalsCompleted)
    {
        if (!_unlocked && goalsCompleted >= Tile.unlockAtGoal)
        {
            _unlocked = true;
            RefreshUI();
            NotificationManager.Instance?.ShowNotification($"{Tile.displayName} unlocked!");
        }
    }

    //Buy
    private void OnBuyClicked()
    {
        if (!_unlocked) return;
        if (!GameManager.Instance.SpendBalance(Tile.shopCost)) return;

        InventoryManager.Instance?.AddTile(Tile);
        AudioManager.Instance?.PlayPurchase();
    }

    //UI
    private void RefreshUI()
    {
        if (lockedOverlay) lockedOverlay.SetActive(!_unlocked);
        buyButton.interactable = _unlocked;

        if (unlockText)
            unlockText.text = _unlocked ? "" : $"Unlock at Goal {Tile.unlockAtGoal}";
    }
}