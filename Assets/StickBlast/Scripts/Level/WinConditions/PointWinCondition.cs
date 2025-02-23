using UnityEngine;

namespace StickBlast.Level
{
    [CreateAssetMenu(fileName = "PointWinCondition", menuName = "StickBlast/Win Conditions/Points")]
    public class PointWinCondition : WinCondition
    {
        public int targetPoints;
        private int currentPoints;

        public override void Initialize()
        {
            currentPoints = 0;
        }

        public void AddPoints(int points)
        {
            currentPoints += points;
            if (CheckWinCondition())
            {
                GameManager.Instance.WinGame();
            }
        }

        public override bool CheckWinCondition()
        {
            return currentPoints >= targetPoints;
        }

        public override string GetProgressText()
        {
            return $"{currentPoints}/{targetPoints}";
        }
    }
}
