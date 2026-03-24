using System.Collections.Generic;
using UnityEngine;

namespace ClawbearGames
{
    public class PlayerComboController : MonoBehaviour
    {
        private enum ComboUiEvent
        {
            IdleUpdate,
            Show,
            LevelUp,
            Hide
        }

        private const string DefaultComboConfigPath = "Configs/ComboConfig";

        [Header("Combo Config")]
        [SerializeField] private ComboConfig comboConfig;

        private readonly Queue<ScoreSample> scoreWindowSamples = new Queue<ScoreSample>();
        private ComboBarView comboBarView;
        private float scoreInWindow;
        private float comboPoints;
        private int comboLevel;
        private bool isActive;
        private bool comboWasActivated;

        public bool IsComboActive => isActive && comboPoints > 0f;
        public int CurrentComboLevel => comboLevel;

        private void Awake()
        {
            EnsureConfig();
        }

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
            EnsureConfig();
            EnsureView();
            RefreshUi(ComboUiEvent.Hide);
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
            int previousLevel = comboLevel;

            if (!isActive)
            {
                comboPoints = Mathf.Max(0f, scoreInWindow);
                if (scoreInWindow >= GetActivationScoreThreshold())
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
            ComboUiEvent uiEvent = ResolveUiEvent(wasActive, isActive, previousLevel, comboLevel);
            RefreshUi(uiEvent);
        }

        private void Update()
        {
            float now = Time.time;
            TrimWindow(now);

            bool wasActive = isActive;
            int previousLevel = comboLevel;

            if (!isActive)
            {
                comboPoints = Mathf.Max(0f, scoreInWindow);
                if (scoreInWindow >= GetActivationScoreThreshold())
                {
                    isActive = true;
                    comboWasActivated = true;
                }
            }
            else
            {
                comboPoints = Mathf.Max(0f, comboPoints - GetDecayPointsPerSecond() * Time.deltaTime);
                if (comboPoints <= 0.001f)
                {
                    comboPoints = 0f;
                    isActive = false;
                    comboWasActivated = false;
                }
            }

            UpdateComboLevel();
            ComboUiEvent uiEvent = ResolveUiEvent(wasActive, isActive, previousLevel, comboLevel);
            RefreshUi(uiEvent);
        }

        private void OnValidate()
        {
            EnsureConfig();
        }

        private void EnsureConfig()
        {
            if (comboConfig != null)
            {
                return;
            }

            comboConfig = Resources.Load<ComboConfig>(DefaultComboConfigPath);
            if (comboConfig == null)
            {
                Debug.LogWarning($"[{nameof(PlayerComboController)}] {nameof(ComboConfig)} is not assigned and fallback load from Resources/{DefaultComboConfigPath} failed.", this);
            }
        }

        private ComboUiEvent ResolveUiEvent(bool wasActive, bool activeNow, int previousLevel, int levelNow)
        {
            if (!wasActive && activeNow)
            {
                return ComboUiEvent.Show;
            }

            if (wasActive && !activeNow)
            {
                return ComboUiEvent.Hide;
            }

            if (activeNow && levelNow > previousLevel)
            {
                return ComboUiEvent.LevelUp;
            }

            return ComboUiEvent.IdleUpdate;
        }

        private void TrimWindow(float now)
        {
            while (scoreWindowSamples.Count > 0)
            {
                ScoreSample oldest = scoreWindowSamples.Peek();
                if (now - oldest.Time <= GetActivationWindowSeconds())
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

        private void RefreshUi(ComboUiEvent uiEvent)
        {
            EnsureView();
            if (comboBarView == null)
            {
                return;
            }

            float fillCap = comboConfig != null ? comboConfig.GetFillCapPoints() : 1f;
            float fillAmount = fillCap > 0f ? Mathf.Clamp01(comboPoints / fillCap) : 0f;

            switch (uiEvent)
            {
                case ComboUiEvent.Show:
                    comboBarView.PlayShow(comboLevel, fillAmount, GetFlashDuration());
                    break;
                case ComboUiEvent.LevelUp:
                    comboBarView.PlayLevelUp(comboLevel, fillAmount, GetFlashDuration());
                    break;
                case ComboUiEvent.Hide:
                    comboBarView.PlayHide();
                    break;
                default:
                    bool shouldBeVisible = comboWasActivated && comboPoints > 0f;
                    if (shouldBeVisible)
                    {
                        comboBarView.ApplyIdleUpdate(comboLevel, fillAmount);
                    }
                    else
                    {
                        comboBarView.ApplyIdleUpdate(0, 0f);
                    }
                    break;
            }
        }

        private void EnsureView()
        {
            if (comboBarView == null)
            {
                comboBarView = GetComponent<ComboBarView>();
            }

            if (comboBarView == null)
            {
                comboBarView = GetComponentInChildren<ComboBarView>(true);
            }

            if (comboBarView == null)
            {
                comboBarView = gameObject.AddComponent<ComboBarView>();
            }

            ComboBarView[] allViews = GetComponentsInChildren<ComboBarView>(true);
            for (int i = 0; i < allViews.Length; i++)
            {
                if (allViews[i] != comboBarView)
                {
                    allViews[i].enabled = false;
                }
            }

            comboBarView.Configure(comboConfig);
        }

        private float GetActivationWindowSeconds()
        {
            return comboConfig != null ? comboConfig.ActivationWindowSeconds : 0.1f;
        }

        private int GetActivationScoreThreshold()
        {
            return comboConfig != null ? comboConfig.ActivationScoreThreshold : 1;
        }

        private float GetDecayPointsPerSecond()
        {
            return comboConfig != null ? comboConfig.DecayPointsPerSecond : 0f;
        }

        private float GetFlashDuration()
        {
            return comboConfig != null ? comboConfig.FlashDuration : 0.2f;
        }
    }
}
