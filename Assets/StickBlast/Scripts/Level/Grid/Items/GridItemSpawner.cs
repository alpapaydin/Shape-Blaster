using UnityEngine;
using System.Collections.Generic;
using StickBlast.Level;
using System.Linq;

namespace StickBlast.Grid
{
    public class GridItemSpawner : MonoBehaviour
    {
        [SerializeField] private float spawnInterval = 5f;
        [SerializeField] private int initialSpawnCount = 3;
        [SerializeField] private int maxConcurrentItems = 5;

        private GridState gridState;
        private LevelDefinition currentLevel;
        private Dictionary<ItemType, int> spawnedItemCounts = new Dictionary<ItemType, int>();
        private float nextSpawnTime;

        public void Initialize(GridState state, LevelDefinition level)
        {
            gridState = state;
            currentLevel = level;
            spawnedItemCounts.Clear();
            
            if (currentLevel.winCondition is CollectItemsWinCondition collectWin)
            {
                foreach (var req in collectWin.requiredItems)
                {
                    spawnedItemCounts[req.itemType] = 0;
                }

                for (int i = 0; i < initialSpawnCount; i++)
                {
                    SpawnRandomItem();
                }
            }
            nextSpawnTime = Time.time + spawnInterval;
        }

        private void Update()
        {
            if (currentLevel == null || gridState == null) return;
            
            if (Time.time >= nextSpawnTime && ShouldSpawnMore())
            {
                SpawnRandomItem();
                nextSpawnTime = Time.time + spawnInterval;
            }
        }

        private bool ShouldSpawnMore()
        {
            if (currentLevel == null || gridState == null || currentLevel.winCondition == null) 
                return false;

            if (currentLevel.winCondition is not CollectItemsWinCondition collectWin) 
                return false;

            int totalCurrentItems = CountCurrentItems();
            if (totalCurrentItems >= maxConcurrentItems)
                return false;

            return collectWin.requiredItems.Any(req => 
                spawnedItemCounts.TryGetValue(req.itemType, out int count) && 
                count < req.requiredAmount);
        }

        private int CountCurrentItems()
        {
            int count = 0;
            for (int x = 0; x < gridState.Width - 1; x++)
            {
                for (int y = 0; y < gridState.Height - 1; y++)
                {
                    if (gridState.Cells[x, y].HasCollectible)
                        count++;
                }
            }
            return count;
        }

        private void SpawnRandomItem()
        {
            if (currentLevel.winCondition is not CollectItemsWinCondition collectWin)
            {
                return;
            }

            var availableItems = collectWin.requiredItems
                .Where(req => spawnedItemCounts.TryGetValue(req.itemType, out int count) && 
                             count < req.requiredAmount && req.itemType.prefab != null)
                .ToList();

            if (!availableItems.Any())
                return;

            var availableCells = GetAvailableCells();

            if (!availableCells.Any())
                return;

            var randomItem = availableItems[Random.Range(0, availableItems.Count)];
            var randomCell = availableCells[Random.Range(0, availableCells.Count)];

            randomCell.SetCollectible(randomItem.itemType.prefab, randomItem.itemType);
            spawnedItemCounts[randomItem.itemType]++;
        }

        private List<Cell> GetAvailableCells()
        {
            var cells = new List<Cell>();
            for (int x = 0; x < gridState.Width - 1; x++)
            {
                for (int y = 0; y < gridState.Height - 1; y++)
                {
                    var cell = gridState.Cells[x, y];
                    if (!cell.IsComplete() && !cell.HasCollectible)
                    {
                        cells.Add(cell);
                    }
                }
            }
            return cells;
        }
    }
}
