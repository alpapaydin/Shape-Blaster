using UnityEngine;
using TMPro;
using StickBlast.Level;

namespace StickBlast.UI
{
    public class PointsGoalUI : BaseGoalUI
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private RectTransform fillBar;
        
        private const float MIN_WIDTH = 75f;
        private const float MAX_WIDTH = 393f;
        
        private float currentWidth;
        private float targetWidth;
        private float lerpSpeed = 10f;

        private void Update()
        {
            if (currentWidth != targetWidth)
            {
                currentWidth = Mathf.Lerp(currentWidth, targetWidth, Time.deltaTime * lerpSpeed);
                UpdateFillWidth(currentWidth);
            }
        }

        public override void UpdateProgress(int current, int total)
        {
            scoreText.text = $"{current}/{total}";
            float ratio = Mathf.Clamp01((float)current / total);
            targetWidth = Mathf.Lerp(MIN_WIDTH, MAX_WIDTH, ratio);
        }

        public override void Initialize(WinCondition winCondition)
        {
            if (winCondition is PointWinCondition pointWin)
            {
                UpdateProgress(0, pointWin.PointsToWin);
            }
        }

        private void UpdateFillWidth(float width)
        {
            if (fillBar != null)
            {
                fillBar.sizeDelta = new Vector2(width, fillBar.sizeDelta.y);
            }
        }
    }
}
