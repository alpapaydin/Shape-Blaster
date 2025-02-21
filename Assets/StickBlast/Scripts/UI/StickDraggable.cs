using UnityEngine;
using UnityEngine.EventSystems;
using StickBlast.Sticks;
using UnityEngine.UI;
using System.Collections.Generic;

public class StickDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private float dragOffset = 100f;
    [SerializeField] public float visualScale = 1f;
    private RectTransform rectTransform;
    private StickData stickData;
    private GridManager gridManager;
    private Vector2 originalPosition;
    private Vector2Int currentGridPosition;
    private StickSpawner spawner;
    private int slotIndex;
    private Vector2 dragStartOffset;
    private RectTransform dragArea;
    private Vector2 gridOffset;

    public StickData StickData => stickData;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        gridManager = FindObjectOfType<GridManager>();
        spawner = GetComponentInParent<StickSpawner>();
    }

    public void Initialize(StickData data)
    {
        stickData = data;
        UpdateStickVisual();
    }

    public void SetCanvas(Canvas canvas)
    {
        this.canvas = canvas;
    }

    public void SetSlotIndex(int index)
    {
        slotIndex = index;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        
        bool canBePlaced = gridManager.CanStickBePlacedAnywhere(stickData);
        if (!canBePlaced && spawner != null)
        {
            spawner.CheckGameOver(this);
        }
        
        Vector2 touchPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform.parent, 
            eventData.position, 
            eventData.pressEventCamera, 
            out touchPos
        );
        
        rectTransform.anchoredPosition = touchPos + Vector2.up * dragOffset;
        dragStartOffset = rectTransform.anchoredPosition - touchPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 touchPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform.parent, 
            eventData.position, 
            eventData.pressEventCamera, 
            out touchPos
        );
        rectTransform.anchoredPosition = touchPos + dragStartOffset;
        
        UpdateGridHighlight();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (CanPlaceAtCurrentPosition())
        {
            PlaceStick();
        }
        else
        {
            rectTransform.anchoredPosition = originalPosition;
        }
        gridManager.ClearHighlights();
    }

    private bool IsSinglePartIShape()
    {
        return stickData.segments.Length == 1 && 
               (stickData.segments[0].start.x == stickData.segments[0].end.x || 
                stickData.segments[0].start.y == stickData.segments[0].end.y);
    }

    private void UpdateGridHighlight()
    {
        Vector3[] corners = new Vector3[4];
        dragArea.GetWorldCorners(corners);
        Vector3 dragAreaCenter = (corners[0] + corners[2]) * 0.5f;
        
        float worldHalfSize = (dragArea.sizeDelta.x * 0.5f) * dragArea.lossyScale.x;
        float angle = -(int)stickData.orientation * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        Vector2 cornerOffset = IsSinglePartIShape() 
            ? new Vector2(0, -worldHalfSize)
            : new Vector2(-worldHalfSize, -worldHalfSize);

        Vector2 rotatedCornerOffset = new Vector2(
            cornerOffset.x * cos - cornerOffset.y * sin,
            cornerOffset.x * sin + cornerOffset.y * cos
        );
        
        Vector3 originPoint = dragAreaCenter + (Vector3)rotatedCornerOffset;
        
        Vector2 rotatedGridOffset = new Vector2(
            gridOffset.x * cos - gridOffset.y * sin,
            gridOffset.x * sin + gridOffset.y * cos
        );
        
        Vector3 adjustedPosition = originPoint + (Vector3)(rotatedGridOffset * dragArea.lossyScale.x);
        
        Vector2Int gridPos = gridManager.WorldToGridPosition(adjustedPosition);
        
        if (gridPos != currentGridPosition)
        {
            currentGridPosition = gridPos;
            gridManager.HighlightPotentialPlacement(gridPos, stickData);
        }
    }

    private bool CanPlaceAtCurrentPosition()
    {
        return gridManager.CanPlaceStick(currentGridPosition, stickData);
    }

    private void PlaceStick()
    {
        if (gridManager.PlaceStick(currentGridPosition, stickData))
        {
            if (spawner != null)
            {
                spawner.OnStickPlaced(slotIndex);
            }
            Destroy(gameObject);
        }
        else
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    private void UpdateStickVisual()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        var container = CreateContainer("PartsContainer");
        dragArea = CreateContainer("DragArea");
        
        var dragImage = dragArea.gameObject.AddComponent<Image>();
        dragImage.color = new Color(1, 1, 1, 0);
        SetRectToFill(dragArea);

        Bounds bounds = CreateStickParts(container);

        float size = Mathf.Max(bounds.size.x, bounds.size.y) * visualScale;
        rectTransform.sizeDelta = new Vector2(size, size);
        container.sizeDelta = new Vector2(size, size);
        dragArea.sizeDelta = new Vector2(size / visualScale, size / visualScale);

        float halfGridUnit = 0.5f;
        gridOffset = new Vector2(-halfGridUnit, -halfGridUnit);
    }

    private RectTransform CreateContainer(string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        return rect;
    }

    private void SetRectToFill(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private Bounds CreateStickParts(RectTransform container)
    {
        var bounds = new Bounds();
        
        foreach (var part in stickData.definition.stickParts)
        {
            if (part.sprite == null) continue;

            var partObj = new GameObject("StickPart", typeof(Image));
            partObj.transform.SetParent(container, false);
            
            var image = partObj.GetComponent<Image>();
            image.sprite = part.sprite;
            image.SetNativeSize();
            image.color = gridManager.GetThemeColor();

            var rect = partObj.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta *= visualScale;
            rect.anchoredPosition = part.relativePosition * visualScale;

            Vector2 size = rect.sizeDelta;
            Vector2 pos = rect.anchoredPosition;
            bounds.Encapsulate(new Vector3(pos.x + size.x/2, pos.y + size.y/2, 0));
            bounds.Encapsulate(new Vector3(pos.x - size.x/2, pos.y - size.y/2, 0));
        }

        foreach (RectTransform child in container)
        {
            child.anchoredPosition -= (Vector2)bounds.center;
        }

        container.localRotation = Quaternion.Euler(0, 0, -(int)stickData.orientation);

        return bounds;
    }

    public static Vector2 CalculateStickBounds(StickDefinition stick, float scale = 1f)
    {
        if (stick.stickParts == null || stick.stickParts.Length == 0)
            return Vector2.one * 100f * scale;

        var bounds = new Bounds();
        foreach (var part in stick.stickParts)
        {
            if (part.sprite == null) continue;
            bounds.Encapsulate(new Vector3(
                part.relativePosition.x + part.sprite.bounds.size.x/2,
                part.relativePosition.y + part.sprite.bounds.size.y/2,
                0
            ));
            bounds.Encapsulate(new Vector3(
                part.relativePosition.x - part.sprite.bounds.size.x/2,
                part.relativePosition.y - part.sprite.bounds.size.y/2,
                0
            ));
        }
        return bounds.size * scale;
    }

    public static float CalculateMaxWidth(StickDefinition[] sticks, float scale = 1f)
    {
        float maxWidth = 0;
        foreach (var stick in sticks)
        {
            float stickWidth = CalculateStickBounds(stick, scale).x;
            maxWidth = Mathf.Max(maxWidth, stickWidth);
        }
        return maxWidth;
    }
}
