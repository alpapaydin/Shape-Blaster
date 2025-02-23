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
            float gridWidth = state.Width - 1;
            float gridHeight = state.Height - 1;
            
            float screenAspect = (float)Screen.width / Screen.height;
            float cameraHeight = mainCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * screenAspect;

            float horizontalPaddingPercent = 0.1f;
            float verticalPaddingPercent = 0.20f;

            float horizontalPadding = cameraWidth * horizontalPaddingPercent;
            float verticalPadding = cameraHeight * verticalPaddingPercent;

            float availableWidth = cameraWidth - (horizontalPadding * 2f);
            float availableHeight = cameraHeight - (verticalPadding * 2f);

            float scale = availableWidth / gridWidth;

            float resultingHeight = gridHeight * scale;
            if (resultingHeight > availableHeight)
            {
                scale = availableHeight / gridHeight;
            }

            return scale;
        }

        private Vector3 CalculateGridPosition()
        {
            return Vector3.zero;
        }

        public float GetCurrentScale() => currentScale;
    }
}
