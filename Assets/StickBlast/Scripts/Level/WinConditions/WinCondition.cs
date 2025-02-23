using UnityEngine;
using StickBlast.UI;

namespace StickBlast.Level
{
    public abstract class WinCondition : ScriptableObject
    {
        [SerializeField] protected GameObject goalUIPrefab;
        protected BaseGoalUI activeGoalUI;

        public abstract bool CheckWinCondition();
        public abstract void Initialize();
        public GameObject SpawnGoalUI(Transform parent)
        {
            if (goalUIPrefab != null)
            {
                var instance = Instantiate(goalUIPrefab, parent);
                activeGoalUI = instance.GetComponent<BaseGoalUI>();
                if (activeGoalUI != null)
                {
                    Initialize();
                    activeGoalUI.Initialize(this);
                    UpdateProgress();
                }
                return instance;
            }
            return null;
        }
        public abstract void UpdateProgress();
        public abstract string GetProgressText();
    }
}
