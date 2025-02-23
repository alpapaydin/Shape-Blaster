using UnityEngine;

namespace StickBlast.Grid
{
    public class GridLayoutController
    {
        private readonly Transform gridTransform;
        private readonly GridState state;
        private readonly ConnectionManager connectionManager;
        private readonly Camera mainCamera;
        private readonly float padding;
        private float currentScale = 1f;

        public GridLayoutController(Transform gridTransform, GridState state, 
                                  ConnectionManager connectionManager, Camera mainCamera, float padding = 1f)
        {
            this.gridTransform = gridTransform;
            this.state = state;
            this.connectionManager = connectionManager;
            this.mainCamera = mainCamera;
            this.padding = padding;
        }

        public void UpdateLayout()
        {
            float gridScale = CalculateGridScale();
            Vector3 gridPosition = CalculateGridPosition();
            
            gridTransform.position = gridPosition;
            gridTransform.localScale = Vector3.one * gridScale;
            currentScale = gridScale;
            
            connectionManager.UpdateConnectionWidths(gridScale);
        }

        private float CalculateGridScale()
        {
            float worldWidth = state.Width - 1;
            float worldHeight = state.Height - 1;
            
            float screenAspect = (float)Screen.width / Screen.height;
            float cameraHeight = mainCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * screenAspect;

            float availableWidth = cameraWidth - (padding * 2f);
            float availableHeight = cameraHeight - (padding * 2f);

            float scaleX = availableWidth / worldWidth;
            float scaleY = availableHeight / worldHeight;

            return Mathf.Min(scaleX, scaleY);
        }

        private Vector3 CalculateGridPosition()
        {
            return Vector3.zero;
        }

        public float GetCurrentScale() => currentScale;
    }
}
