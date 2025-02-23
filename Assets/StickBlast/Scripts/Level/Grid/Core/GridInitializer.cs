using UnityEngine;

namespace StickBlast.Grid
{
    public class GridInitializer
    {
        private readonly GridState state;
        private readonly Transform gridParent;
        private readonly GameObject dotPrefab;
        private readonly GameObject connectionPrefab;
        private readonly GameObject cellPrefab;

        public GridInitializer(
            GridState state,
            Transform gridParent,
            GameObject dotPrefab,
            GameObject connectionPrefab,
            GameObject cellPrefab)
        {
            this.state = state;
            this.gridParent = gridParent;
            this.dotPrefab = dotPrefab;
            this.connectionPrefab = connectionPrefab;
            this.cellPrefab = cellPrefab;
        }

        public void InitializeGrid()
        {
            Vector2 offset = state.GetGridOffset();
            InitializeDots(offset);
            InitializeHorizontalConnections(offset);
            InitializeVerticalConnections(offset);
            InitializeCells(offset);
        }

        private void InitializeDots(Vector2 offset)
        {
            for (int x = 0; x < state.Width; x++)
            {
                for (int y = 0; y < state.Height; y++)
                {
                    Vector3 position = new Vector3(x + offset.x, y + offset.y, 0);
                    GameObject dotObj = Object.Instantiate(dotPrefab, position, Quaternion.identity, gridParent);
                    var dot = dotObj.GetComponent<Dot>();
                    dot.Initialize(new Vector2Int(x, y));
                    dot.SetBaseColor(state.EmptyColor);
                    state.Dots[x, y] = dot;
                }
            }
        }

        private void InitializeHorizontalConnections(Vector2 offset)
        {
            for (int x = 0; x < state.Width; x++)
            {
                for (int y = 0; y < state.Height - 1; y++)
                {
                    Vector3 position = new Vector3(x + offset.x, y + 0.5f + offset.y, 0);
                    GameObject connObj = Object.Instantiate(connectionPrefab, position, Quaternion.identity, gridParent);
                    var conn = connObj.GetComponent<Connection>();
                    conn.Initialize(state.Dots[x, y], state.Dots[x, y + 1]);
                    conn.SetColors(state.EmptyColor, state.ThemeColor);
                    state.HorizontalConnections[x, y] = conn;
                }
            }
        }

        private void InitializeVerticalConnections(Vector2 offset)
        {
            for (int x = 0; x < state.Width - 1; x++)
            {
                for (int y = 0; y < state.Height; y++)
                {
                    Vector3 position = new Vector3(x + 0.5f + offset.x, y + offset.y, 0);
                    GameObject connObj = Object.Instantiate(connectionPrefab, position, Quaternion.identity, gridParent);
                    var conn = connObj.GetComponent<Connection>();
                    conn.Initialize(state.Dots[x, y], state.Dots[x + 1, y]);
                    conn.SetColors(state.EmptyColor, state.ThemeColor);
                    state.VerticalConnections[x, y] = conn;
                }
            }
        }

        private void InitializeCells(Vector2 offset)
        {
            for (int x = 0; x < state.Width - 1; x++)
            {
                for (int y = 0; y < state.Height - 1; y++)
                {
                    Vector3 position = new Vector3(x + 0.5f + offset.x, y + 0.5f + offset.y, 0);
                    GameObject cellObj = Object.Instantiate(cellPrefab, position, Quaternion.identity, gridParent);
                    var cell = cellObj.GetComponent<Cell>();
                    cell.Initialize(state.ThemeColor, state.ThemeColor);
                    state.Cells[x, y] = cell;
                }
            }
        }
    }
}
