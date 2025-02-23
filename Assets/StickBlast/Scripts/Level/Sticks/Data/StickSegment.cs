using UnityEngine;

namespace StickBlast.Sticks
{
    [System.Serializable]
    public struct StickSegment
    {
        public Vector2Int start;
        public Vector2Int end;

        public StickSegment(Vector2Int start, Vector2Int end)
        {
            this.start = start;
            this.end = end;
        }

        public StickSegment Rotate(float degrees)
        {
            float rad = -degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            Vector2Int rotatedStart = new Vector2Int(
                Mathf.RoundToInt(start.x * cos - start.y * sin),
                Mathf.RoundToInt(start.x * sin + start.y * cos)
            );

            Vector2Int rotatedEnd = new Vector2Int(
                Mathf.RoundToInt(end.x * cos - end.y * sin),
                Mathf.RoundToInt(end.x * sin + end.y * cos)
            );

            return new StickSegment(rotatedStart, rotatedEnd);
        }
    }
}
