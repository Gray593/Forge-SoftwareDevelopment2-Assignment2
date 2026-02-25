using UnityEngine;

public enum TileType
{
    Mine,
    Refiner,
    Forge
}

/// <summary>
/// ScriptableObject that defines a tile's properties.
/// Create assets via: Right-click in Project > Create > Forge > TileDefinition
/// </summary>
[CreateAssetMenu(fileName = "NewTile", menuName = "Forge/TileDefinition")]
public class TileDefinition : ScriptableObject
{
    [Header("Identity")]
    public TileType tileType;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Economics")]
    [Tooltip("Base balance produced per tick (Mines only)")]
    public float baseValue = 1f;

    [Tooltip("Multiplier applied to the chain value passing through this tile (Refiners only)")]
    public float refinerMultiplier = 2f;

    [Tooltip("Cost to purchase from the shop")]
    public float shopCost = 10f;

    [Tooltip("Goal level at which this tile becomes available in the shop (0 = available from start)")]
    public int unlockAtGoal = 0;
}
