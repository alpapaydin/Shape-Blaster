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

    private Vector2[] slotPositions;
    private bool[] occupiedSlots;
    private GridManager gridManager;

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        slotPositions = new Vector2[maxSticks];
        occupiedSlots = new bool[maxSticks];
        CalculateSlotPositions();
        SpawnInitialSticks();
    }

    private void CalculateSlotPositions()
    {
        float maxWidth = 0;
        foreach (var stick in availableSticks)
        {
            float stickWidth = CalculateStickBounds(stick).x;
            maxWidth = Mathf.Max(maxWidth, stickWidth);
        }

        float spacing = Mathf.Max(minSpacing, maxWidth * paddingMultiplier);
        
        float totalWidth = spacing * (maxSticks - 1);
        float startX = -totalWidth * 0.5f;

        for (int i = 0; i < maxSticks; i++)
        {
            slotPositions[i] = new Vector2(startX + (spacing * i), 0);
        }
    }

    private Vector2 CalculateStickBounds(StickDefinition stick)
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
        GameObject stickUI = Instantiate(stickUIPrefab, stickContainer);
        
        RectTransform rect = stickUI.GetComponent<RectTransform>();
        rect.anchoredPosition = slotPositions[slotIndex];
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        StickDragHandler dragHandler = stickUI.GetComponent<StickDragHandler>();
        dragHandler.Initialize(stickData);
        dragHandler.SetCanvas(canvas);
        dragHandler.SetSlotIndex(slotIndex);

        UpdateStickVisual(stickUI, stickData);

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

    private void UpdateStickVisual(GameObject stickUI, StickData stickData)
    {
        foreach (Transform child in stickUI.transform)
        {
            Destroy(child.gameObject);
        }

        GameObject container = new GameObject("PartsContainer", typeof(RectTransform));
        container.transform.SetParent(stickUI.transform, false);
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);

        Bounds bounds = new Bounds();
        foreach (var part in stickData.definition.stickParts)
        {
            if (part.sprite == null) continue;
            bounds.Encapsulate(new Vector3(part.relativePosition.x, part.relativePosition.y, 0));
        }

        foreach (var part in stickData.definition.stickParts)
        {
            GameObject partObj = new GameObject("StickPart", typeof(Image));
            partObj.transform.SetParent(container.transform, false);
            
            Image image = partObj.GetComponent<Image>();
            image.sprite = part.sprite;
            image.SetNativeSize();
            image.color = gridManager.GetThemeColor();

            RectTransform rect = partObj.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0.5f);
            
            Vector2 centeredPos = part.relativePosition - (Vector2)bounds.center;
            rect.anchoredPosition = centeredPos;
        }

        float angle = (int)stickData.orientation;
        container.transform.localRotation = Quaternion.Euler(0, 0, -angle);
    }

    public void OnStickPlaced(int slotIndex)
    {
        occupiedSlots[slotIndex] = false;
        SpawnRandomStick();
    }
}
