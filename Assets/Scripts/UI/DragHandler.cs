using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class DragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public TileDefinition TileDefinition { get; private set; }

    private Image         _image;
    private Canvas        _canvas;
    private InventorySlot _inventoryOwner;
    private GridCell      _gridOwner;
    private GameObject    _ghost;
    private bool          _placedSuccessfully = false; // prevents double return on success

    //Init Methods
    //Called by InventorySlot.Init()
    public void InitFromInventory(TileDefinition tile, InventorySlot owner)
    {
        TileDefinition  = tile;
        _inventoryOwner = owner;
        _image          = GetComponent<Image>();
        _canvas         = FindFirstObjectByType<Canvas>();
    }

    //Called by GridCell when it becomes draggable
    public void InitFromGrid(TileDefinition tile, GridCell owner)
    {
        TileDefinition = tile;
        _gridOwner     = owner;
        _image         = GetComponent<Image>();
        _canvas        = FindFirstObjectByType<Canvas>();
    }

    //Drag Lifecycle
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Check we are allowed to drag
        if (_inventoryOwner != null && !_inventoryOwner.CanDrag)
        {
            eventData.pointerDrag = null;
            return;
        }

        // Find canvas here in case it wasn't ready during Init
        if (_canvas == null) _canvas = FindFirstObjectByType<Canvas>();
        if (_canvas == null) { Debug.LogError("No Canvas found in scene"); return; }

        // Create a ghost copy that follows the mouse
        // The original stays in the inventory panel
        _ghost = new GameObject("DragGhost");
        _ghost.transform.SetParent(_canvas.transform, false);

        Image ghostImage         = _ghost.AddComponent<Image>();
        ghostImage.sprite        = _image.sprite;
        ghostImage.raycastTarget = false; // so drops register on cells below

        RectTransform ghostRT  = _ghost.GetComponent<RectTransform>();
        ghostRT.sizeDelta      = GetComponent<RectTransform>().sizeDelta;
        ghostRT.position       = transform.position;

        CanvasGroup cg         = _ghost.AddComponent<CanvasGroup>();
        cg.blocksRaycasts      = false;
        cg.alpha               = 0.8f;

        // If dragging from grid, remove tile from that cell immediately
        _placedSuccessfully = false;
        if (_gridOwner != null)
            GridManager.Instance.TryRemoveTile(_gridOwner.GridX, _gridOwner.GridY);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_ghost != null)
            _ghost.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Ghost was not dropped on a valid cell — clean up
        if (_ghost != null) { Destroy(_ghost); _ghost = null; }

        // Only return to inventory if we picked up from grid AND never placed successfully
        if (_gridOwner != null && !_placedSuccessfully)
            InventoryManager.Instance?.ReturnTile(TileDefinition);
    }

    //Placement Callbacks (called by GridCell.OnDrop)
    public void OnPlacedSuccessfully()
    {
        _placedSuccessfully = true;
        if (_ghost != null) { Destroy(_ghost); _ghost = null; }

        // Decrement inventory count only if dragged from inventory
        if (_inventoryOwner != null)
            _inventoryOwner.OnTilePlaced();

        AudioManager.Instance?.PlaySnap();
    }

    public void OnPlaceFailed()
    {
        if (_ghost != null) { Destroy(_ghost); _ghost = null; }

        // If picked from grid and placement failed, return tile to inventory
        if (_gridOwner != null)
            InventoryManager.Instance?.ReturnTile(TileDefinition);

        AudioManager.Instance?.PlayError();
    }
}