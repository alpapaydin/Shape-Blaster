using UnityEngine;

public class Cell : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private bool isComplete;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = "Grid";
        spriteRenderer.sortingOrder = -1;
        spriteRenderer.color = Color.clear;
    }

    public void SetComplete(Color color)
    {
        isComplete = true;
        spriteRenderer.color = color;
    }

    public bool IsComplete() => isComplete;

    public void Reset()
    {
        isComplete = false;
        spriteRenderer.color = Color.clear;
    }

    public void ShowBlastPreview(Color color)
    {
        spriteRenderer.color = new Color(color.r, color.g, color.b, 0.3f);
    }

    public void ClearBlastPreview()
    {
        spriteRenderer.color = Color.clear;
    }
}
