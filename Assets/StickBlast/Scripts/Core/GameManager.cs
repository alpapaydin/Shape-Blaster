using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StickBlast.Level;

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
    private UIManager uiManager;
    private LevelManager levelManager;

    public GameState CurrentState
    {
        get => currentState;
        set
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
        levelManager = LevelManager.Instance;
        currentState = GameState.Playing;
        StartGame();
    }

    public void SetUIManager(UIManager manager)
    {
        uiManager = manager;
    }

    public void SetLevelManager(LevelManager manager)
    {
        levelManager = manager;
    }

    public void StartGame()
    {
        CurrentState = GameState.Playing;
        soundManager.PlayBGM("bgm");
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadNextLevel();
        }
    }

    public void WinGame()
    {
        if (currentState != GameState.Playing) return;
        Debug.Log("You won.");
        CurrentState = GameState.Won;
    }

    public void LoseGame()
    {
        if (currentState != GameState.Playing) return;
        CurrentState = GameState.Lost;
    }

    private void OnGameStateChanged()
    {
        switch (currentState)
        {
            case GameState.Playing:
                break;
            case GameState.Won:
                if (uiManager != null) uiManager.ShowWinPopup();
                break;
            case GameState.Lost:
                if (uiManager != null) uiManager.ShowLosePopup();
                break;
        }
    }

    public bool IsPlaying()
    {
        return currentState == GameState.Playing;
    }
}
