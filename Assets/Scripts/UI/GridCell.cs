using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


// Attached to each cell in the visual grid.
// Acts as a drop target for tiles dragged from the shop/inventory.

public class GridCell : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("References")]
    [SerializeField] private Image    backgroundImage;
    [SerializeField] private Image    tileIconImage;
    [SerializeField] private Color    emptyColor    = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color    occupiedColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    public int GridX { get; private set; }
    public int GridY { get; private set; }

    private TileDefinition _currentTile;
    public void Init(int x, int y)
    {
        GridX = x;
        GridY = y;
        ClearTile();
    }

    // Tile Display
    public void SetTile(TileDefinition tile)
    {
        _currentTile = tile;
        tileIconImage.sprite        = tile.icon;
        tileIconImage.enabled       = true;
        tileIconImage.raycastTarget = true;
        if (backgroundImage) backgroundImage.color = occupiedColor;

        DragHandler drag = tileIconImage.GetComponent<DragHandler>();
        if (drag != null) drag.InitFromGrid(tile, this);

        AudioManager.Instance?.PlaySnap();
    }

    public void ClearTile()
    {
        _currentTile = null;
        tileIconImage.enabled       = false;
        tileIconImage.raycastTarget = false;
        if (backgroundImage) backgroundImage.color = emptyColor;
    }

    //Drop Handler (receives tiles dragged from inventory or grid) 
    public void OnDrop(PointerEventData eventData)
    {
        DragHandler drag = eventData.pointerDrag?.GetComponent<DragHandler>();
        if (drag == null) return;

        bool placed = GridManager.Instance.TryPlaceTile(GridX, GridY, drag.TileDefinition);
        if (placed)
            drag.OnPlacedSuccessfully();
        else
            drag.OnPlaceFailed();
    }

    //Right-click to return tile to inventory
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (_currentTile != null && GridManager.Instance.TryRemoveTile(GridX, GridY))
                InventoryManager.Instance?.ReturnTile(_currentTile);
        }
    }
}