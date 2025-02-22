using UnityEngine;
using UnityEngine.UI;
using StickBlast.Sticks;

public class StickSpawner : MonoBehaviour
{
    [SerializeField] private GameObject stickUIPrefab;
    [SerializeField] private Transform stickContainer;
    [SerializeField] private int maxSticks = 3;
    [SerializeField] private Canvas canvas;
    [SerializeField] private StickDefinition[] availableSticks;
    [SerializeField] private float previewScaleMultiplier = 0.5f;
    [SerializeField] private float dragScaleMultiplier = 1f;

    private Vector2[] slotPositions;
    private bool[] occupiedSlots;
    private GridManager gridManager;

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        slotPositions = new Vector2[maxSticks];
        occupiedSlots = new bool[maxSticks];

        RectTransform containerRect = (RectTransform)stickContainer;
        
        CalculateSlotPositions();
        SpawnInitialSticks();
    }

    private void CalculateSlotPositions()
    {
        RectTransform containerRect = (RectTransform)stickContainer;
        float containerWidth = containerRect.rect.width;
        float slotWidth = containerWidth / maxSticks;
        float startX = -containerWidth * 0.5f + (slotWidth * 0.5f);

        for (int i = 0; i < maxSticks; i++)
        {
            slotPositions[i] = new Vector2(startX + (slotWidth * i), 0);
        }
    }

    private void SpawnInitialSticks()
    {
        for (int i = 0; i < maxSticks; i++)
        {
            SpawnRandomStick();
        }
    }

    private void SpawnRandomStick()
    {
        int slotIndex = -1;
        for (int i = 0; i < maxSticks; i++)
        {
            if (!occupiedSlots[i]) {
                slotIndex = i;
                break;
            }
        }
        if (slotIndex == -1) return;

        StickData stickData = GetRandomStickData();
        GameObject stickUI = Instantiate(stickUIPrefab, stickContainer);
        
        RectTransform containerRect = (RectTransform)stickContainer;
        float slotWidth = containerRect.rect.width / maxSticks;
        float slotHeight = containerRect.rect.height;
        
        RectTransform rect = stickUI.GetComponent<RectTransform>();
        rect.anchoredPosition = slotPositions[slotIndex];
        rect.sizeDelta = new Vector2(slotWidth, slotHeight);
        
        StickDraggable dragHandler = stickUI.GetComponent<StickDraggable>();
        dragHandler.previewScaleMultiplier = previewScaleMultiplier;
        dragHandler.dragScaleMultiplier = dragScaleMultiplier;
        dragHandler.Initialize(stickData, new Vector2(slotWidth, slotHeight));
        dragHandler.SetCanvas(canvas);
        dragHandler.SetSlotIndex(slotIndex);

        occupiedSlots[slotIndex] = true;
    }

    private StickData GetRandomStickData()
    {
        if (availableSticks == null || availableSticks.Length == 0)
            return null;

        StickDefinition randomDef = availableSticks[Random.Range(0, availableSticks.Length)];
        StickOrientation[] orientations = { 
            StickOrientation.Normal,
            StickOrientation.Right,
            StickOrientation.Bottom,
            StickOrientation.Left
        };
        StickOrientation randomOrientation = orientations[Random.Range(0, orientations.Length)];
        return StickData.Create(randomDef, randomOrientation);
    }

    public bool IsGameOver()
    {
        var activeSticks = GetComponentsInChildren<StickDraggable>();
        
        foreach (var stickDraggable in activeSticks)
        {
            if (gridManager.CanStickBePlacedAnywhere(stickDraggable.StickData))
            {
                return false;
            }
        }
        
        return true;
    }

    private void CheckGameOver()
    {
        if (IsGameOver())
        {
            Debug.Log("Game Over - No valid moves left!");
        }
    }

    public void OnStickPlaced(int slotIndex)
    {
        occupiedSlots[slotIndex] = false;
        SpawnRandomStick();
        CheckGameOver();
    }

    public void CheckGameOver(StickDraggable currentStick)
    {
        var activeSticks = GetComponentsInChildren<StickDraggable>();
        
        foreach (var stick in activeSticks)
        {
            if (stick == currentStick) continue;
            
            if (gridManager.CanStickBePlacedAnywhere(stick.StickData))
            {
                return;
            }
        }
        
        Debug.Log("Game Over - No valid moves left!");
        // OnGameOver.Invoke();
    }
}
