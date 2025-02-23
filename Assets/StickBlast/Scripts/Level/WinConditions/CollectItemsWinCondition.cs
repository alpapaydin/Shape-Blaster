using UnityEngine;
using System.Collections.Generic;

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
    }
}
