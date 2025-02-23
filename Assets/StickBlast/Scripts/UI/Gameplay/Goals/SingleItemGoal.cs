using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace StickBlast.UI
{
    public class SingleItemGoal : MonoBehaviour
    {
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI remainingCount;
        [SerializeField] private GameObject completedTick;

        public void Initialize(Sprite icon, int required)
        {
            itemIcon.sprite = icon;
            remainingCount.text = required.ToString();
            completedTick.SetActive(false);
        }

        public void UpdateProgress(int current, int required)
        {
            if (completedTick == null || remainingCount == null) return;

            if (current >= required)
            {
                SoundManager.Instance.PlaySound("objectiveComplete");
                completedTick.SetActive(true);
                remainingCount.gameObject.SetActive(false);
            }
            else
            {
                completedTick.SetActive(false);
                remainingCount.gameObject.SetActive(true);
                remainingCount.text = $"{current}/{required}";
            }
        }
    }
}
