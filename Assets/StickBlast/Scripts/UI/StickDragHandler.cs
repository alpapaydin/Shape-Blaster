using UnityEngine;
using UnityEngine.EventSystems;
using StickBlast.Sticks;

public class StickDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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
}
