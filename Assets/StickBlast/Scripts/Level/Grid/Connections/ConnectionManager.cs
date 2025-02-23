using UnityEngine;

namespace StickBlast.Grid
{
    public class ConnectionManager
    {
        private readonly GridState state;

        public ConnectionManager(GridState state)
        {
            this.state = state;
        }

        public Connection GetConnection(Vector2Int start, Vector2Int end)
        {
            if (start.x == end.x)
            {
                int minY = Mathf.Min(start.y, end.y);
                return state.HorizontalConnections[start.x, minY];
            }
            else
            {
                int minX = Mathf.Min(start.x, end.x);
                return state.VerticalConnections[minX, start.y];
            }
        }

        public void ResetCellConnections(Vector2Int cellPos)
        {
            Connection[] connections = GetCellConnections(cellPos);
            foreach (var conn in connections)
            {
                if (conn != null)
                {
                    conn.Reset();

                    Dot startDot = conn.StartDot;
                    Dot endDot = conn.EndDot;

                    if (!DotHasOccupiedConnections(startDot.GridPosition))
                    {
                        startDot.SetBaseColor(state.EmptyColor);
                        startDot.SetOccupied(false);
                    }

                    if (!DotHasOccupiedConnections(endDot.GridPosition))
                    {
                        endDot.SetBaseColor(state.EmptyColor);
                        endDot.SetOccupied(false);
                    }
                }
            }
        }

        public Connection[] GetCellConnections(Vector2Int cellPos)
        {
            return new Connection[]
            {
                state.VerticalConnections[cellPos.x, cellPos.y],
                state.HorizontalConnections[cellPos.x + 1, cellPos.y],
                state.VerticalConnections[cellPos.x, cellPos.y + 1],
                state.HorizontalConnections[cellPos.x, cellPos.y]
            };
        }

        public bool DotHasOccupiedConnections(Vector2Int pos)
        {
            int x = pos.x, y = pos.y;

            if (x > 0 && state.VerticalConnections[x-1, y]?.IsOccupied == true) return true;
            if (x < state.Width-1 && state.VerticalConnections[x, y]?.IsOccupied == true) return true;
            if (y > 0 && state.HorizontalConnections[x, y-1]?.IsOccupied == true) return true;
            if (y < state.Height-1 && state.HorizontalConnections[x, y]?.IsOccupied == true) return true;

            return false;
        }

        public void UpdateConnectionWidths(float scale)
        {
            for (int x = 0; x < state.Width; x++)
            {
                for (int y = 0; y < state.Height - 1; y++)
                {
                    state.HorizontalConnections[x, y]?.UpdateWidth(scale);
                }
            }

            for (int x = 0; x < state.Width - 1; x++)
            {
                for (int y = 0; y < state.Height; y++)
                {
                    state.VerticalConnections[x, y]?.UpdateWidth(scale);
                }
            }
        }
    }
}
