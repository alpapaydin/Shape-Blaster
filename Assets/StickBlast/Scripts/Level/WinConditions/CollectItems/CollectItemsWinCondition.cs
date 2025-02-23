using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using StickBlast.UI;

namespace StickBlast.Level
{
    [System.Serializable]
    public class ItemRequirement
    {
        public ItemType itemType;
        public int requiredAmount;
    }

    [CreateAssetMenu(fileName = "CollectItemsWinCondition", menuName = "StickBlast/Win Conditions/Collect Items")]
    public class CollectItemsWinCondition : WinCondition
    {
        public List<ItemRequirement> requiredItems = new List<ItemRequirement>();
        private Dictionary<ItemType, int> collectedItems = new Dictionary<ItemType, int>();

        public override void Initialize()
        {
            collectedItems.Clear();
            foreach (var requirement in requiredItems)
            {
                collectedItems[requirement.itemType] = 0;
            }
        }

        public void CollectItem(ItemType itemType)
        {
            if (!collectedItems.ContainsKey(itemType)) return;
            
            collectedItems[itemType]++;
            UpdateProgress();
            if (CheckWinCondition())
            {
                GameManager.Instance.WinGame();
            }
        }

        public override bool CheckWinCondition()
        {
            foreach (var requirement in requiredItems)
            {
                if (!collectedItems.ContainsKey(requirement.itemType) ||
                    collectedItems[requirement.itemType] < requirement.requiredAmount)
                {
                    return false;
                }
            }
            return true;
        }

        public override string GetProgressText()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < requiredItems.Count; i++)
            {
                var requirement = requiredItems[i];
                int collected = collectedItems.ContainsKey(requirement.itemType) ? 
                    collectedItems[requirement.itemType] : 0;
                
                sb.Append($"{requirement.itemType.itemName}: {collected}/{requirement.requiredAmount}");
                if (i < requiredItems.Count - 1) sb.Append(", ");
            }
            return sb.ToString();
        }

        public override void UpdateProgress()
        {
            if (activeGoalUI == null) return;
            
            if (activeGoalUI is CollectibleGoalUI collectibleUI)
            {
                foreach (var req in requiredItems)
                {
                    int collected = collectedItems.ContainsKey(req.itemType) ? collectedItems[req.itemType] : 0;
                    collectibleUI.UpdateItemProgress(req.itemType, collected, req.requiredAmount);
                }
            }
        }
    }
}
