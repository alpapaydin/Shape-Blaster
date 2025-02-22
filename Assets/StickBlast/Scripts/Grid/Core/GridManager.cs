using UnityEngine;
using StickBlast.Grid;
using StickBlast.Sticks;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int width = 6;
    [SerializeField] private int height = 6;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private GameObject connectionPrefab;
    [SerializeField] private GameObject cellPrefab;
    
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    
    [Header("Colors")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color themeColor = Color.green;
    [SerializeField] private Color emptyColor = Color.blue;

    [Header("Debug")]
    [SerializeField] private bool enableDebugPattern = false;

    // Core components
    private GridState state;
    private GridInitializer initializer;
    private GridLayoutController layoutController;
    private ConnectionManager connectionManager;
    private CellManager cellManager;
    private BlastManager blastManager;
    private PlacementManager placementManager;
    private GridValidator validator;

    private void Awake()
    {
        InitializeManagers();
    }

    private void Start()
    {
        initializer.InitializeGrid();
        layoutController.UpdateLayout();

        if (enableDebugPattern)
        {
            var debugger = new GridDebugger(state, connectionManager, cellManager);
            debugger.FillTestPattern();
        }

        GameManager.Instance.StartGame();
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

    public void CheckWinCondition()
    {
        if (false)
        {
            GameManager.Instance.WinGame();
        }
    }
}
