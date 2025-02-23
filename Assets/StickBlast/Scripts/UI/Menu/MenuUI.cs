using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    private void Start() 
        {SoundManager.Instance.PlayBGM("menubgm");}

    public void LogoSplash()
        {SoundManager.Instance.PlaySound("click");}

    public void ButtonPop()
        {SoundManager.Instance.PlaySound("pop");}

    public void StartGame()
        {GameManager.Instance.StartGame();}
    
    public void PlayIdle()
        {GetComponent<Animator>().Play("MenuIdle");}
}
