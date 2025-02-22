using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject losePopupPrefab;
    [SerializeField] private GameObject winPopupPrefab;
    
    private GameObject activeWinPopup;
    private GameObject activeLosePopup;
    private Canvas canvas;

    private void Start()
    {
        canvas = GetComponent<Canvas>();
        GameManager.Instance.SetUIManager(this);
    }

    private void HideAllPopups()
    {
        if (activeWinPopup) Destroy(activeWinPopup);
        if (activeLosePopup) Destroy(activeLosePopup);
    }

    public void ShowWinPopup()
    {
        HideAllPopups();
        if (winPopupPrefab)
        {
            activeWinPopup = Instantiate(winPopupPrefab, canvas.transform);
            SoundManager.Instance.PlaySound("win_popup", true);
        }
    }

    public void ShowLosePopup()
    {
        HideAllPopups();
        if (losePopupPrefab)
        {
            activeLosePopup = Instantiate(losePopupPrefab, canvas.transform);
            SoundManager.Instance.PlaySound("lose_popup", true);
        }
    }
}
