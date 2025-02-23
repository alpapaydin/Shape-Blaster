using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using StickBlast.Level;
using static System.Math;

namespace StickBlast.Grid
{
    public class BlastManager
    {
        private readonly GridState state;
        private readonly ConnectionManager connectionManager;
        private readonly CellManager cellManager;
        private bool isBlasting;
        private int comboCount;

        public BlastManager(GridState state, ConnectionManager connectionManager, CellManager cellManager)
        {
            this.state = state;
            this.connectionManager = connectionManager;
            this.cellManager = cellManager;
        }

        public void ResetCombo()
        {
            comboCount = 0;
        }

        public void CheckForBlast()
        {
            if (isBlasting) return;
            isBlasting = true;

            try
            {
                ProcessBlasts();
            }
            finally
            {
                isBlasting = false;
            }
        }

        private void ProcessBlasts()
        {
            var allBlastPositions = CollectAllBlastPositions();
            if (allBlastPositions.Count == 0) return;

            comboCount++;
            SoundManager.Instance.PlaySoundDelayed("blast", allBlastPositions.Count, 0.1f, 0.7f);
            LevelManager.Instance.AddPoints((int)(Pow(2, allBlastPositions.Count) * 10));
            Debug.Log($"{allBlastPositions.Count} lines to blast!");

            foreach (var pos in allBlastPositions)
            {
                ShowBlastVisual(pos);
            }

            var connectionsToReset = new HashSet<Connection>();
            var cellsToReset = new HashSet<Vector2Int>();

            foreach (var blastPos in allBlastPositions)
            {
                CollectCellsToReset(blastPos, cellsToReset);
            }

            foreach (var cellPos in cellsToReset)
            {
                if (state.IsCellWithinBounds(cellPos))
                {
                    var connections = connectionManager.GetCellConnections(cellPos);
                    if (connections != null)
                    {
                        foreach (var conn in connections)
                        {
                            if (conn != null && conn.IsOccupied)
                            {
                                connectionsToReset.Add(conn);
                            }
                        }
                    }
                }
            }

            foreach (var cellPos in cellsToReset)
            {
                if (state.IsCellWithinBounds(cellPos))
                {
                    var cell = state.Cells[cellPos.x, cellPos.y];
                    if (cell != null)
                    {
                        cell.SetComplete(false);
                    }
                }
            }

            foreach (var conn in connectionsToReset)
            {
                conn.Reset();
                if (conn.StartDot != null) CheckAndResetDot(conn.StartDot);
                if (conn.EndDot != null) CheckAndResetDot(conn.EndDot);
            }

            CheckAndResetPartialCells();

            var chainReactionBlasts = CollectAllBlastPositions();
            if (chainReactionBlasts.Count > 0)
            {
                ProcessBlasts();
            }
        }

        private void CheckAndResetPartialCells()
        {
            for (int x = 0; x < state.Width - 1; x++)
            {
                for (int y = 0; y < state.Height - 1; y++)
                {
                    var cell = state.Cells[x, y];
                    if (cell != null && cell.IsComplete())
                    {
                        var connections = connectionManager.GetCellConnections(new Vector2Int(x, y));
                        if (connections != null && !connections.All(conn => conn != null && conn.IsOccupied))
                        {
                            cell.SetComplete(false);
                        }
                    }
                }
            }
        }

        private List<Vector2Int> CollectAllBlastPositions()
        {
            var result = new List<Vector2Int>();
            var checkedRows = new HashSet<int>();
            var checkedColumns = new HashSet<int>();

            for (int y = 0; y < state.Height - 1; y++)
            {
                if (!checkedRows.Contains(y) && IsRowComplete(y))
                {
                    result.Add(new Vector2Int(-1, y));
                    checkedRows.Add(y);
                }
            }

            for (int x = 0; x < state.Width - 1; x++)
            {
                if (!checkedColumns.Contains(x) && IsColumnComplete(x))
                {
                    result.Add(new Vector2Int(x, -1));
                    checkedColumns.Add(x);
                }
            }

            return result;
        }

        private HashSet<Vector2Int> GetAllCellsToReset(List<Vector2Int> blastPositions)
        {
            var cellsToReset = new HashSet<Vector2Int>();
            foreach (var pos in blastPositions)
            {
                CollectCellsToReset(pos, cellsToReset);
            }
            return cellsToReset;
        }

        private void CollectBlastPositions(List<Vector2Int> blastPositions)
        {
            for (int y = 0; y < state.Height - 1; y++)
            {
                if (IsRowComplete(y))
                {
                    blastPositions.Add(new Vector2Int(-1, y));
                }
            }

            for (int x = 0; x < state.Width - 1; x++)
            {
                if (IsColumnComplete(x))
                {
                    blastPositions.Add(new Vector2Int(x, -1));
                }
            }
        }

        private bool IsRowComplete(int row)
        {
            int completedCells = 0;
            for (int x = 0; x < state.Width - 1; x++)
            {
                if (state.Cells[x, row].IsComplete())
                {
                    completedCells++;
                }
                else
                {
                    var connections = connectionManager.GetCellConnections(new Vector2Int(x, row));
                    if (connections != null && connections.All(conn => conn != null && conn.IsOccupied))
                    {
                        completedCells++;
                    }
                }
            }
            return completedCells == state.Width - 1;
        }

        private bool IsColumnComplete(int column)
        {
            int completedCells = 0;
            for (int y = 0; y < state.Height - 1; y++)
            {
                if (state.Cells[column, y].IsComplete())
                {
                    completedCells++;
                }
                else
                {
                    var connections = connectionManager.GetCellConnections(new Vector2Int(column, y));
                    if (connections != null && connections.All(conn => conn != null && conn.IsOccupied))
                    {
                        completedCells++;
                    }
                }
            }
            return completedCells == state.Height - 1;
        }

        private void ShowBlastVisual(Vector2Int pos)
        {
            if (pos.x == -1)
            {
                int middleColumn = (state.Width - 2) / 2;
                Vector3 blastScale = new Vector3(state.Width, 0.5f, 1);
                state.Cells[middleColumn, pos.y].ShowBlastVisual(false, blastScale);
            }
            else
            {
                int middleRow = (state.Height - 2) / 2;
                Vector3 blastScale = new Vector3(0.5f, state.Height, 1);
                state.Cells[pos.x, middleRow].ShowBlastVisual(true, blastScale);
            }
        }

        private void CollectCellsToReset(Vector2Int pos, HashSet<Vector2Int> cellsToReset)
        {
            if (pos.x == -1)
            {
                for (int x = 0; x < state.Width - 1; x++)
                {
                    cellsToReset.Add(new Vector2Int(x, pos.y));
                }
            }
            else
            {
                for (int y = 0; y < state.Height - 1; y++)
                {
                    cellsToReset.Add(new Vector2Int(pos.x, y));
                }
            }
        }

        private void CheckAndResetDot(Dot dot)
        {
            if (!connectionManager.DotHasOccupiedConnections(dot.GridPosition))
            {
                dot.SetBaseColor(state.EmptyColor);
                dot.SetOccupied(false);
            }
        }

        private void RecheckAffectedCells(HashSet<Vector2Int> affectedCells)
        {
            if (cellManager == null) return;

            foreach (var pos in affectedCells)
            {
                if (state.IsCellWithinBounds(pos))
                {
                    cellManager.CheckAndCompleteCell(pos);
                }
            }
        }

        private HashSet<Vector2Int> GetAllAffectedCells(HashSet<Vector2Int> centerCells)
        {
            var affectedCells = new HashSet<Vector2Int>();
            foreach (var centerPos in centerCells)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    for (int yOffset = -1; yOffset <= 1; yOffset++)
                    {
                        var neighborPos = new Vector2Int(
                            centerPos.x + xOffset,
                            centerPos.y + yOffset
                        );
                        if (state.IsCellWithinBounds(neighborPos))
                        {
                            affectedCells.Add(neighborPos);
                        }
                    }
                }
            }
            return affectedCells;
        }

        private void CheckForChainReaction()
        {
            var chainReactionBlasts = CollectAllBlastPositions();
            if (chainReactionBlasts.Count > 0)
            {
                CheckForBlast();
            }
        }
    }
}
