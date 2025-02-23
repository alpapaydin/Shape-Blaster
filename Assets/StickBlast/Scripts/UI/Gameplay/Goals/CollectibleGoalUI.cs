using UnityEngine;
using System.Collections.Generic;
using StickBlast.Level;

namespace StickBlast.UI
{
    public class CollectibleGoalUI : BaseGoalUI
    {
        [SerializeField] private GameObject singleItemGoalPrefab;
        [SerializeField] private Transform goalsContainer;
        
        private Dictionary<ItemType, SingleItemGoal> itemGoals = new Dictionary<ItemType, SingleItemGoal>();
        private List<ItemRequirement> requirements;

        public override void Initialize(WinCondition winCondition)
        {
            if (winCondition is CollectItemsWinCondition collectWin)
            {
                Initialize(collectWin.requiredItems);
            }
        }

        private void Initialize(List<ItemRequirement> itemRequirements)
        {
            requirements = itemRequirements;
            foreach (var req in itemRequirements)
            {
                var goalInstance = Instantiate(singleItemGoalPrefab, goalsContainer).GetComponent<SingleItemGoal>();
                goalInstance.Initialize(req.itemType.icon, req.requiredAmount);
                itemGoals[req.itemType] = goalInstance;
            }
        }

        public void UpdateItemProgress(ItemType type, int current, int required)
        {
            if (itemGoals.TryGetValue(type, out SingleItemGoal goal))
            {
                goal.UpdateProgress(current, required);
            }
        }

        public override void UpdateProgress(int current, int total)
        {
        }
    }
}
