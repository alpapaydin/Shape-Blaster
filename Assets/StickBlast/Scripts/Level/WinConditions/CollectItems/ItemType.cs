using UnityEngine;

namespace StickBlast.Level
{
    [CreateAssetMenu(fileName = "ItemType", menuName = "StickBlast/Item Type")]
    public class ItemType : ScriptableObject
    {
        public string itemName;
        public Sprite icon;
        public GameObject prefab;  // Moved from ItemRequirement
    }
}
