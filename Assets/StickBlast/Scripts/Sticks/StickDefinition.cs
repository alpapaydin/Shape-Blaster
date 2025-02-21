using UnityEngine;

namespace StickBlast.Sticks
{
    [CreateAssetMenu(fileName = "StickDefinition", menuName = "StickBlast/Stick Definition")]
    public class StickDefinition : ScriptableObject
    {
        [System.Serializable]
        public class StickPart
        {
            public Sprite sprite;
            public Vector2 relativePosition;
        }

        public string stickName;
        public StickShape shape;
        public StickSegment[] segments;
        public StickPart[] stickParts;

        public StickSegment[] GetRotatedSegments(StickOrientation orientation)
        {
            float angle = (int)orientation;
            StickSegment[] rotatedSegments = new StickSegment[segments.Length];
            
            for (int i = 0; i < segments.Length; i++)
            {
                rotatedSegments[i] = segments[i].Rotate(angle);
            }

            return rotatedSegments;
        }
    }
}
