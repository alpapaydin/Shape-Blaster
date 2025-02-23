using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StickBlast.Level
{
    public abstract class WinCondition : ScriptableObject
    {
        public abstract bool CheckWinCondition();
        public abstract void Initialize();
        public abstract string GetProgressText();
    }

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

    [CreateAssetMenu(fileName = "CollectItemsWinCondition", menuName = "StickBlast/Win Conditions/Collect Items")]
    public class CollectItemsWinCondition : WinCondition
    {
        public int itemCount;
        private int collectedItems;

        public override void Initialize()
        {
            collectedItems = 0;
        }

        public void CollectItem()
        {
            collectedItems++;
            if (CheckWinCondition())
            {
                GameManager.Instance.WinGame();
            }
        }

        public override bool CheckWinCondition()
        {
            return collectedItems >= itemCount;
        }

        public override string GetProgressText()
        {
            return $"{collectedItems}/{itemCount}";
        }
    }
}
