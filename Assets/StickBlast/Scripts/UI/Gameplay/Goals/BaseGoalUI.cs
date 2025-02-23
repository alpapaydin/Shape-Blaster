using UnityEngine;
using StickBlast.Level;

namespace StickBlast.UI
{
    public abstract class BaseGoalUI : MonoBehaviour
    {
        public abstract void Initialize(WinCondition winCondition);
        public abstract void UpdateProgress(int current, int total);
    }
}
