using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Playing,
        Won,
        Lost
    }

    private GameState currentState;
    private SoundManager soundManager;

    public GameState CurrentState
    {
        get => currentState;
        private set
        {
            currentState = value;
            OnGameStateChanged();
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeManager()
    {
        soundManager = FindObjectOfType<SoundManager>();
        currentState = GameState.Playing;
    }

    public void StartGame()
    {
        CurrentState = GameState.Playing;
        soundManager.PlayBGM("bgm");
    }

    public void WinGame()
    {
        if (currentState != GameState.Playing) return;
        Debug.Log("You won.");
        CurrentState = GameState.Won;
        soundManager.PlaySound("win");
    }

    public void LoseGame()
    {
        if (currentState != GameState.Playing) return;
        Debug.Log("You lost.");
        CurrentState = GameState.Lost;
        soundManager.PlaySound("lose");
    }

    private void OnGameStateChanged()
    {
        switch (currentState)
        {
            case GameState.Playing:
                break;
            case GameState.Won:
                break;
            case GameState.Lost:
                break;
        }
    }

    public bool IsPlaying()
    {
        return currentState == GameState.Playing;
    }
}
