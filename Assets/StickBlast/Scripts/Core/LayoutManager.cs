using UnityEngine;

public class LayoutManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float padding = 1f;
    [SerializeField] private float targetAspectRatio = 0.75f;
    
    void Start()
    {
        AdjustCamera();
    }

    void OnRectTransformDimensionsChange()
    {
        AdjustCamera();
    }

    private void AdjustCamera()
    {
        float screenAspect = (float)Screen.width / Screen.height;
        
        if (screenAspect < targetAspectRatio)
        {
            mainCamera.orthographicSize = 5f * (targetAspectRatio / screenAspect);
        }
        else
        {
            mainCamera.orthographicSize = 5f;
        }
    }

    public Vector3 GetGridPosition(int width, int height)
    {
        return Vector3.zero;
    }

    public float GetGridScale(int width, int height)
    {
        GridManager gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null) return 1f;

        float worldWidth = gridManager.GetWorldWidth();
        float worldHeight = gridManager.GetWorldHeight();
        
        float screenAspect = (float)Screen.width / Screen.height;
        float cameraHeight = mainCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * screenAspect;

        float availableWidth = cameraWidth - (padding * 2f);
        float availableHeight = cameraHeight - (padding * 2f);

        float scaleX = availableWidth / worldWidth;
        float scaleY = availableHeight / worldHeight;

        return Mathf.Min(scaleX, scaleY);
    }
}
