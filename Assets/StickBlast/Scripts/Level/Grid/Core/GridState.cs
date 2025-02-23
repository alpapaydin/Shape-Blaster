using UnityEngine;

namespace StickBlast.Grid
{
    public class GridState
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Dot[,] Dots { get; private set; }
        public Connection[,] HorizontalConnections { get; private set; }
        public Connection[,] VerticalConnections { get; private set; }
        public Cell[,] Cells { get; private set; }
        public Color ThemeColor { get; private set; }
        public Color EmptyColor { get; private set; }

        public GridState(int width, int height, Color themeColor, Color emptyColor)
        {
            Width = width;
            Height = height;
            ThemeColor = themeColor;
            EmptyColor = emptyColor;
            InitializeArrays();
        }

        private void InitializeArrays()
        {
            Dots = new Dot[Width, Height];
            HorizontalConnections = new Connection[Width, Height - 1];
            VerticalConnections = new Connection[Width - 1, Height];
            Cells = new Cell[Width - 1, Height - 1];
        }

        public Vector2 GetGridOffset()
        {
            float offsetX = -(Width - 1) * 0.5f;
            float offsetY = -(Height - 1) * 0.5f;
            return new Vector2(offsetX, offsetY);
        }

        public bool IsWithinBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < Width && pos.y >= 0 && pos.y < Height;
        }

        public bool IsCellWithinBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < Width - 1 && pos.y >= 0 && pos.y < Height - 1;
        }
    }
}
