using UnityEngine;

public class Dot : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isOccupied;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = "Grid";
            spriteRenderer.sortingOrder = 2;
        }
    }

    private void Start()
    {
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void Initialize(Vector2Int position)
    {
        GridPosition = position;
        isOccupied = false;
    }

    public void Highlight(Color color)
    {
        if (spriteRenderer != null && !isOccupied)
        {
            spriteRenderer.color = color;
        }
    }

    public void ClearHighlight()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
    }

    public void SetBaseColor(Color color)
    {
        if (spriteRenderer != null)
        {
            originalColor = color;
            spriteRenderer.color = color;
        }
    }
}
