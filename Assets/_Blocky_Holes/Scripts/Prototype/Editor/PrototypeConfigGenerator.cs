#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BlockyHoles.Prototype.Editor
{
    public static class PrototypeConfigGenerator
    {
        [MenuItem("Tools/BlockyHoles/Prototype/Create Default Configs")]
        public static void CreateDefaults()
        {
            const string root = "Assets/_Blocky_Holes/Settings/Prototype";
            if (!AssetDatabase.IsValidFolder(root))
            {
                AssetDatabase.CreateFolder("Assets/_Blocky_Holes/Settings", "Prototype");
            }

            CreateLevelConfig(root + "/HoleLevelConfig.asset");
            CreateAsset<HoleGrowthConfig>(root + "/HoleGrowthConfig.asset");
            CreateAsset<ProgressBarConfig>(root + "/ProgressBarConfig.asset");
            CreateAsset<CameraConfig>(root + "/CameraConfig.asset");
            CreateAsset<LevelUpNotificationConfig>(root + "/LevelUpNotificationConfig.asset");
            CreateAsset<PointsNotificationConfig>(root + "/PointsNotificationConfig.asset");
            CreateAsset<TargetItemFlyConfig>(root + "/TargetItemFlyConfig.asset");
            CreateAsset<ArrowIndicatorConfig>(root + "/ArrowIndicatorConfig.asset");
            CreateAsset<CollectablesConfig>(root + "/CollectablesConfig.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Prototype config assets generated.");
        }

        private static T CreateAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void CreateLevelConfig(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<HoleLevelConfig>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<HoleLevelConfig>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.Levels = new List<HoleLevelDefinition>
            {
                new HoleLevelDefinition {Level = 1, PointsToNext = 10, PointsSum = 0, HeroScale = 1.0f, HeroMoveSpeed = 0.9f, CameraY = 5.2f, CameraZ = -3.2f},
                new HoleLevelDefinition {Level = 2, PointsToNext = 20, PointsSum = 10, HeroScale = 1.4f, HeroMoveSpeed = 1.1f, CameraY = 6.0f, CameraZ = -3.55f},
                new HoleLevelDefinition {Level = 3, PointsToNext = 100, PointsSum = 30, HeroScale = 1.9f, HeroMoveSpeed = 1.25f, CameraY = 6.5f, CameraZ = -3.8f},
                new HoleLevelDefinition {Level = 4, PointsToNext = 200, PointsSum = 130, HeroScale = 2.45f, HeroMoveSpeed = 1.45f, CameraY = 7.5f, CameraZ = -4.25f},
                new HoleLevelDefinition {Level = 5, PointsToNext = 300, PointsSum = 330, HeroScale = 2.9f, HeroMoveSpeed = 1.75f, CameraY = 8.0f, CameraZ = -4.5f},
                new HoleLevelDefinition {Level = 6, PointsToNext = 400, PointsSum = 630, HeroScale = 3.3f, HeroMoveSpeed = 2.0f, CameraY = 8.5f, CameraZ = -4.7f},
                new HoleLevelDefinition {Level = 7, PointsToNext = 500, PointsSum = 1030, HeroScale = 3.75f, HeroMoveSpeed = 2.2f, CameraY = 9.0f, CameraZ = -4.95f},
                new HoleLevelDefinition {Level = 8, PointsToNext = 600, PointsSum = 1530, HeroScale = 4.2f, HeroMoveSpeed = 2.4f, CameraY = 9.5f, CameraZ = -5.2f},
                new HoleLevelDefinition {Level = 9, PointsToNext = 700, PointsSum = 2130, HeroScale = 4.8f, HeroMoveSpeed = 2.6f, CameraY = 10.5f, CameraZ = -5.7f},
                new HoleLevelDefinition {Level = 10, PointsToNext = 800, PointsSum = 2830, HeroScale = 5.5f, HeroMoveSpeed = 2.8f, CameraY = 11.5f, CameraZ = -6.2f},
                new HoleLevelDefinition {Level = 11, PointsToNext = 900, PointsSum = 3630, HeroScale = 5.9f, HeroMoveSpeed = 3.0f, CameraY = 12.0f, CameraZ = -6.45f},
                new HoleLevelDefinition {Level = 12, PointsToNext = 1000, PointsSum = 4530, HeroScale = 6.3f, HeroMoveSpeed = 3.2f, CameraY = 12.5f, CameraZ = -6.7f},
                new HoleLevelDefinition {Level = 13, PointsToNext = 1000, PointsSum = 5530, HeroScale = 6.7f, HeroMoveSpeed = 3.4f, CameraY = 13.0f, CameraZ = -6.95f},
                new HoleLevelDefinition {Level = 14, PointsToNext = 1000, PointsSum = 6530, HeroScale = 7.1f, HeroMoveSpeed = 3.6f, CameraY = 13.5f, CameraZ = -7.2f},
                new HoleLevelDefinition {Level = 15, PointsToNext = 1000, PointsSum = 7530, HeroScale = 7.5f, HeroMoveSpeed = 3.8f, CameraY = 14.0f, CameraZ = -7.45f},
            };
            EditorUtility.SetDirty(asset);
        }
    }
}
#endif
