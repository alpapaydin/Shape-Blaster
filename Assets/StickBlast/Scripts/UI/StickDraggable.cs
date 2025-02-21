using UnityEngine;
using UnityEngine.EventSystems;
using StickBlast.Sticks;
using UnityEngine.UI;
using System.Collections.Generic;

public class StickDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Canvas canvas;
    private RectTransform rectTransform;
    private StickData stickData;
    private GridManager gridManager;
    private Vector2 originalPosition;
    private Vector2Int currentGridPosition;
    private StickSpawner spawner;
    private int slotIndex;
    
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
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
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

    private void UpdateGridHighlight()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = gridManager.WorldToGridPosition(mousePos);
        
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

        GameObject dragArea = new GameObject("DragArea", typeof(RectTransform), typeof(Image));
        dragArea.transform.SetParent(transform, false);
        RectTransform dragRect = dragArea.GetComponent<RectTransform>();
        dragRect.anchorMin = Vector2.zero;
        dragRect.anchorMax = Vector2.one;
        dragRect.offsetMin = Vector2.zero;
        dragRect.offsetMax = Vector2.zero;
        
        Image dragImage = dragArea.GetComponent<Image>();
        dragImage.color = new Color(1, 1, 1, 0);
        
        GameObject container = new GameObject("PartsContainer", typeof(RectTransform));
        container.transform.SetParent(transform, false);
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);

        Bounds initialBounds = new Bounds();
        List<(RectTransform rect, Vector2 position)> parts = new List<(RectTransform, Vector2)>();
        
        foreach (var part in stickData.definition.stickParts)
        {
            if (part.sprite == null) continue;
            
            GameObject partObj = new GameObject("StickPart", typeof(Image));
            partObj.transform.SetParent(container.transform, false);
            
            Image image = partObj.GetComponent<Image>();
            image.sprite = part.sprite;
            image.SetNativeSize();
            image.color = gridManager.GetThemeColor();

            RectTransform rect = partObj.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = part.relativePosition;
            
            parts.Add((rect, part.relativePosition));
            
            Vector2 spriteSize = rect.sizeDelta;
            Vector2 halfSize = spriteSize * 0.5f;
            Vector2[] corners = new Vector2[]
            {
                part.relativePosition + new Vector2(-halfSize.x, -halfSize.y),
                part.relativePosition + new Vector2(-halfSize.x, halfSize.y),
                part.relativePosition + new Vector2(halfSize.x, -halfSize.y),
                part.relativePosition + new Vector2(halfSize.x, halfSize.y)
            };

            foreach (var corner in corners)
            {
                initialBounds.Encapsulate(new Vector3(corner.x, corner.y, 0));
            }
        }

        Vector2 centerOffset = initialBounds.center;
        foreach (var part in parts)
        {
            part.rect.anchoredPosition = part.position - centerOffset;
        }

        float angle = (int)stickData.orientation;
        container.transform.localRotation = Quaternion.Euler(0, 0, -angle);

        Bounds finalBounds = new Bounds();
        foreach (Transform child in container.transform)
        {
            RectTransform childRect = child.GetComponent<RectTransform>();
            Vector3[] corners = new Vector3[4];
            childRect.GetWorldCorners(corners);
            
            foreach (Vector3 corner in corners)
            {
                finalBounds.Encapsulate(transform.InverseTransformPoint(corner));
            }
        }

        float size = Mathf.Max(finalBounds.size.x, finalBounds.size.y);
        rectTransform.sizeDelta = new Vector2(size, size);
        containerRect.sizeDelta = new Vector2(size, size);
        containerRect.anchoredPosition = Vector2.zero;
    }

    public static Vector2 CalculateStickBounds(StickDefinition stick)
    {
        if (stick.stickParts == null || stick.stickParts.Length == 0)
            return Vector2.one * 100f;

        Bounds maxBounds = new Bounds();
        
        foreach (StickOrientation orientation in System.Enum.GetValues(typeof(StickOrientation)))
        {
            Bounds orientationBounds = new Bounds();
            float angle = -(int)orientation * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            foreach (var part in stick.stickParts)
            {
                if (part.sprite == null) continue;

                Vector2 size = part.sprite.bounds.size;
                Vector2 pos = part.relativePosition;
                
                Vector2 rotatedPos = new Vector2(
                    pos.x * cos - pos.y * sin,
                    pos.x * sin + pos.y * cos
                );

                Vector2[] corners = new Vector2[]
                {
                    rotatedPos + new Vector2(-size.x/2, -size.y/2),
                    rotatedPos + new Vector2(-size.x/2, size.y/2),
                    rotatedPos + new Vector2(size.x/2, -size.y/2),
                    rotatedPos + new Vector2(size.x/2, size.y/2)
                };

                foreach (var corner in corners)
                {
                    orientationBounds.Encapsulate(new Vector3(corner.x, corner.y, 0));
                }
            }

            maxBounds.Encapsulate(orientationBounds);
        }

        return maxBounds.size;
    }

    public static float CalculateMaxWidth(StickDefinition[] sticks)
    {
        float maxWidth = 0;
        foreach (var stick in sticks)
        {
            float stickWidth = CalculateStickBounds(stick).x;
            maxWidth = Mathf.Max(maxWidth, stickWidth);
        }
        return maxWidth;
    }
}
