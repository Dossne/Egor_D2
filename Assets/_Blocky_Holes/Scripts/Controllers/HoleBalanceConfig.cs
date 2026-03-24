using System;
using UnityEngine;

namespace ClawbearGames
{
    [CreateAssetMenu(menuName = "BlockyHoles/Hole Balance Config", fileName = "HoleBalanceConfig")]
    public class HoleBalanceConfig : ScriptableObject
    {
        [SerializeField] private HoleBalanceLevel[] levels = Array.Empty<HoleBalanceLevel>();

        public HoleBalanceLevel[] Levels => levels;

        public HoleBalanceLevel GetLevelByIndex(int index)
        {
            if (levels == null || levels.Length == 0)
            {
                throw new InvalidOperationException("HoleBalanceConfig does not contain any levels.");
            }

            index = Mathf.Clamp(index, 0, levels.Length - 1);
            return levels[index];
        }

        public int GetLevelIndexByPoints(int points)
        {
            if (levels == null || levels.Length == 0)
            {
                return 0;
            }

            int result = 0;
            for (int i = 0; i < levels.Length; i++)
            {
                if (points >= levels[i].PointsSum)
                {
                    result = i;
                }
                else
                {
                    break;
                }
            }

            return result;
        }
    }

    [Serializable]
    public struct HoleBalanceLevel
    {
        [SerializeField] private int level;
        [SerializeField] private int pointsSum;
        [SerializeField] private int pointsToNext;
        [SerializeField] private float heroScale;
        [SerializeField] private float heroMoveSpeed;
        [SerializeField] private float cameraY;
        [SerializeField] private float cameraZ;

        public int Level => level;
        public int PointsSum => pointsSum;
        public int PointsToNext => pointsToNext;
        public float HeroScale => heroScale;
        public float HeroMoveSpeed => heroMoveSpeed;
        public float CameraY => cameraY;
        public float CameraZ => cameraZ;

        public int NextPointsSum => pointsSum + Mathf.Max(1, pointsToNext);

        public HoleBalanceLevel(int level, int pointsSum, int pointsToNext, float heroScale, float heroMoveSpeed, float cameraY, float cameraZ)
        {
            this.level = level;
            this.pointsSum = pointsSum;
            this.pointsToNext = pointsToNext;
            this.heroScale = heroScale;
            this.heroMoveSpeed = heroMoveSpeed;
            this.cameraY = cameraY;
            this.cameraZ = cameraZ;
        }
    }
}
