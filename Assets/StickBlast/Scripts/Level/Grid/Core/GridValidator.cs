using UnityEngine;
using StickBlast.Sticks;

namespace StickBlast.Grid
{
    public class GridValidator
    {
        private readonly GridState state;
        private readonly ConnectionManager connectionManager;

        public GridValidator(GridState state, ConnectionManager connectionManager)
        {
            this.state = state;
            this.connectionManager = connectionManager;
        }

        public bool IsValidConnection(Vector2Int start, Vector2Int end)
        {
            if (!state.IsWithinBounds(start) || !state.IsWithinBounds(end))
                return false;

            return (Mathf.Abs(start.x - end.x) == 1 && start.y == end.y) ||
                   (Mathf.Abs(start.y - end.y) == 1 && start.x == end.x);
        }

        public bool IsConnectionOccupied(Vector2Int start, Vector2Int end)
        {
            Connection conn = connectionManager.GetConnection(start, end);
            return conn == null || conn.IsOccupied;
        }

        public bool CanPlaceStick(Vector2Int origin, StickData stick, Vector2 centerOffset)
        {
            if (!IsValidPlacement(origin, stick, centerOffset)) 
                return false;

            foreach (var segment in stick.segments)
            {
                Vector2Int start = origin + segment.start - Vector2Int.RoundToInt(centerOffset);
                Vector2Int end = origin + segment.end - Vector2Int.RoundToInt(centerOffset);

                Connection conn = connectionManager.GetConnection(start, end);
                if (conn == null || conn.IsOccupied) 
                    return false;
            }

            return true;
        }

        public bool IsValidPlacement(Vector2Int origin, StickData stick, Vector2 centerOffset)
        {
            foreach (var segment in stick.segments)
            {
                Vector2Int start = origin + segment.start - Vector2Int.RoundToInt(centerOffset);
                Vector2Int end = origin + segment.end - Vector2Int.RoundToInt(centerOffset);

                if (!state.IsWithinBounds(start) || !state.IsWithinBounds(end)) 
                    return false;
                if (!IsValidConnection(start, end)) 
                    return false;
            }
            return true;
        }

        public bool CanStickBePlacedAnywhere(StickData stick)
        {
            Vector2Int min = Vector2Int.zero;
            Vector2Int max = Vector2Int.zero;

            foreach (var segment in stick.segments)
            {
                min.x = Mathf.Min(min.x, Mathf.Min(segment.start.x, segment.end.x));
                min.y = Mathf.Min(min.y, Mathf.Min(segment.start.y, segment.end.y));
                max.x = Mathf.Max(max.x, Mathf.Max(segment.start.x, segment.end.x));
                max.y = Mathf.Max(max.y, Mathf.Max(segment.start.y, segment.end.y));
            }

            int startX = Mathf.Max(0, -min.x);
            int endX = Mathf.Min(state.Width - 1, state.Width - max.x);
            int startY = Mathf.Max(0, -min.y);
            int endY = Mathf.Min(state.Height - 1, state.Height - max.y);

            if (startX > endX || startY > endY)
                return false;

            Vector2 centerOffset = CalculateStickCenterOffset(stick);
            
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    if (CanPlaceStick(new Vector2Int(x, y), stick, centerOffset))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private Vector2 CalculateStickCenterOffset(StickData stick)
        {
            Vector2 total = Vector2.zero;
            foreach (var segment in stick.segments)
            {
                total += (Vector2)(segment.start + segment.end) * 0.5f;
            }
            return total / stick.segments.Length;
        }
    }
}
