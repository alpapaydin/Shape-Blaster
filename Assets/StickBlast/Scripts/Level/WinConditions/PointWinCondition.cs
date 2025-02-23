using UnityEngine;
using StickBlast.UI;

namespace StickBlast.Level
{
    [CreateAssetMenu(fileName = "PointWinCondition", menuName = "StickBlast/Win Conditions/Points")]
    public class PointWinCondition : WinCondition
    {
        [SerializeField] private int pointsToWin = 100;
        public int PointsToWin => pointsToWin;
        private int currentPoints;

        public override void Initialize()
        {
            currentPoints = 0;
            UpdateProgress();
        }

        public override bool CheckWinCondition()
        {
            return currentPoints >= pointsToWin;
        }

        public void AddPoints(int points)
        {
            currentPoints += points;
            UpdateProgress();
            
            if (CheckWinCondition())
            {
                GameManager.Instance.WinGame();
            }
        }

        public override string GetProgressText()
        {
            return $"{currentPoints}/{pointsToWin}";
        }

        public override void UpdateProgress()
        {
            if (activeGoalUI is PointsGoalUI pointsUI)
            {
                pointsUI.UpdateProgress(currentPoints, pointsToWin);
            }
        }
    }
}
