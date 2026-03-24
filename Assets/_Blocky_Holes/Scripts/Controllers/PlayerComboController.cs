using System.Collections.Generic;
using UnityEngine;

namespace ClawbearGames
{
    public class PlayerComboController : MonoBehaviour
    {
        [SerializeField] private ComboConfig comboConfig;

        private readonly Queue<ScoreSample> scoreWindowSamples = new Queue<ScoreSample>();
        private ComboBarView comboBarView;
        private float scoreInWindow;
        private float comboPoints;
        private int comboLevel;
        private bool isActive;
        private bool comboWasActivated;

        private struct ScoreSample
        {
            public float Time;
            public int Points;

            public ScoreSample(float time, int points)
            {
                Time = time;
                Points = points;
            }
        }

        public void Setup(ComboConfig config)
        {
            if (config != null)
            {
                comboConfig = config;
            }

            EnsureView();
            RefreshUi(false);
        }

        public int EvaluateBonusForPickup(int basePoints)
        {
            if (basePoints <= 0)
            {
                return 0;
            }

            return comboLevel;
        }

        public void RegisterAwardedPoints(int awardedPoints)
        {
            if (awardedPoints <= 0 || comboConfig == null)
            {
                return;
            }

            float now = Time.time;
            scoreWindowSamples.Enqueue(new ScoreSample(now, awardedPoints));
            scoreInWindow += awardedPoints;
            TrimWindow(now);

            bool wasActive = isActive;
            int prevLevel = comboLevel;

            if (!isActive)
            {
                comboPoints = Mathf.Max(0f, scoreInWindow);
                if (scoreInWindow > comboConfig.ActivationScoreThreshold)
                {
                    isActive = true;
                    comboWasActivated = true;
                }
            }
            else
            {
                comboPoints += awardedPoints;
            }

            UpdateComboLevel();

            bool firstActivation = !wasActive && isActive;
            bool levelIncreased = comboLevel > prevLevel;
            RefreshUi(firstActivation || levelIncreased);
        }

        private void Update()
        {
            if (comboConfig == null)
            {
                return;
            }

            float now = Time.time;
            TrimWindow(now);

            bool wasActive = isActive;
            int prevLevel = comboLevel;

            if (!isActive)
            {
                comboPoints = Mathf.Max(0f, scoreInWindow);
                if (scoreInWindow > comboConfig.ActivationScoreThreshold)
                {
                    isActive = true;
                    comboWasActivated = true;
                }
            }
            else
            {
                comboPoints = Mathf.Max(0f, comboPoints - comboConfig.DecayPointsPerSecond * Time.deltaTime);
                if (comboPoints <= 0.001f)
                {
                    comboPoints = 0f;
                    isActive = false;
                    comboWasActivated = false;
                }
            }

            UpdateComboLevel();

            bool firstActivation = !wasActive && isActive;
            bool levelIncreased = comboLevel > prevLevel;
            RefreshUi(firstActivation || levelIncreased);
        }

        private void TrimWindow(float now)
        {
            float maxAge = comboConfig != null ? comboConfig.ActivationWindowSeconds : 5f;
            while (scoreWindowSamples.Count > 0)
            {
                ScoreSample oldest = scoreWindowSamples.Peek();
                if (now - oldest.Time <= maxAge)
                {
                    break;
                }

                scoreInWindow -= oldest.Points;
                scoreWindowSamples.Dequeue();
            }

            if (scoreInWindow < 0f)
            {
                scoreInWindow = 0f;
            }
        }

        private void UpdateComboLevel()
        {
            comboLevel = comboConfig != null ? comboConfig.GetLevelByPoints(comboPoints) : 0;
        }

        private void RefreshUi(bool playFlash)
        {
            EnsureView();
            if (comboBarView == null || comboConfig == null)
            {
                return;
            }

            bool shouldBeVisible = comboWasActivated && comboPoints > 0f;
            comboBarView.SetVisible(shouldBeVisible);
            comboBarView.SetLevel(comboLevel);

            float fillCap = comboConfig.GetFillCapPoints();
            float fillAmount = fillCap > 0f ? Mathf.Clamp01(comboPoints / fillCap) : 0f;
            comboBarView.SetFill(fillAmount);

            if (playFlash)
            {
                comboBarView.PlayFlash(comboConfig.FlashDuration);
            }

        }

        private void EnsureView()
        {
            if (comboBarView == null)
            {
                comboBarView = GetComponentInChildren<ComboBarView>(true);
            }

            if (comboBarView == null)
            {
                comboBarView = gameObject.AddComponent<ComboBarView>();
            }

            if (comboConfig != null)
            {
                comboBarView.Configure(comboConfig);
            }
        }
    }
}
