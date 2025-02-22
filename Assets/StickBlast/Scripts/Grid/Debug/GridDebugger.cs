using UnityEngine;

namespace StickBlast.Grid
{
    public class GridDebugger
    {
        private readonly GridState state;
        private readonly ConnectionManager connectionManager;
        private readonly CellManager cellManager;

        public GridDebugger(GridState state, ConnectionManager connectionManager, CellManager cellManager)
        {
            this.state = state;
            this.connectionManager = connectionManager;
            this.cellManager = cellManager;
        }

        public void FillTestPattern()
        {
            for (int y = 2; y <= 4; y++)
            {
                FillConnection(new Vector2Int(0, y), new Vector2Int(1, y));
                FillConnection(new Vector2Int(1, y), new Vector2Int(2, y));
                FillConnection(new Vector2Int(2, y), new Vector2Int(3, y));
                FillConnection(new Vector2Int(4, y), new Vector2Int(5, y));
            }

            FillConnection(new Vector2Int(0, 2), new Vector2Int(0, 3));
            FillConnection(new Vector2Int(1, 2), new Vector2Int(1, 3));
            FillConnection(new Vector2Int(2, 2), new Vector2Int(2, 3));
            FillConnection(new Vector2Int(3, 2), new Vector2Int(3, 3));
            FillConnection(new Vector2Int(4, 2), new Vector2Int(4, 3));
            FillConnection(new Vector2Int(5, 2), new Vector2Int(5, 3));

            FillConnection(new Vector2Int(0, 3), new Vector2Int(0, 4));
            FillConnection(new Vector2Int(1, 3), new Vector2Int(1, 4));
            FillConnection(new Vector2Int(2, 3), new Vector2Int(2, 4));
            FillConnection(new Vector2Int(3, 3), new Vector2Int(3, 4));
            FillConnection(new Vector2Int(4, 3), new Vector2Int(4, 4));
            FillConnection(new Vector2Int(5, 3), new Vector2Int(5, 4));

            for (int x = 0; x < state.Width - 1; x++)
            {
                cellManager.CheckAndCompleteCell(new Vector2Int(x, 2));
                cellManager.CheckAndCompleteCell(new Vector2Int(x, 3));
            }
        }

        private void FillConnection(Vector2Int start, Vector2Int end)
        {
            Connection conn = connectionManager.GetConnection(start, end);
            if (conn != null)
            {
                conn.Occupy();
                state.Dots[start.x, start.y].SetBaseColor(state.ThemeColor);
                state.Dots[start.x, start.y].SetOccupied(true);
                state.Dots[end.x, end.y].SetBaseColor(state.ThemeColor);
                state.Dots[end.x, end.y].SetOccupied(true);
            }
        }
    }
}
