using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Connection : MonoBehaviour
{
    public Dot StartDot { get; private set; }
    public Dot EndDot { get; private set; }
    public bool IsOccupied { get; private set; }

    private LineRenderer lineRenderer;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private SpriteRenderer barSprite;
    [SerializeField] private Color normalColor = Color.gray;
    [SerializeField] private Color occupiedColor = Color.blue;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetupLine();
        if (barSprite != null)
        {
            barSprite.color = Color.clear;
        }
    }

    private void SetupLine()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = normalColor;
        lineRenderer.endColor = normalColor;
        
        lineRenderer.sortingLayerName = "Grid";
        lineRenderer.sortingOrder = 0;
    }

    private void Update()
    {
        if (StartDot != null && EndDot != null)
        {
            lineRenderer.SetPosition(0, StartDot.transform.position);
            lineRenderer.SetPosition(1, EndDot.transform.position);
        }
    }

    public void Initialize(Dot start, Dot end)
    {
        StartDot = start;
        EndDot = end;
        IsOccupied = false;
        
        lineRenderer.SetPosition(0, start.transform.position);
        lineRenderer.SetPosition(1, end.transform.position);

        if (barSprite != null)
        {
            bool isHorizontal = Mathf.Approximately(start.transform.position.y, end.transform.position.y);
            if (isHorizontal)
            {
                barSprite.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
            barSprite.color = Color.clear;
        }
    }

    public void Occupy()
    {
        IsOccupied = true;
        lineRenderer.startColor = Color.clear;
        lineRenderer.endColor = Color.clear;
        if (barSprite != null)
        {
            barSprite.color = occupiedColor;
        }
    }

    public void Highlight(Color highlightColor)
    {
        if (!IsOccupied)
        {
            lineRenderer.startColor = Color.clear;
            lineRenderer.endColor = Color.clear;
            if (barSprite != null)
            {
                barSprite.color = highlightColor;
            }
        }
    }

    public void UpdateWidth(float scale)
    {
        lineRenderer.startWidth = lineWidth * scale;
        lineRenderer.endWidth = lineWidth * scale;
    }

    public void SetColors(Color normal, Color occupied)
    {
        normalColor = normal;
        occupiedColor = occupied;
        
        if (!IsOccupied)
        {
            lineRenderer.startColor = normal;
            lineRenderer.endColor = normal;
            if (barSprite != null)
            {
                barSprite.color = Color.clear;
            }
        }
    }

    public void Reset()
    {
        IsOccupied = false;
        SetColors(normalColor, occupiedColor);
    }
}
