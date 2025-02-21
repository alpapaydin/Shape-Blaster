using UnityEngine;

namespace StickBlast.Sticks
{
    public class StickData
    {
        public StickDefinition definition;
        public StickOrientation orientation;
        public StickSegment[] segments;
        public StickDefinition.StickPart[] parts => definition.stickParts;

        public static StickData Create(StickDefinition definition, StickOrientation orientation)
        {
            return new StickData
            {
                definition = definition,
                orientation = orientation,
                segments = definition.GetRotatedSegments(orientation)
            };
        }
    }
}
