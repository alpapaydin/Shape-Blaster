using UnityEngine;
using UnityEngine.UI;
using StickBlast.Sticks;

public class StickSpawner : MonoBehaviour
{
    [SerializeField] private GameObject stickUIPrefab;
    [SerializeField] private Transform stickContainer;
    [SerializeField] private int maxSticks = 3;
    [SerializeField] private float minSpacing = 100f;
    [SerializeField] private float paddingMultiplier = 1.2f;
    [SerializeField] private Canvas canvas;
    [SerializeField] private StickDefinition[] availableSticks;
    [SerializeField] private float containerMargin = 20f;
    [SerializeField] private float stickScale = 1f;

    private Vector2[] slotPositions;
    private bool[] occupiedSlots;
    private GridManager gridManager;
    private Transform spawnedSticksContainer;

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        slotPositions = new Vector2[maxSticks];
        occupiedSlots = new bool[maxSticks];
        
        spawnedSticksContainer = new GameObject("SpawnedSticks", typeof(RectTransform)).transform;
        spawnedSticksContainer.SetParent(stickContainer, false);
        
        RectTransform containerRect = spawnedSticksContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = new Vector2(containerMargin, containerMargin);
        containerRect.offsetMax = new Vector2(-containerMargin, -containerMargin);
        
        CalculateSlotPositions();
        SpawnInitialSticks();
    }

    private void CalculateSlotPositions()
    {
        float maxWidth = StickDraggable.CalculateMaxWidth(availableSticks, stickScale);
        float spacing = Mathf.Max(minSpacing, maxWidth * paddingMultiplier);
        
        float totalWidth = spacing * (maxSticks - 1);
        float startX = -totalWidth * 0.5f;

        for (int i = 0; i < maxSticks; i++)
        {
            slotPositions[i] = new Vector2(startX + (spacing * i), 0);
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
            if (!occupiedSlots[i])
            {
                slotIndex = i;
                break;
            }
        }

        if (slotIndex == -1) return;

        StickData stickData = GetRandomStickData();
        GameObject stickUI = Instantiate(stickUIPrefab, spawnedSticksContainer);
        
        RectTransform rect = stickUI.GetComponent<RectTransform>();
        rect.anchoredPosition = slotPositions[slotIndex];
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        StickDraggable dragHandler = stickUI.GetComponent<StickDraggable>();
        dragHandler.visualScale = stickScale;
        dragHandler.Initialize(stickData);
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
