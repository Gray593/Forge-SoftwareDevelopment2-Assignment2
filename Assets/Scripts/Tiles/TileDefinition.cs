using UnityEngine;

public enum TileType // the different types of tiles set as a enum so mine is 0 refiner is 1 and forge is 2
{
    Mine,
    Refiner,
    Forge
}


[CreateAssetMenu(fileName = "NewTile", menuName = "Forge/TileDefinition")] // creates a new option in thr tight click create menu
public class TileDefinition : ScriptableObject // tile definition inherits from the scriptable object unity base class
{
    // the below lines of code establish input fields in the unity inspector
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
