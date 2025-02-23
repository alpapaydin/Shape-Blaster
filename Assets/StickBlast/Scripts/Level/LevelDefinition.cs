using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StickBlast.Sticks;
using StickBlast.Level;

[CreateAssetMenu(fileName = "Level", menuName = "StickBlast/Level Definition")]
public class LevelDefinition : ScriptableObject
{
    [Header("Grid Settings")]
    public int width = 6;
    public int height = 6;

    [Header("Win Condition")]
    public WinCondition winCondition;

    [Header("Available Sticks")]
    public StickDefinition[] availableSticks;
    public int maxStickCount = 3;

    [Header("Collectible Settings")]
    public bool hasCollectibles;
    [System.Serializable]
    public class CollectibleSetting
    {
        public ItemType itemType;
        public GameObject prefab;
        [Range(0f, 1f)]
        public float spawnChance = 0.3f;
    }
    public CollectibleSetting[] collectibleSettings;
}
