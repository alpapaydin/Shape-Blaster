namespace StickBlast.Sticks
{
    public enum StickOrientation
    {
        Normal = 0,
        Right = 90,
        Bottom = 180,
        Left = 270
    }

    public static class StickOrientationExtensions
    {
        public static float GetRotationAngle(this StickOrientation orientation)
        {
            return (int)orientation;
        }
    }
}
