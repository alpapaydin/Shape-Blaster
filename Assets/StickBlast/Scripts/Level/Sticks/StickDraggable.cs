using UnityEngine;
using UnityEngine.EventSystems;
using StickBlast.Sticks;
using UnityEngine.UI;
using System.Collections.Generic;

public class StickDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Vector2 touchOffset = new Vector2(0, 250);
    public float previewScaleMultiplier = 0.5f;
    public float dragScaleMultiplier = 1f;
    private float currentScaleMultiplier;
    private RectTransform rectTransform;
    private StickData stickData;
    private GridManager gridManager;
    private Vector2Int currentGridPosition;
    private StickSpawner spawner;
    private int slotIndex;
    private RectTransform dragArea;
    private Vector2 gridOffset;
    private float baseScale;
    private bool isDragging = false;
    private Camera dragCamera;
    private RectTransform partsContainerRect;
    private Vector2 weightedCenterCache;
    private Vector2 lastHighlightPosition;
    [SerializeField] private bool useVibration = true;
    [SerializeField] private Material stickMaterial;
    private Vector2 slotPosition;
    private Vector2 lastDragPosition;
    private float minDragDistance = 1f;

    public StickData StickData => stickData;
    public bool ShouldCancelDrag;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        gridManager = FindObjectOfType<GridManager>();
        spawner = GetComponentInParent<StickSpawner>();
        dragCamera = Camera.main;
    }

    public void Initialize(StickData data, Vector2 slotSize)
    {
        stickData = data;
        rectTransform.sizeDelta = slotSize;
        
        var tempContainer = CreateContainer("TempContainer");
        Bounds bounds = CreateStickParts(tempContainer, 1f);
        Destroy(tempContainer.gameObject);
        
        float padding = 0.9f;
        float scaleX = (slotSize.x * padding) / bounds.size.x;
        float scaleY = (slotSize.y * padding) / bounds.size.y;
        baseScale = Mathf.Min(scaleX, scaleY);
        
        currentScaleMultiplier = baseScale * previewScaleMultiplier;
        UpdateStickVisual();
    }

    public void SetCanvas(Canvas canvas)
    {
        this.canvas = canvas;
    }

    public void SetSlotIndex(int index)
    {
        slotIndex = index;
        slotPosition = rectTransform.anchoredPosition;
        
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = slotPosition;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isDragging) return;
        SoundManager.Instance.PlaySound("pop");
        if (useVibration)
        {
            Vibration.VibrateMedium();
        }
        isDragging = true;
        currentScaleMultiplier = gridManager.GetCurrentScale() * dragScaleMultiplier * stickData.definition.scaleMultiplier;
        UpdateStickVisual();
        
        if (spawner != null)
        {
            spawner.CheckGameOver(this);
        }

        UpdateDragPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        if (ShouldCancelDrag)
        {
            CancelDrag(eventData);
            return;
        }
        Vector2 touchPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform.parent,
            eventData.position,
            dragCamera,
            out touchPos))
        {
            rectTransform.anchoredPosition = touchPos - GetRotatedOffset() + touchOffset;
            
            if (Vector2.Distance(eventData.position, lastDragPosition) > minDragDistance)
            {
                UpdateGridHighlight();
                lastDragPosition = eventData.position;
            }
        }
    }

    private void CancelDrag(PointerEventData eventData)
    {
        OnEndDrag(eventData);
        eventData.pointerDrag = null;
        eventData.Use();
    }

    private void UpdateDragPosition(PointerEventData eventData)
    {
        Vector2 touchPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform.parent, 
            eventData.position, 
            eventData.pressEventCamera, 
            out touchPos
        );

        var partsContainer = transform.Find("PartsContainer");
        if (partsContainer != null)
        {
            var containerRect = partsContainer.GetComponent<RectTransform>();
            var weightedCenter = CalculatePartsWeightedCenter();
            
            float angle = -(int)stickData.orientation;
            Vector2 rotatedOffset = Quaternion.Euler(0, 0, angle) * weightedCenter;
            
            rectTransform.anchoredPosition = touchPos - rotatedOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (CanPlaceAtCurrentPosition())
        {
            SoundManager.Instance.PlaySound("click");
            PlaceStick();
        }
        else
        {
            isDragging = false;
            ReturnToSlot();
        }
        
        SoundManager.Instance.StopSound("highlightBlast");
        gridManager.ClearHighlights();
    }

    private void UpdateGridHighlight()
    {
        if (partsContainerRect == null) return;

        Vector3 worldPosition = partsContainerRect.TransformPoint(weightedCenterCache);
        currentGridPosition = gridManager.WorldToGridPosition(worldPosition);
        gridManager.HighlightPotentialPlacement(currentGridPosition, stickData);
    }

    private void OnDrawGizmos()
    {
        var partsContainer = transform.Find("PartsContainer");
        if (partsContainer == null) return;

        RectTransform containerRect = partsContainer.GetComponent<RectTransform>();
        Vector2 weightedCenter = CalculatePartsWeightedCenter();
        
        Vector3 worldPosition = containerRect.TransformPoint(weightedCenter);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(worldPosition, 0.1f);
    }
    
    private bool IsValidSegment(Vector2Int start, Vector2Int end)
    {
        return gridManager.IsValidGridPosition(start) && 
               gridManager.IsValidGridPosition(end) && 
               !gridManager.IsConnectionOccupied(start, end);
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
            isDragging = false;
            ReturnToSlot();
        }
    }

    private void ReturnToSlot()
    {
        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = slotPosition;
        currentScaleMultiplier = baseScale * previewScaleMultiplier;
        UpdateStickVisual(false);
    }

    private void UpdateStickVisual(bool updatePosition = true)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        var container = CreateContainer("PartsContainer");
        dragArea = CreateContainer("DragArea");
        dragArea.gameObject.AddComponent<Image>().color = Color.clear;

        CreateStickParts(container, currentScaleMultiplier);
        SetRectToFill(dragArea);
        
        if (updatePosition)
        {
            float halfGridUnit = 0.5f;
            gridOffset = new Vector2(-halfGridUnit, -halfGridUnit);
        }
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

    private Bounds CreateStickParts(RectTransform container, float scale)
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
            if (stickMaterial != null)
            {
                image.material = stickMaterial;
            }

            var rect = partObj.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta *= scale;
            rect.anchoredPosition = part.relativePosition * scale;

            Vector2 size = rect.sizeDelta;
            Vector2 pos = rect.anchoredPosition;
            bounds.Encapsulate(new Vector3(pos.x + size.x/2, pos.y + size.y/2, 0));
            bounds.Encapsulate(new Vector3(pos.x - size.x/2, pos.y - size.y/2, 0));
        }

        if (isDragging)
        {
            Vector2 weightedCenter = CalculatePartsWeightedCenter();
            foreach (RectTransform child in container)
            {
                child.anchoredPosition -= weightedCenter;
            }
        }
        else
        {
            foreach (RectTransform child in container)
            {
                child.anchoredPosition -= (Vector2)bounds.center;
            }
        }

        container.localRotation = Quaternion.Euler(0, 0, -(int)stickData.orientation);
        return bounds;
    }

    private Vector2 CalculatePartsWeightedCenter()
    {
        var partsContainer = transform.Find("PartsContainer");
        if (partsContainer == null) return Vector2.zero;

        Vector2 totalPosition = Vector2.zero;
        float totalWeight = 0f;

        foreach (RectTransform child in partsContainer)
        {
            Vector2 size = child.sizeDelta;
            float weight = size.x * size.y;
            Vector2 pos = child.anchoredPosition;
            
            totalPosition += pos * weight;
            totalWeight += weight;
        }

        return totalWeight > 0 ? totalPosition / totalWeight : Vector2.zero;
    }

    private Vector2 GetRotatedOffset()
    {
        if (partsContainerRect == null)
        {
            var container = transform.Find("PartsContainer");
            if (container != null)
            {
                partsContainerRect = container.GetComponent<RectTransform>();
                weightedCenterCache = CalculatePartsWeightedCenter();
            }
        }
        
        float angle = -(int)stickData.orientation;
        return Quaternion.Euler(0, 0, angle) * weightedCenterCache;
    }

    public void UpdateSlotPosition(Vector2 newPosition)
    {
        slotPosition = newPosition;
        if (!isDragging)
        {
            rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = slotPosition;
        }
    }
}
