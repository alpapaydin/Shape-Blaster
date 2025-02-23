using UnityEngine;

namespace StickBlast.Level
{
    public abstract class WinCondition : ScriptableObject
    {
        public abstract bool CheckWinCondition();
        public abstract void Initialize();
        public abstract string GetProgressText();
    }
}
