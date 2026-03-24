using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClawbearGames
{
    public class PlayerComboController : MonoBehaviour
    {
        [Header("Combo Gameplay")]
        [SerializeField][Min(0.1f)] private float activationWindowSeconds = 5f;
        [SerializeField][Min(1)] private int activationScoreThreshold = 100;
        [SerializeField][Min(0f)] private float decayPointsPerSecond = 30f;
        [SerializeField] private int[] comboLevelThresholds = { 100, 200, 300 };

        [Header("Combo UI")]
        [SerializeField][Range(0.05f, 0.6f)] private float flashDuration = 0.2f;

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

        public void Setup()
        {
            EnsureValidConfig();
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
            if (awardedPoints <= 0)
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
                if (scoreInWindow > activationScoreThreshold)
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
            float now = Time.time;
            TrimWindow(now);

            bool wasActive = isActive;
            int prevLevel = comboLevel;

            if (!isActive)
            {
                comboPoints = Mathf.Max(0f, scoreInWindow);
                if (scoreInWindow > activationScoreThreshold)
                {
                    isActive = true;
                    comboWasActivated = true;
                }
            }
            else
            {
                comboPoints = Mathf.Max(0f, comboPoints - decayPointsPerSecond * Time.deltaTime);
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

        private void OnValidate()
        {
            EnsureValidConfig();
        }

        private void EnsureValidConfig()
        {
            activationWindowSeconds = Mathf.Max(0.1f, activationWindowSeconds);
            activationScoreThreshold = Mathf.Max(1, activationScoreThreshold);
            decayPointsPerSecond = Mathf.Max(0f, decayPointsPerSecond);
            flashDuration = Mathf.Clamp(flashDuration, 0.05f, 0.6f);

            if (comboLevelThresholds == null || comboLevelThresholds.Length == 0)
            {
                comboLevelThresholds = new[] { activationScoreThreshold };
                return;
            }

            Array.Sort(comboLevelThresholds);
            for (int i = 0; i < comboLevelThresholds.Length; i++)
            {
                comboLevelThresholds[i] = Mathf.Max(1, comboLevelThresholds[i]);
                if (i > 0 && comboLevelThresholds[i] < comboLevelThresholds[i - 1])
                {
                    comboLevelThresholds[i] = comboLevelThresholds[i - 1];
                }
            }
        }

        private void TrimWindow(float now)
        {
            while (scoreWindowSamples.Count > 0)
            {
                ScoreSample oldest = scoreWindowSamples.Peek();
                if (now - oldest.Time <= activationWindowSeconds)
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
            comboLevel = GetLevelByPoints(comboPoints);
        }

        private int GetLevelByPoints(float points)
        {
            if (comboLevelThresholds == null || comboLevelThresholds.Length == 0)
            {
                return 0;
            }

            int level = 0;
            int pointsRounded = Mathf.FloorToInt(points);
            for (int i = 0; i < comboLevelThresholds.Length; i++)
            {
                if (pointsRounded >= comboLevelThresholds[i])
                {
                    level = i + 1;
                    continue;
                }

                break;
            }

            return level;
        }

        private int GetFillCapPoints()
        {
            if (comboLevelThresholds != null && comboLevelThresholds.Length > 0)
            {
                return Mathf.Max(1, comboLevelThresholds[0]);
            }

            return Mathf.Max(1, activationScoreThreshold);
        }

        private void RefreshUi(bool playFlash)
        {
            EnsureView();
            if (comboBarView == null)
            {
                return;
            }

            bool shouldBeVisible = comboWasActivated && comboPoints > 0f;
            comboBarView.SetVisible(shouldBeVisible);
            comboBarView.SetLevel(comboLevel);

            float fillCap = GetFillCapPoints();
            float fillAmount = fillCap > 0f ? Mathf.Clamp01(comboPoints / fillCap) : 0f;
            comboBarView.SetFill(fillAmount);

            if (playFlash)
            {
                comboBarView.PlayFlash(flashDuration);
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
        }
    }
}
