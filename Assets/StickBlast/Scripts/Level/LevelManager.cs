using UnityEngine;
using UnityEngine.SceneManagement;
using StickBlast.Grid;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StickBlast.Level
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [SerializeField] private LevelDefinition[] levels;

#if UNITY_EDITOR
        [SerializeField] private SceneAsset levelScene;
        [SerializeField] private SceneAsset menuScene;
#endif
        [SerializeField] private string levelSceneName = "LevelScene";
        [SerializeField] private string menuSceneName = "MainMenu";

        public LevelDefinition CurrentLevel { get; private set; }
        public int CurrentLevelIndex => currentLevelIndex;
        private GridManager gridManager;
        private int currentLevelIndex = -1;

        private const string CURRENT_LEVEL_KEY = "CurrentLevel";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadSavedLevel();
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetLevelManager(this);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadSavedLevel()
        {
            currentLevelIndex = PlayerPrefs.GetInt(CURRENT_LEVEL_KEY, -1);
        }

        private void SaveCurrentLevel()
        {
            PlayerPrefs.SetInt(CURRENT_LEVEL_KEY, currentLevelIndex);
            PlayerPrefs.Save();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (levelScene != null)
            {
                levelSceneName = levelScene.name;
            }
            if (menuScene != null)
            {
                menuSceneName = menuScene.name;
            }
        }
#endif

        public void LoadNextLevel()
        {
            currentLevelIndex++;
            if (currentLevelIndex >= levels.Length)
            {
                currentLevelIndex = 0;
            }
            SaveCurrentLevel();
            GameManager.Instance.CurrentState = GameManager.GameState.Playing;
            LoadLevelByIndex(currentLevelIndex);
        }

        public void RestartLevel()
        {
            GameManager.Instance.CurrentState = GameManager.GameState.Playing;
            LoadLevelByIndex(currentLevelIndex);
        }

        public void LoadLevelByIndex(int index)
        {
            if (index < 0 || index >= levels.Length) return;
            
            currentLevelIndex = index;
            SaveCurrentLevel();
            StartCoroutine(LoadLevelRoutine());
        }

        private System.Collections.IEnumerator LoadLevelRoutine()
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelSceneName);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            yield return null;

            CurrentLevel = levels[currentLevelIndex];
            gridManager = FindObjectOfType<GridManager>();
            
            if (gridManager != null)
            {
                gridManager.InitializeFromLevel(CurrentLevel);
                
                if (CurrentLevel.hasCollectibles)
                {
                    SpawnCollectibles();
                }

                CurrentLevel.winCondition.Initialize();
                
                var uiManager = FindObjectOfType<UIManager>();
                if (uiManager != null)
                {
                    uiManager.UpdateProgressText(CurrentLevel.winCondition.GetProgressText());
                }
            }
        }

        private void SpawnCollectibles()
        {
            var cells = FindObjectsOfType<Cell>();
            foreach (var cell in cells)
            {
                if (Random.value < CurrentLevel.collectibleSpawnChance)
                {
                    cell.SpawnCollectible(CurrentLevel.collectiblePrefab);
                }
            }
        }

        public void AddPoints(int points)
        {
            if (CurrentLevel.winCondition is PointWinCondition pointWin)
            {
                pointWin.AddPoints(points);
                var uiManager = FindObjectOfType<UIManager>();
                if (uiManager != null)
                {
                    uiManager.UpdateProgressText(CurrentLevel.winCondition.GetProgressText());
                }
            }
        }

        public void LoadMainMenu()
        {
            SceneManager.LoadScene(menuSceneName);
        }
    }
}
