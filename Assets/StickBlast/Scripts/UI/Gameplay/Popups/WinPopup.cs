using UnityEngine;
using UnityEngine.SceneManagement;
using StickBlast.Level;
public class WinPopup : Popup
{
    public new void RestartLevel()
        { LevelManager.Instance.LoadNextLevel(); }
}