using System;
using UnityEngine;

namespace ClawbearGames
{
    [CreateAssetMenu(menuName = "BlockyHoles/Combo Config", fileName = "ComboConfig")]
    public class ComboConfig : ScriptableObject
    {
        [Header("Gameplay")]
        [SerializeField][Min(0.1f)] private float activationWindowSeconds = 5f;
        [SerializeField][Min(1)] private int activationScoreThreshold = 100;
        [SerializeField][Min(0f)] private float decayPointsPerSecond = 30f;
        [SerializeField] private int[] comboLevelThresholds = { 100, 200, 300 };

        [Header("UI")]
        [SerializeField] private Vector2 uiScreenOffset = new Vector2(0f, 145f);
        [SerializeField][Range(0.05f, 0.6f)] private float flashDuration = 0.2f;

        public float ActivationWindowSeconds => activationWindowSeconds;
        public int ActivationScoreThreshold => activationScoreThreshold;
        public float DecayPointsPerSecond => decayPointsPerSecond;
        public Vector2 UiScreenOffset => uiScreenOffset;
        public float FlashDuration => flashDuration;

        public int GetFillCapPoints()
        {
            if (comboLevelThresholds != null && comboLevelThresholds.Length > 0)
            {
                return Mathf.Max(1, comboLevelThresholds[0]);
            }

            return Mathf.Max(1, activationScoreThreshold);
        }

        public int GetLevelByPoints(float comboPoints)
        {
            if (comboLevelThresholds == null || comboLevelThresholds.Length == 0)
            {
                return 0;
            }

            int level = 0;
            int pointsRounded = Mathf.FloorToInt(comboPoints);
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

        private void OnValidate()
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
    }
}
