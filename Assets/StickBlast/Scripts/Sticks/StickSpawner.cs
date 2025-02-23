using UnityEngine;
using UnityEngine.UI;
using StickBlast.Sticks;
using System.Collections;

public class StickSpawner : MonoBehaviour
{
    [SerializeField] private GameObject stickUIPrefab;
    [SerializeField] private Transform stickContainer;
    [SerializeField] private int maxSticks = 3;
    [SerializeField] private Canvas canvas;
    [SerializeField] private StickDefinition[] availableSticks;
    [SerializeField] private float previewScaleMultiplier = 0.5f;
    [SerializeField] private float dragScaleMultiplier = 1f;
    [SerializeField] private float slideInDuration = 0.5f;
    [SerializeField] private float delayBetweenSticks = 0.2f;

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
        SpawnNewStickGroup();
    }

    private void SpawnNewStickGroup()
    {
        for (int i = 0; i < maxSticks; i++)
        {
            SpawnRandomStickWithAnimation(i);
        }
    }

    private void SpawnRandomStickWithAnimation(int index)
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
        rect.anchoredPosition = new Vector2(containerRect.rect.width, slotPositions[slotIndex].y);
        rect.sizeDelta = new Vector2(slotWidth, slotHeight);
        
        StickDraggable dragHandler = stickUI.GetComponent<StickDraggable>();
        dragHandler.previewScaleMultiplier = previewScaleMultiplier;
        dragHandler.dragScaleMultiplier = dragScaleMultiplier;
        dragHandler.Initialize(stickData, new Vector2(slotWidth, slotHeight));
        dragHandler.SetCanvas(canvas);
        dragHandler.SetSlotIndex(slotIndex);

        StartCoroutine(SlideStickToPosition(rect, slotPositions[slotIndex], index));
        occupiedSlots[slotIndex] = true;
    }

    private IEnumerator SlideStickToPosition(RectTransform rect, Vector2 targetPos, int index)
    {
        yield return new WaitForSeconds(index * delayBetweenSticks);
        SoundManager.Instance.PlaySound("swoosh");
        Vector2 startPos = rect.anchoredPosition;
        float elapsedTime = 0;

        while (elapsedTime < slideInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / slideInDuration;
            
            t = BounceEaseOut(t);
            
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        rect.anchoredPosition = targetPos;
    }

    private float BounceEaseOut(float t)
    {
        if (t < 1f/2.75f)
        {
            return 7.5625f * t * t;
        }
        else if (t < 2f/2.75f)
        {
            t -= 1.5f/2.75f;
            return 7.5625f * t * t + 0.75f;
        }
        else if (t < 2.5f/2.75f)
        {
            t -= 2.25f/2.75f;
            return 7.5625f * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f/2.75f;
            return 7.5625f * t * t + 0.984375f;
        }
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

    public void OnStickPlaced(int slotIndex)
    {
        occupiedSlots[slotIndex] = false;
        
        if (AreAllSlotsEmpty())
        {
            SpawnNewStickGroup();
        }
    }

    private bool AreAllSlotsEmpty()
    {
        for (int i = 0; i < maxSticks; i++)
        {
            if (occupiedSlots[i])
                return false;
        }
        return true;
    }

    public void CheckGameOver(StickDraggable currentStick)
    {
        var activeSticks = GetComponentsInChildren<StickDraggable>();
        if (gridManager.CanStickBePlacedAnywhere(currentStick.StickData))
            return;

        foreach (var stick in activeSticks)
        {
            if (stick == currentStick) 
                continue;
            
            if (gridManager.CanStickBePlacedAnywhere(stick.StickData))
                return;
        }
        Vibration.VibrateHeavy();
        GameManager.Instance.LoseGame();
    }
}
