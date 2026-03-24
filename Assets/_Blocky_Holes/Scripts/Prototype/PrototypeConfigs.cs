using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockyHoles.Prototype
{
    public enum CollectableType
    {
        Ball,
        GiftBox,
        Hat,
        House,
        Building,
        Grave,
        Pine,
        Custom,
    }

    [Serializable]
    public struct HoleLevelDefinition
    {
        [Min(1)] public int Level;
        [Min(0)] public int PointsToNext;
        [Min(0)] public int PointsSum;
        [Min(0.1f)] public float HeroScale;
        [Min(0.1f)] public float HeroMoveSpeed;
        public float CameraY;
        public float CameraZ;
    }

    [CreateAssetMenu(menuName = "BlockyHoles/Prototype/Hole Level Config")]
    public class HoleLevelConfig : ScriptableObject
    {
        public List<HoleLevelDefinition> Levels = new List<HoleLevelDefinition>();

        public HoleLevelDefinition GetByLevel(int level)
        {
            if (Levels == null || Levels.Count == 0)
            {
                throw new InvalidOperationException("HoleLevelConfig is empty.");
            }

            level = Mathf.Clamp(level, 1, Levels.Count);
            return Levels[level - 1];
        }
    }

    [CreateAssetMenu(menuName = "BlockyHoles/Prototype/Hole Growth Config")]
    public class HoleGrowthConfig : ScriptableObject
    {
        [Min(0.01f)] public float GrowthDuration = 0.22f;
    }

    [CreateAssetMenu(menuName = "BlockyHoles/Prototype/Progress Bar Config")]
    public class ProgressBarConfig : ScriptableObject
    {
        [Min(0.01f)] public float MinFillDuration = 0.1f;
        [Min(0.05f)] public float MaxFillDuration = 0.5f;
        [Min(0.05f)] public float LevelTextBounceDuration = 0.16f;
        [Min(1f)] public float LevelTextBounceScale = 1.12f;
    }

    [CreateAssetMenu(menuName = "BlockyHoles/Prototype/Camera Config")]
    public class CameraConfig : ScriptableObject
    {
        [Min(0.01f)] public float CameraMoveDuration = 0.3f;
    }

    [CreateAssetMenu(menuName = "BlockyHoles/Prototype/Level Up Notification Config")]
    public class LevelUpNotificationConfig : ScriptableObject
    {
        [Min(0.01f)] public float Duration = 0.8f;
        [Min(0f)] public float Delay = 0f;
        public float YOffset = 1.2f;
    }

    [CreateAssetMenu(menuName = "BlockyHoles/Prototype/Points Notification Config")]
    public class PointsNotificationConfig : ScriptableObject
    {
        [Min(0.01f)] public float Duration = 0.7f;
        [Min(0f)] public float Delay = 0f;
        public float YOffset = 0.7f;
        public float MinXOffset = -0.2f;
        public float MaxXOffset = 0.2f;
        public float MinYOffset = 0.0f;
        public float MaxYOffset = 0.3f;
    }

    [CreateAssetMenu(menuName = "BlockyHoles/Prototype/Target Item Fly Config")]
    public class TargetItemFlyConfig : ScriptableObject
    {
        [Min(0.05f)] public float FlyDuration = 0.6f;
        [Min(0f)] public float ArcHeight = 90f;
        [Min(0.1f)] public float ScaleMultiplier = 0.9f;
        [Min(0.05f)] public float CounterBounceDuration = 0.16f;
        [Min(1f)] public float CounterBounceScale = 1.15f;
    }

    [CreateAssetMenu(menuName = "BlockyHoles/Prototype/Arrow Indicator Config")]
    public class ArrowIndicatorConfig : ScriptableObject
    {
        [Min(0.1f)] public float BaseSize = 0.45f;
        [Min(0.1f)] public float ScaleMultiplier = 0.3f;
        [Min(0.1f)] public float DistanceFromCenter = 1.2f;
        [Min(0.01f)] public float ShowHideDuration = 0.1f;
    }

    [Serializable]
    public class CollectableDefinition
    {
        public string Id;
        public CollectableType CollectableType;
        public string IconName;
        [Min(0)] public int Cost = 1;
        [Min(1)] public int SizeTier = 1;
        public bool IsTarget;
        public GameObject Prefab;
    }

    [CreateAssetMenu(menuName = "BlockyHoles/Prototype/Collectables Config")]
    public class CollectablesConfig : ScriptableObject
    {
        public List<CollectableDefinition> Definitions = new List<CollectableDefinition>();
    }
}
