using System.Collections.Generic;
using UnityEngine;


// This class exists to manage the in game grid 

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    // values that are displayed in the inspector 
    [Header("Grid Size")]
    [SerializeField] private int columns = 8;
    [SerializeField] private int rows    = 8;

    [Header("Visuals")]
    [SerializeField] private GameObject cellPrefab;   // GridCell prefab
    [SerializeField] private Transform  cellParent;   // GridPanel RectTransform
    [SerializeField] private float      cellSize = 100f;

    
    private TileDefinition[,] _grid;   // logical grid
    private GridCell[,]       _cells;  // visual cells
    public int Columns => columns;
    public int Rows    => rows;

    // Creates an array that contains directions to later be used in the breadth first search
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    // runs on the objects creation destroys duplicates of this object then creates two arrays one for the tiles placed and 
    // the other for the cell references
    private void Awake() 
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; } 
        Instance = this;
        _grid  = new TileDefinition[columns, rows];
        _cells = new GridCell[columns, rows];
    }
    // runs the first frame the object is active
    private void Start() => SpawnVisualCells();

    // this function loops through every grid position and creates a gridcell prefab named after its coordinates 
    private void SpawnVisualCells()
    {
        for (int x = 0; x < columns; x++)
        for (int y = 0; y < rows;    y++)
        {
            GameObject go = Instantiate(cellPrefab, cellParent);
            go.name = $"Cell_{x}_{y}";
            RectTransform rt = go.GetComponent<RectTransform>();  // Inverts the y axis
            rt.anchoredPosition = new Vector2(x * cellSize, -y * cellSize);
            GridCell cell = go.GetComponent<GridCell>();
            cell.Init(x, y); // tells the cell its own coordinates then stores the result
            _cells[x, y] = cell;
        }
    }

    // this function handles placing tiles 
    public bool TryPlaceTile(int x, int y, TileDefinition tile)
    {
        if (!IsInBounds(x, y))  return false; // checks if the tile is outside the grid
        if (_grid[x, y] != null) return false;   // checks if the grid cell is already occupied
        _grid[x, y] = tile; // stores the tiles x and y in the grid
        _cells[x, y].SetTile(tile); // updates the display
        return true; 
    }
    // This function handles removing tiles 
    public bool TryRemoveTile(int x, int y)
    {
        if (!IsInBounds(x, y) || _grid[x, y] == null) return false;
        _grid[x, y] = null; //clears the grid entry
        _cells[x, y].ClearTile(); // removes the tile from the display
        return true;
    }
    // the below functions are used to determine whether a tile is in bounds and whether the cell is occupied
    public TileDefinition GetTile(int x, int y) =>
        IsInBounds(x, y) ? _grid[x, y] : null;

    public bool IsOccupied(int x, int y) =>
        IsInBounds(x, y) && _grid[x, y] != null;

    public bool IsInBounds(int x, int y) =>
        x >= 0 && x < columns && y >= 0 && y < rows;

    // scans all cells in the grid for mines, then evaluates each chain before returning the balance total for the in game tick 
    public float EvaluateAllChains()
    {
        float total = 0f;
        HashSet<Vector2Int> usedForges = new HashSet<Vector2Int>(); // one mine per forge per tick

        for (int x = 0; x < columns; x++)
        for (int y = 0; y < rows;    y++)
        {
            if (_grid[x, y]?.tileType == TileType.Mine)
                total += EvaluateMine(x, y, usedForges);
        }

        return total;
    }

    // This function utilises a breadth first search to calculate the total for each chain by starting at a mine, taking the base value
    // then multiplying it along a chain of refiners before stopping at a forge.
    private float EvaluateMine(int mx, int my, HashSet<Vector2Int> usedForges)
    {
        TileDefinition mine = _grid[mx, my];
        float total = 0f;
        Queue<(Vector2Int pos, float multiplier)> queue = new Queue<(Vector2Int, float)>(); //queue entries store the position 
        // and current multiplier
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>(); // Hashset is used to prevent visiting the same tile twice
        Vector2Int minePos = new Vector2Int(mx, my);
        queue.Enqueue((minePos, 1f));
        visited.Add(minePos);
        while (queue.Count > 0)
        {
            var (current, mult) = queue.Dequeue();

            foreach (var dir in Directions)
            {
                Vector2Int neighbour = current + dir;
                if (!IsInBounds(neighbour.x, neighbour.y)) continue;
                if (visited.Contains(neighbour))           continue;

                TileDefinition neighbourTile = _grid[neighbour.x, neighbour.y];
                if (neighbourTile == null) continue;

                visited.Add(neighbour);

                if (neighbourTile.tileType == TileType.Forge) // checks if the chain is complete
                {
                    
                    if (!usedForges.Contains(neighbour)) // only counts each forge once
                    {
                        usedForges.Add(neighbour);
                        total += mine.baseValue * mult;
                    }
                }
                else if (neighbourTile.tileType == TileType.Refiner) // if the neighbouring tile is a refiner carries on the search
                {
                    queue.Enqueue((neighbour, mult * neighbourTile.refinerMultiplier));
                }
            }
        }

        return total;
    }
}