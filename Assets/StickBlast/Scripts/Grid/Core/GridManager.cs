using UnityEngine;
using StickBlast.Grid;
using StickBlast.Sticks;
using StickBlast.Level;
using System;

public class GridManager : MonoBehaviour
{
    
    [Header("Prefabs")]
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private GameObject connectionPrefab;
    [SerializeField] private GameObject cellPrefab;
    
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    
    [Header("Colors")]
    [SerializeField] private Color[] themeColors;
    [SerializeField] private Color emptyColor = Color.blue;

    // Core components
    private GridState state;
    private GridInitializer initializer;
    private GridLayoutController layoutController;
    private ConnectionManager connectionManager;
    private CellManager cellManager;
    private BlastManager blastManager;
    private PlacementManager placementManager;
    private GridValidator validator;
    private GridItemSpawner itemSpawner;
    private Color themeColor = Color.green;
    private int width = 6;
    private int height = 6;
    public static GridManager Instance { get; private set; }
    public static event Action OnGridResized;
    private void Awake()
    {
        Instance = this;
        InitializeManagers();
        SelectRandomThemeColor();
    }

    private void SelectRandomThemeColor()
    {
        themeColor = themeColors[UnityEngine.Random.Range(0, themeColors.Length)];
    }

    private void OnRectTransformDimensionsChange()
    {
        layoutController.UpdateLayout();
    }

    private void InitializeManagers()
    {
        state = new GridState(width, height, themeColor, emptyColor);

        initializer = new GridInitializer(state, transform, dotPrefab, connectionPrefab, cellPrefab);
        connectionManager = new ConnectionManager(state);
        layoutController = new GridLayoutController(transform, state, connectionManager, mainCamera);
        blastManager = new BlastManager(state, connectionManager, null);
        cellManager = new CellManager(state, connectionManager, blastManager);
        validator = new GridValidator(state, connectionManager);
        placementManager = new PlacementManager(state, validator, connectionManager, cellManager, blastManager);
        blastManager = new BlastManager(state, connectionManager, cellManager);
        itemSpawner = gameObject.AddComponent<GridItemSpawner>();
        
        if (itemSpawner == null)
        {
            itemSpawner = gameObject.AddComponent<GridItemSpawner>();
        }
    }

    public void InitializeFromLevel(LevelDefinition level)
    {
        width = level.width;
        height = level.height;
        InitializeManagers();
        initializer.InitializeGrid();
        layoutController.UpdateLayout();
        OnGridResized?.Invoke();
        
        if (level.hasCollectibles && level.winCondition is CollectItemsWinCondition)
        {
            itemSpawner.Initialize(state, level);
        }
    }

    public void HighlightPotentialPlacement(Vector2Int position, StickData stick)
    {
        placementManager.HighlightPotentialPlacement(position, stick);
    }

    public bool PlaceStick(Vector2Int position, StickData stick)
    {
        if (!GameManager.Instance.IsPlaying())
            return false;

        return placementManager.PlaceStick(position, stick);
    }

    public void ClearHighlights()
    {
        placementManager.ClearHighlights();
    }

    public bool CanPlaceStick(Vector2Int position, StickData stick)
    {
        return validator.CanPlaceStick(position, stick, CalculateStickCenterOffset(stick));
    }

    public bool CanStickBePlacedAnywhere(StickData stick)
    {
        return validator.CanStickBePlacedAnywhere(stick);
    }

    public Vector2Int WorldToGridPosition(Vector2 worldPosition)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);
        return new Vector2Int(
            Mathf.RoundToInt(localPos.x + (width - 1) * 0.5f),
            Mathf.RoundToInt(localPos.y + (height - 1) * 0.5f)
        );
    }

    private Vector2 CalculateStickCenterOffset(StickData stick)
    {
        Vector2 total = Vector2.zero;
        foreach (var segment in stick.segments)
        {
            total += (Vector2)(segment.start + segment.end) * 0.5f;
        }
        return total / stick.segments.Length;
    }

    public float GetWorldWidth() => width - 1;
    public float GetWorldHeight() => height - 1;
    public Color GetThemeColor() => themeColor;

    public float GetCurrentScale()
    {
        return layoutController.GetCurrentScale();
    }

    public bool IsValidGridPosition(Vector2Int pos)
    {
        return state.IsWithinBounds(pos);
    }

    public bool IsConnectionOccupied(Vector2Int start, Vector2Int end)
    {
        return validator.IsConnectionOccupied(start, end);
    }

    public Cell[,] Cells => state.Cells;
}
