using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StickBlast.Sticks;
using System.Linq;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width = 6;
    [SerializeField] private int height = 4;
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private GameObject connectionPrefab;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private LayoutManager layoutManager;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color themeColor = Color.green;
    [SerializeField] private Color emptyColor = Color.blue;
    private Dot[,] dots;
    private Connection[,] horizontalConnections;
    private Connection[,] verticalConnections;
    private Cell[,] cells;
    private float currentScale = 1f;
    private HashSet<Vector2Int> currentBlastPreviewCells = new HashSet<Vector2Int>();

    void Start()
    {
        InitializeGrid();
    }

    private void OnRectTransformDimensionsChange()
    {
        UpdateGridTransform();
    }

    private void UpdateGridTransform()
    {
        if (layoutManager != null)
        {
            float gridScale = layoutManager.GetGridScale(width, height);
            Vector3 gridPosition = layoutManager.GetGridPosition(width, height);
            transform.position = gridPosition;
            transform.localScale = Vector3.one * gridScale;
            currentScale = gridScale;
            UpdateConnectionWidths(gridScale);
        }
    }

    public float GetCurrentScale() => currentScale;

    private void UpdateConnectionWidths(float scale)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 1; y++)
            {
                if (horizontalConnections[x, y] != null)
                {
                    horizontalConnections[x, y].UpdateWidth(scale);
                }
            }
        }

        for (int x = 0; x < width - 1; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (verticalConnections[x, y] != null)
                {
                    verticalConnections[x, y].UpdateWidth(scale);
                }
            }
        }
    }

    private void InitializeGrid()
    {
        dots = new Dot[width, height];
        horizontalConnections = new Connection[width, height - 1];
        verticalConnections = new Connection[width - 1, height];

        float offsetX = -(width - 1) * 0.5f;
        float offsetY = -(height - 1) * 0.5f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x + offsetX, y + offsetY, 0);
                GameObject dotObj = Instantiate(dotPrefab, position, Quaternion.identity, transform);
                dots[x, y] = dotObj.GetComponent<Dot>();
                dots[x, y].Initialize(new Vector2Int(x, y));
                dots[x, y].SetBaseColor(emptyColor);
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 1; y++)
            {
                Vector3 position = new Vector3(x + offsetX, y + 0.5f + offsetY, 0);
                GameObject connObj = Instantiate(connectionPrefab, position, Quaternion.identity, transform);
                horizontalConnections[x, y] = connObj.GetComponent<Connection>();
                horizontalConnections[x, y].Initialize(dots[x, y], dots[x, y + 1]);
                horizontalConnections[x, y].SetColors(emptyColor, themeColor);
            }
        }

        for (int x = 0; x < width - 1; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x + 0.5f + offsetX, y + offsetY, 0);
                GameObject connObj = Instantiate(connectionPrefab, position, Quaternion.identity, transform);
                verticalConnections[x, y] = connObj.GetComponent<Connection>();
                verticalConnections[x, y].Initialize(dots[x, y], dots[x + 1, y]);
                verticalConnections[x, y].SetColors(emptyColor, themeColor);
            }
        }

        cells = new Cell[width - 1, height - 1];
        for (int x = 0; x < width - 1; x++)
        {
            for (int y = 0; y < height - 1; y++)
            {
                Vector3 position = new Vector3(x + 0.5f + offsetX, y + 0.5f + offsetY, 0);
                GameObject cellObj = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                cells[x, y] = cellObj.GetComponent<Cell>();
            }
        }

        UpdateGridTransform();

    }


    private bool IsValidConnection(Vector2Int start, Vector2Int end)
    {
        if (start.x < 0 || start.x >= width || start.y < 0 || start.y >= height ||
            end.x < 0 || end.x >= width || end.y < 0 || end.y >= height)
            return false;

        return (Mathf.Abs(start.x - end.x) == 1 && start.y == end.y) ||
               (Mathf.Abs(start.y - end.y) == 1 && start.x == end.x);
    }

    private Connection GetConnection(Vector2Int start, Vector2Int end)
    {
        if (start.x == end.x)
        {
            int minY = Mathf.Min(start.y, end.y);
            return horizontalConnections[start.x, minY];
        }
        else
        {
            int minX = Mathf.Min(start.x, end.x);
            return verticalConnections[minX, start.y];
        }
    }

    private void CheckForCompletedCells(Vector2Int start, Vector2Int end)
    {
        Vector2Int[] cellPositions = GetAdjacentCellPositions(start, end);

        foreach (Vector2Int cellPos in cellPositions)
        {
            if (IsCellWithinBounds(cellPos) && !cells[cellPos.x, cellPos.y].IsComplete())
            {
                CheckAndCompleteCell(cellPos);
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

    private bool IsCellWithinBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width - 1 &&
               pos.y >= 0 && pos.y < height - 1;
    }

    private Connection[] GetCellConnections(Vector2Int cellPos)
    {
        return new Connection[]
        {
            verticalConnections[cellPos.x, cellPos.y],
            horizontalConnections[cellPos.x + 1, cellPos.y],
            verticalConnections[cellPos.x, cellPos.y + 1],
            horizontalConnections[cellPos.x, cellPos.y]
        };
    }

    private void CheckAndCompleteCell(Vector2Int cellPos)
    {
        Connection[] cellConnections = GetCellConnections(cellPos);

        bool isComplete = cellConnections.All(conn => conn != null && conn.IsOccupied);

        if (isComplete)
        {
            cells[cellPos.x, cellPos.y].SetComplete(themeColor);
            CheckForBlast();
        }
    }

    private void CheckForBlast()
    {
        for (int y = 0; y < height - 1; y++)
        {
            bool rowComplete = true;
            for (int x = 0; x < width - 1; x++)
            {
                if (!cells[x, y].IsComplete())
                {
                    rowComplete = false;
                    break;
                }
            }
            if (rowComplete)
            {
                BlastRow(y);
            }
        }

        for (int x = 0; x < width - 1; x++)
        {
            bool columnComplete = true;
            for (int y = 0; y < height - 1; y++)
            {
                if (!cells[x, y].IsComplete())
                {
                    columnComplete = false;
                    break;
                }
            }
            if (columnComplete)
            {
                BlastColumn(x);
            }
        }
    }

    private void BlastRow(int row)
    {
        for (int x = 0; x < width - 1; x++)
        {
            cells[x, row].Reset();
            ResetCellConnections(new Vector2Int(x, row));
        }

        for (int x = 0; x < width - 1; x++)
        {
            if (row > 0)
                CheckAndResetIncompleteCell(new Vector2Int(x, row - 1));
            if (row < height - 2)
                CheckAndResetIncompleteCell(new Vector2Int(x, row + 1));
        }
    }

    private void BlastColumn(int column)
    {
        for (int y = 0; y < height - 1; y++)
        {
            cells[column, y].Reset();
            ResetCellConnections(new Vector2Int(column, y));
        }

        for (int y = 0; y < height - 1; y++)
        {
            if (column > 0)
                CheckAndResetIncompleteCell(new Vector2Int(column - 1, y));
            if (column < width - 2)
                CheckAndResetIncompleteCell(new Vector2Int(column + 1, y));
        }
    }

    private void CheckAndResetIncompleteCell(Vector2Int cellPos)
    {
        if (!cells[cellPos.x, cellPos.y].IsComplete())
            return;

        Connection[] cellConnections = GetCellConnections(cellPos);
        bool isStillComplete = cellConnections.All(conn => conn != null && conn.IsOccupied);

        if (!isStillComplete)
        {
            cells[cellPos.x, cellPos.y].Reset();
        }
    }

    private bool DotHasOccupiedConnections(int x, int y)
    {
        if (x > 0 && verticalConnections[x-1, y] != null && verticalConnections[x-1, y].IsOccupied) return true;
        if (x < width-1 && verticalConnections[x, y] != null && verticalConnections[x, y].IsOccupied) return true;

        if (y > 0 && horizontalConnections[x, y-1] != null && horizontalConnections[x, y-1].IsOccupied) return true;
        if (y < height-1 && horizontalConnections[x, y] != null && horizontalConnections[x, y].IsOccupied) return true;

        return false;
    }

    private void ResetCellConnections(Vector2Int cellPos)
    {
        Connection[] connections = GetCellConnections(cellPos);
        foreach (var conn in connections)
        {
            if (conn != null)
            {
                conn.Reset();

                Dot startDot = conn.StartDot;
                Dot endDot = conn.EndDot;

                if (!DotHasOccupiedConnections(startDot.GridPosition.x, startDot.GridPosition.y))
                {
                    startDot.SetBaseColor(emptyColor);
                    startDot.SetOccupied(false);
                }

                if (!DotHasOccupiedConnections(endDot.GridPosition.x, endDot.GridPosition.y))
                {
                    endDot.SetBaseColor(emptyColor);
                    endDot.SetOccupied(false);
                }
            }
        }
    }

    public float GetWorldWidth() => width - 1;
    public float GetWorldHeight() => height - 1;

    public Vector2Int WorldToGridPosition(Vector2 worldPosition)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);
        return new Vector2Int(
            Mathf.RoundToInt(localPos.x + (width - 1) * 0.5f),
            Mathf.RoundToInt(localPos.y + (height - 1) * 0.5f)
        );
    }

    public void HighlightPotentialPlacement(Vector2Int origin, StickData stick)
    {
        ClearHighlights();

        if (!CanPlaceStick(origin, stick)) return;

        Vector2 centerOffset = CalculateStickCenterOffset(stick);
        foreach (var segment in stick.segments)
        {
            Vector2Int start = origin + segment.start - Vector2Int.RoundToInt(centerOffset);
            Vector2Int end = origin + segment.end - Vector2Int.RoundToInt(centerOffset);

            if (IsValidGridPosition(start) && IsValidGridPosition(end))
            {
                if (IsValidConnection(start, end))
                {
                    Connection conn = GetConnection(start, end);
                    if (conn != null && !conn.IsOccupied)
                    {
                        dots[start.x, start.y].Highlight(highlightColor);
                        dots[end.x, end.y].Highlight(highlightColor);
                        conn.Highlight(highlightColor);
                    }
                }
            }
        }

        var completedCells = SimulatePlacement(origin, stick);
        if (completedCells.Count > 0)
        {
            //cell completion preview
            var blastCells = CheckPotentialBlast(completedCells);
            if (blastCells.Count > 0)
            {
                foreach (var cellPos in blastCells)
                {
                    cells[cellPos.x, cellPos.y].ShowBlastPreview(themeColor);
                    currentBlastPreviewCells.Add(cellPos);
                }
            }
        }
    }

    private Vector2 CalculateStickCenterOffset(StickData stick)
    {
        Vector2 total = Vector2.zero;
        foreach (var segment in stick.segments)
        {
            total += (Vector2)(segment.start + segment.end) * 0.5f;
        }
        return total / (float)stick.segments.Length;
    }

    public void ClearHighlights()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                dots[x, y].ClearHighlight();
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 1; y++)
            {
                if (horizontalConnections[x, y] != null)
                {
                    horizontalConnections[x, y].SetColors(emptyColor, themeColor);
                }
            }
        }

        for (int x = 0; x < width - 1; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (verticalConnections[x, y] != null)
                {
                    verticalConnections[x, y].SetColors(emptyColor, themeColor);
                }
            }
        }

        foreach (var cellPos in currentBlastPreviewCells)
        {
            cells[cellPos.x, cellPos.y].ClearBlastPreview();
        }
        currentBlastPreviewCells.Clear();
    }

    public bool CanPlaceStick(Vector2Int origin, StickData stick)
    {
        if (!IsValidPlacement(origin, stick)) return false;

        Vector2 centerOffset = CalculateStickCenterOffset(stick);
        foreach (var segment in stick.segments)
        {
            Vector2Int start = origin + segment.start - Vector2Int.RoundToInt(centerOffset);
            Vector2Int end = origin + segment.end - Vector2Int.RoundToInt(centerOffset);

            Connection conn = GetConnection(start, end);
            if (conn == null || conn.IsOccupied) return false;
        }

        return true;
    }

    public bool PlaceStick(Vector2Int origin, StickData stick)
    {
        if (!CanPlaceStick(origin, stick)) return false;

        Vector2 centerOffset = CalculateStickCenterOffset(stick);
        foreach (var segment in stick.segments)
        {
            Vector2Int start = origin + segment.start - Vector2Int.RoundToInt(centerOffset);
            Vector2Int end = origin + segment.end - Vector2Int.RoundToInt(centerOffset);

            Connection conn = GetConnection(start, end);
            if (conn != null)
            {
                conn.Occupy();
                dots[start.x, start.y].SetBaseColor(themeColor);
                dots[start.x, start.y].SetOccupied(true);
                dots[end.x, end.y].SetBaseColor(themeColor);
                dots[end.x, end.y].SetOccupied(true);

                CheckForCompletedCells(start, end);
            }
        }

        return true;
    }

    private bool IsValidPlacement(Vector2Int origin, StickData stick)
    {
        Vector2 centerOffset = CalculateStickCenterOffset(stick);
        foreach (var segment in stick.segments)
        {
            Vector2Int start = origin + segment.start - Vector2Int.RoundToInt(centerOffset);
            Vector2Int end = origin + segment.end - Vector2Int.RoundToInt(centerOffset);

            if (!IsValidGridPosition(start) || !IsValidGridPosition(end)) return false;
            if (!IsValidConnection(start, end)) return false;
        }
        return true;
    }

    public bool IsValidGridPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public bool IsConnectionOccupied(Vector2Int start, Vector2Int end)
    {
        Connection conn = GetConnection(start, end);
        return conn == null || conn.IsOccupied;
    }

    public Color GetThemeColor()
    {
        return themeColor;
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
        int endX = Mathf.Min(width - 1, width - max.x);
        int startY = Mathf.Max(0, -min.y);
        int endY = Mathf.Min(height - 1, height - max.y);

        if (startX > endX || startY > endY)
            return false;

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                if (CanPlaceStick(new Vector2Int(x, y), stick))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private List<Vector2Int> SimulatePlacement(Vector2Int origin, StickData stick)
    {
        List<Vector2Int> completedCells = new List<Vector2Int>();
        Vector2 centerOffset = CalculateStickCenterOffset(stick);

        foreach (var segment in stick.segments)
        {
            Vector2Int start = origin + segment.start - Vector2Int.RoundToInt(centerOffset);
            Vector2Int end = origin + segment.end - Vector2Int.RoundToInt(centerOffset);

            Vector2Int[] cellPositions = GetAdjacentCellPositions(start, end);
            foreach (Vector2Int cellPos in cellPositions)
            {
                if (IsCellWithinBounds(cellPos) && !cells[cellPos.x, cellPos.y].IsComplete())
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
        Connection[] cellConnections = GetCellConnections(cellPos);
        foreach (var conn in cellConnections)
        {
            if (conn == null) return false;
            if (!conn.IsOccupied)
            {
                if (!IsMatchingConnection(conn, newStart, newEnd))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private bool IsMatchingConnection(Connection conn, Vector2Int start, Vector2Int end)
    {
        return (conn.StartDot.GridPosition == start && conn.EndDot.GridPosition == end) ||
               (conn.StartDot.GridPosition == end && conn.EndDot.GridPosition == start);
    }

    private List<Vector2Int> CheckPotentialBlast(List<Vector2Int> completedCells)
    {
        List<Vector2Int> blastCells = new List<Vector2Int>();
        Dictionary<int, int> rowCounts = new Dictionary<int, int>();
        Dictionary<int, int> colCounts = new Dictionary<int, int>();

        foreach (var cell in completedCells)
        {
            if (!rowCounts.ContainsKey(cell.y)) rowCounts[cell.y] = 0;
            if (!colCounts.ContainsKey(cell.x)) colCounts[cell.x] = 0;

            rowCounts[cell.y]++;
            colCounts[cell.x]++;
        }

        foreach (var kvp in rowCounts)
        {
            if (kvp.Value + GetCompletedCellsInRow(kvp.Key) >= width - 1)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    blastCells.Add(new Vector2Int(x, kvp.Key));
                }
            }
        }

        foreach (var kvp in colCounts)
        {
            if (kvp.Value + GetCompletedCellsInColumn(kvp.Key) >= height - 1)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    blastCells.Add(new Vector2Int(kvp.Key, y));
                }
            }
        }

        return blastCells;
    }

    private int GetCompletedCellsInRow(int row)
    {
        int count = 0;
        for (int x = 0; x < width - 1; x++)
        {
            if (cells[x, row].IsComplete()) count++;
        }
        return count;
    }

    private int GetCompletedCellsInColumn(int col)
    {
        int count = 0;
        for (int y = 0; y < height - 1; y++)
        {
            if (cells[col, y].IsComplete()) count++;
        }
        return count;
    }
}
