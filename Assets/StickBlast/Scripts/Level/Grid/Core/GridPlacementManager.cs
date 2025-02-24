using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using StickBlast.Sticks;

namespace StickBlast.Grid
{
    public class PlacementManager
    {
        private readonly GridState state;
        private readonly GridValidator validator;
        private readonly ConnectionManager connectionManager;
        private readonly CellManager cellManager;
        private readonly BlastManager blastManager;

        private HashSet<Vector2Int> currentBlastPreviewCells = new HashSet<Vector2Int>();

        private List<Vector2Int> lastCompletedCells;
        private HashSet<Vector2Int> lastBlastCells;
        private Vector2Int lastOrigin;
        private bool showingHighlight;
        private const float MIN_HIGHLIGHT_DISTANCE = 0.05f;

        public PlacementManager(GridState state, GridValidator validator, ConnectionManager connectionManager, 
                              CellManager cellManager, BlastManager blastManager)
        {
            this.state = state;
            this.validator = validator;
            this.connectionManager = connectionManager;
            this.cellManager = cellManager;
            this.blastManager = blastManager;
        }

        public void HighlightPotentialPlacement(Vector2Int origin, StickData stick)
        {
            bool isValid = validator.CanPlaceStick(origin, stick, CalculateStickCenterOffset(stick));
            
            if (!isValid)
            {
                if (showingHighlight)
                {
                    ClearHighlights();
                }
                SoundManager.Instance.FadeOutSound("highlightBlast");
                return;
            }

            if (showingHighlight && lastOrigin != origin)
            {
                ClearHighlights();
            }

            lastOrigin = origin;
            Vector2 centerOffset = CalculateStickCenterOffset(stick);
            HighlightValidConnections(origin, stick, centerOffset);
            ShowCompletionPreviews(origin, stick, centerOffset);
        }

        private void HighlightValidConnections(Vector2Int origin, StickData stick, Vector2 centerOffset)
        {
            showingHighlight = true;
            Color transparentThemeColor = state.ThemeColor;
            transparentThemeColor.a = 0.6f;

            foreach (var segment in stick.segments)
            {
                Vector2Int start = origin + segment.start - Vector2Int.RoundToInt(centerOffset);
                Vector2Int end = origin + segment.end - Vector2Int.RoundToInt(centerOffset);

                if (state.IsWithinBounds(start) && state.IsWithinBounds(end))
                {
                    Connection conn = connectionManager.GetConnection(start, end);
                    if (conn != null && !conn.IsOccupied)
                    {
                        state.Dots[start.x, start.y].Highlight(transparentThemeColor);
                        state.Dots[end.x, end.y].Highlight(transparentThemeColor);
                        conn.Highlight(transparentThemeColor);
                    }
                }
            }
        }

        public bool PlaceStick(Vector2Int origin, StickData stick)
        {
            Vector2 centerOffset = CalculateStickCenterOffset(stick);
            if (!validator.CanPlaceStick(origin, stick, centerOffset))
            {
                SoundManager.Instance.FadeOutSound("highlightBlast");
                return false;
            }

            blastManager.ResetCombo();

            foreach (var segment in stick.segments)
            {
                Vector2Int start = origin + segment.start - Vector2Int.RoundToInt(centerOffset);
                Vector2Int end = origin + segment.end - Vector2Int.RoundToInt(centerOffset);

                Connection conn = connectionManager.GetConnection(start, end);
                if (conn != null)
                {
                    OccupyConnection(conn, start, end);
                    CheckAdjacentCells(start, end);
                }
            }

            return true;
        }

        private void OccupyConnection(Connection conn, Vector2Int start, Vector2Int end)
        {
            conn.Occupy();
            state.Dots[start.x, start.y].SetBaseColor(state.ThemeColor);
            state.Dots[start.x, start.y].SetOccupied(true);
            state.Dots[end.x, end.y].SetBaseColor(state.ThemeColor);
            state.Dots[end.x, end.y].SetOccupied(true);
        }

        private void CheckAdjacentCells(Vector2Int start, Vector2Int end)
        {
            Vector2Int[] cellPositions = GetAdjacentCellPositions(start, end);
            foreach (Vector2Int cellPos in cellPositions)
            {
                if (state.IsCellWithinBounds(cellPos))
                {
                    cellManager.CheckAndCompleteCell(cellPos);
                }
            }
        }

        private Vector2Int[] GetAdjacentCellPositions(Vector2Int start, Vector2Int end)
        {
            if (start.x == end.x)
            {
                int x = start.x - 1;
                int y = Mathf.Min(start.y, end.y);
                return new Vector2Int[]
                {
                    new Vector2Int(x, y),
                    new Vector2Int(x + 1, y)
                };
            }
            else
            {
                int x = Mathf.Min(start.x, end.x);
                int y = start.y - 1;
                return new Vector2Int[]
                {
                    new Vector2Int(x, y),
                    new Vector2Int(x, y + 1)
                };
            }
        }

        private void ShowCompletionPreviews(Vector2Int origin, StickData stick, Vector2 centerOffset)
        {
            var completedCells = SimulatePlacement(origin, stick, centerOffset);
            
            if (lastCompletedCells != null && 
                completedCells.Count == lastCompletedCells.Count && 
                completedCells.TrueForAll(c => lastCompletedCells.Contains(c)))
            {
                if (lastBlastCells != null && lastBlastCells.Count > 0)
                {
                    foreach (var cellPos in lastBlastCells)
                    {
                        cellManager.ShowBlastPreview(cellPos);
                    }
                    SoundManager.Instance.TryPlaySound("highlightBlast");
                    return;
                }
            }

            lastCompletedCells = completedCells;
            
            if (completedCells.Count > 0)
            {
                var simulatedState = new HashSet<Vector2Int>(completedCells);
                var blastCells = CalculateBlastCells(simulatedState);
                
                lastBlastCells = blastCells;

                foreach (var cellPos in blastCells)
                {
                    cellManager.ShowBlastPreview(cellPos);
                }

                if (blastCells.Count > 0)
                    SoundManager.Instance.TryPlaySound("highlightBlast");
                else
                    SoundManager.Instance.FadeOutSound("highlightBlast");
            }
            else
            {
                lastBlastCells = null;
                SoundManager.Instance.FadeOutSound("highlightBlast");
            }
        }

        private HashSet<Vector2Int> CalculateBlastCells(HashSet<Vector2Int> simulatedState)
        {
            var blastCells = new HashSet<Vector2Int>();

            var rowCompletions = new int[state.Height - 1];
            var colCompletions = new int[state.Width - 1];

            for (int y = 0; y < state.Height - 1; y++)
            {
                rowCompletions[y] = cellManager.GetCompletedCellsInRow(y);
            }
            for (int x = 0; x < state.Width - 1; x++)
            {
                colCompletions[x] = cellManager.GetCompletedCellsInColumn(x);
            }

            foreach (var cell in simulatedState)
            {
                rowCompletions[cell.y]++;
                colCompletions[cell.x]++;
            }

            for (int y = 0; y < state.Height - 1; y++)
            {
                if (rowCompletions[y] >= state.Width - 1)
                {
                    for (int x = 0; x < state.Width - 1; x++)
                    {
                        blastCells.Add(new Vector2Int(x, y));
                    }
                }
            }

            for (int x = 0; x < state.Width - 1; x++)
            {
                if (colCompletions[x] >= state.Height - 1)
                {
                    for (int y = 0; y < state.Height - 1; y++)
                    {
                        blastCells.Add(new Vector2Int(x, y));
                    }
                }
            }

            return blastCells;
        }

        private List<Vector2Int> SimulatePlacement(Vector2Int origin, StickData stick, Vector2 centerOffset)
        {
            List<Vector2Int> completedCells = new List<Vector2Int>();
            foreach (var segment in stick.segments)
            {
                Vector2Int start = origin + segment.start - Vector2Int.RoundToInt(centerOffset);
                Vector2Int end = origin + segment.end - Vector2Int.RoundToInt(centerOffset);

                foreach (Vector2Int cellPos in GetAdjacentCellPositions(start, end))
                {
                    if (state.IsCellWithinBounds(cellPos) && !state.Cells[cellPos.x, cellPos.y].IsComplete())
                    {
                        if (WouldCompleteCell(cellPos, start, end))
                        {
                            completedCells.Add(cellPos);
                        }
                    }
                }
            }
            return completedCells;
        }

        private bool WouldCompleteCell(Vector2Int cellPos, Vector2Int newStart, Vector2Int newEnd)
        {
            Connection[] cellConnections = connectionManager.GetCellConnections(cellPos);
            foreach (var conn in cellConnections)
            {
                if (conn == null) return false;
                if (!conn.IsOccupied && !IsMatchingConnection(conn, newStart, newEnd))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsMatchingConnection(Connection conn, Vector2Int start, Vector2Int end)
        {
            return (conn.StartDot.GridPosition == start && conn.EndDot.GridPosition == end) ||
                   (conn.StartDot.GridPosition == end && conn.EndDot.GridPosition == start);
        }

        public void ClearHighlights()
        {
            showingHighlight = false;
            
            for (int x = 0; x < state.Width; x++)
            {
                for (int y = 0; y < state.Height; y++)
                {
                    state.Dots[x, y].ClearHighlight();
                }
            }

            for (int x = 0; x < state.Width; x++)
            {
                for (int y = 0; y < state.Height - 1; y++)
                {
                    state.HorizontalConnections[x, y]?.SetColors(state.EmptyColor, state.ThemeColor);
                }
            }

            for (int x = 0; x < state.Width - 1; x++)
            {
                for (int y = 0; y < state.Height; y++)
                {
                    state.VerticalConnections[x, y]?.SetColors(state.EmptyColor, state.ThemeColor);
                }
            }

            cellManager.ClearBlastPreviews();
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
