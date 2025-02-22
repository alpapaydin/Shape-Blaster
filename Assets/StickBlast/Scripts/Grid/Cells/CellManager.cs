using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace StickBlast.Grid
{
    public class CellManager
    {
        private readonly GridState state;
        private readonly ConnectionManager connectionManager;
        private readonly BlastManager blastManager;
        private HashSet<Vector2Int> currentBlastPreviewCells = new HashSet<Vector2Int>();

        public CellManager(GridState state, ConnectionManager connectionManager, BlastManager blastManager)
        {
            this.state = state;
            this.connectionManager = connectionManager;
            this.blastManager = blastManager;
        }

        public void CheckAndCompleteCell(Vector2Int cellPos)
        {
            if (!state.IsCellWithinBounds(cellPos) || state.Cells[cellPos.x, cellPos.y].IsComplete())
                return;

            Connection[] cellConnections = connectionManager.GetCellConnections(cellPos);
            bool isComplete = cellConnections.All(conn => conn != null && conn.IsOccupied);

            if (isComplete)
            {
                SoundManager.Instance.PlaySound("completeCell");
                state.Cells[cellPos.x, cellPos.y].SetComplete(true, true);
                blastManager.CheckForBlast();
            }
        }

        public void ClearBlastPreviews()
        {
            foreach (var cellPos in currentBlastPreviewCells)
            {
                state.Cells[cellPos.x, cellPos.y].ClearBlastPreview();
            }
            currentBlastPreviewCells.Clear();
        }

        public void ShowBlastPreview(Vector2Int cellPos)
        {
            state.Cells[cellPos.x, cellPos.y].ShowBlastPreview();
            currentBlastPreviewCells.Add(cellPos);
        }

        public int GetCompletedCellsInRow(int row)
        {
            int count = 0;
            for (int x = 0; x < state.Width - 1; x++)
            {
                if (state.Cells[x, row].IsComplete()) count++;
            }
            return count;
        }

        public int GetCompletedCellsInColumn(int col)
        {
            int count = 0;
            for (int y = 0; y < state.Height - 1; y++)
            {
                if (state.Cells[col, y].IsComplete()) count++;
            }
            return count;
        }
    }
}
