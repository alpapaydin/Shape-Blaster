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
                for (int x = 0; x < state.Width - 1; x++)
                {
                    if (x == 3 && y == 3) continue;
                    FillConnection(new Vector2Int(x, y), new Vector2Int(x + 1, y));
                }
            }

            for (int x = 0; x <= 5; x++)
            {
                for (int y = 2; y <= 3; y++)
                {
                    FillConnection(new Vector2Int(x, y), new Vector2Int(x, y + 1));
                }
            }

            for (int x = 3; x <= 4; x++)
            {
                for (int y = 0; y <= 4; y++)
                {
                    if (y == 2) continue;
                    FillConnection(new Vector2Int(x, y), new Vector2Int(x, y + 1));
                }
            }

            for (int y = 0; y <= 5; y += 5)
            {
                FillConnection(new Vector2Int(3, y), new Vector2Int(4, y));
            }
            FillConnection(new Vector2Int(3, 1), new Vector2Int(4, 1));

            for (int x = 0; x < state.Width - 1; x++)
            {
                cellManager.CheckAndCompleteCell(new Vector2Int(x, 2));
                cellManager.CheckAndCompleteCell(new Vector2Int(x, 3));
            }
            for (int y = 0; y < state.Height - 1; y++)
            {
                cellManager.CheckAndCompleteCell(new Vector2Int(3, y));
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
