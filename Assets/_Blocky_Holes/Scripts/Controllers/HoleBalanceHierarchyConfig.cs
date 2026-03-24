using UnityEngine;

namespace ClawbearGames
{
    public class HoleBalanceHierarchyConfig : MonoBehaviour
    {
        [SerializeField] private HoleBalanceLevel[] levels = new HoleBalanceLevel[]
        {
            new HoleBalanceLevel(1, 0, 10, 1f, 0.9f, 3.2f, -1.5f),
            new HoleBalanceLevel(2, 10, 20, 1.4f, 1.1f, 4f, -1.85f),
            new HoleBalanceLevel(3, 30, 100, 1.9f, 1.25f, 4.5f, -2.1f),
            new HoleBalanceLevel(4, 130, 200, 2.45f, 1.45f, 5.5f, -2.55f),
            new HoleBalanceLevel(5, 330, 300, 2.9f, 1.75f, 6f, -2.8f),
            new HoleBalanceLevel(6, 630, 400, 3.3f, 2f, 6.5f, -3f),
            new HoleBalanceLevel(7, 1030, 500, 3.75f, 2.2f, 7f, -3.25f),
            new HoleBalanceLevel(8, 1530, 600, 4.2f, 2.4f, 7.5f, -3.5f),
            new HoleBalanceLevel(9, 2130, 700, 4.8f, 2.6f, 8.5f, -4f),
            new HoleBalanceLevel(10, 2830, 800, 5.5f, 2.8f, 9.5f, -4.5f),
            new HoleBalanceLevel(11, 3630, 900, 5.9f, 3f, 10f, -4.75f),
            new HoleBalanceLevel(12, 4530, 1000, 6.3f, 3.2f, 10.5f, -5f),
            new HoleBalanceLevel(13, 5530, 1000, 6.7f, 3.4f, 11f, -5.25f),
        };

        public bool HasLevels => levels != null && levels.Length > 0;

        public int GetLevelIndexByPoints(int points)
        {
            if (!HasLevels)
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

        public HoleBalanceLevel GetLevelByIndex(int index)
        {
            if (!HasLevels)
            {
                return default;
            }

            index = Mathf.Clamp(index, 0, levels.Length - 1);
            return levels[index];
        }
    }
}
