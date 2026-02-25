using System.Collections.Generic;
using UnityEngine;


// Manages the logical grid (a 2-D array of TileDefinitions).
// Evaluates balance from all valid Mine → Refiner* → Forge chains each tick.

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    //Inspector
    [Header("Grid Size")]
    [SerializeField] private int columns = 8;
    [SerializeField] private int rows    = 8;

    [Header("Visuals")]
    [SerializeField] private GameObject cellPrefab;   // GridCell prefab
    [SerializeField] private Transform  cellParent;   // GridPanel RectTransform
    [SerializeField] private float      cellSize = 100f;

    //State
    private TileDefinition[,] _grid;   // logical grid
    private GridCell[,]       _cells;  // visual cells

    public int Columns => columns;
    public int Rows    => rows;

    //Adjacent directions (N / S / E / W)
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _grid  = new TileDefinition[columns, rows];
        _cells = new GridCell[columns, rows];
    }

    private void Start() => SpawnVisualCells();

    //Visual Cell Spawning
    private void SpawnVisualCells()
    {
        for (int x = 0; x < columns; x++)
        for (int y = 0; y < rows;    y++)
        {
            GameObject go = Instantiate(cellPrefab, cellParent);
            go.name = $"Cell_{x}_{y}";

            // UI grids use RectTransform — positive Y goes UP so we negate it
            // so row 0 is at the top and rows increase downward
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(x * cellSize, -y * cellSize);

            GridCell cell = go.GetComponent<GridCell>();
            cell.Init(x, y);
            _cells[x, y] = cell;
        }
    }

    //Placement 
    public bool TryPlaceTile(int x, int y, TileDefinition tile)
    {
        if (!IsInBounds(x, y))  return false;
        if (_grid[x, y] != null) return false;   // cell already occupied

        _grid[x, y] = tile;
        _cells[x, y].SetTile(tile);
        return true;
    }

    public bool TryRemoveTile(int x, int y)
    {
        if (!IsInBounds(x, y) || _grid[x, y] == null) return false;
        _grid[x, y] = null;
        _cells[x, y].ClearTile();
        return true;
    }

    public TileDefinition GetTile(int x, int y) =>
        IsInBounds(x, y) ? _grid[x, y] : null;

    public bool IsOccupied(int x, int y) =>
        IsInBounds(x, y) && _grid[x, y] != null;

    public bool IsInBounds(int x, int y) =>
        x >= 0 && x < columns && y >= 0 && y < rows;

    // ── Chain Evaluation ──────────────────────────────────────────────────
    /// <summary>
    /// Called every tick by GameManager.
    /// Finds every Mine, then BFS through adjacent Refiners to reach Forges.
    /// Returns total balance earned this tick.
    /// </summary>
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

    /// <summary>
    /// BFS from a Mine through adjacent Refiners.
    /// When we reach a cell adjacent to a Forge we complete the chain.
    /// Chain value = mine.baseValue * product of all refiner multipliers along path.
    /// We accumulate contributions from ALL reachable Forges.
    /// </summary>
    private float EvaluateMine(int mx, int my, HashSet<Vector2Int> usedForges)
    {
        TileDefinition mine = _grid[mx, my];
        float total = 0f;

        // Each queue entry: (position, accumulated multiplier so far)
        Queue<(Vector2Int pos, float multiplier)> queue = new Queue<(Vector2Int, float)>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

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

                if (neighbourTile.tileType == TileType.Forge)
                {
                    // Chain is complete — only count each Forge once per tick
                    if (!usedForges.Contains(neighbour))
                    {
                        usedForges.Add(neighbour);
                        total += mine.baseValue * mult;
                    }
                }
                else if (neighbourTile.tileType == TileType.Refiner)
                {
                    // Continue BFS through Refiner, apply its multiplier
                    queue.Enqueue((neighbour, mult * neighbourTile.refinerMultiplier));
                }
                // Mines block the path (don't traverse through another Mine)
            }
        }

        return total;
    }
}