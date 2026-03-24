using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BlockyHoles.Prototype
{
    public enum PrototypeGameState
    {
        Playing,
        Win,
        Lose,
    }

    public interface IHapticsService
    {
        void Selection();
        void Medium();
    }

    public sealed class DummyHapticsService : IHapticsService
    {
        public void Selection() { }
        public void Medium() { }
    }

    public class CollectableItem : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private string collectableId;
        [SerializeField] private int points = 1;
        [SerializeField] private int sizeTier = 1;
        [SerializeField] private bool isTarget;

        [Header("Physics")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Collider itemCollider;
        [SerializeField] private float destroyDelay = 0.05f;

        public string CollectableId => collectableId;
        public int Points => points;
        public int SizeTier => sizeTier;
        public bool IsTarget => isTarget;

        private bool isFalling;
        private bool isCollected;

        public event Action<CollectableItem> Collected;

        private void Reset()
        {
            rb = GetComponent<Rigidbody>();
            itemCollider = GetComponent<Collider>();
        }

        public bool CanBeConsumed(int currentLevel)
        {
            return sizeTier <= currentLevel;
        }

        public void TriggerFall(int currentLevel)
        {
            if (isFalling || isCollected || !CanBeConsumed(currentLevel))
            {
                return;
            }

            isFalling = true;
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }

        public void Consume()
        {
            if (isCollected)
            {
                return;
            }

            isCollected = true;
            Collected?.Invoke(this);
            Destroy(gameObject, destroyDelay);
        }
    }

    public class HoleMovementController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform holeVisual;

        [Header("Movement")]
        [SerializeField] private Vector2 xBounds = new Vector2(-7f, 7f);
        [SerializeField] private Vector2 zBounds = new Vector2(-11f, 11f);

        public Vector2 InputVector { get; private set; }
        public float MoveSpeed { get; private set; } = 0.9f;

        public void SetSpeed(float speed)
        {
            MoveSpeed = Mathf.Max(0f, speed);
        }

        public void SetScale(float scale)
        {
            if (holeVisual != null)
            {
                holeVisual.localScale = new Vector3(scale, 1f, scale);
            }
        }

        public void SetInput(Vector2 input)
        {
            InputVector = input;
        }

        private void Update()
        {
            if (InputVector.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector3 step = new Vector3(InputVector.x, 0f, InputVector.y) * (MoveSpeed * Time.deltaTime);
            Vector3 next = transform.position + step;
            next.x = Mathf.Clamp(next.x, xBounds.x, xBounds.y);
            next.z = Mathf.Clamp(next.z, zBounds.x, zBounds.y);
            transform.position = next;
        }
    }

    public class HoleTriggerController : MonoBehaviour
    {
        [SerializeField] private HoleLevelSystem levelSystem;

        private readonly HashSet<CollectableItem> fallingItems = new HashSet<CollectableItem>();

        public event Action<CollectableItem> ItemCollected;

        public void HandleLayerChangeEnter(Collider other)
        {
            var item = other.GetComponentInParent<CollectableItem>();
            if (item == null)
            {
                return;
            }

            item.TriggerFall(levelSystem.CurrentLevel);
            fallingItems.Add(item);
        }

        public void HandleItemCollectEnter(Collider other)
        {
            var item = other.GetComponentInParent<CollectableItem>();
            if (item == null || !fallingItems.Contains(item))
            {
                return;
            }

            if (!item.CanBeConsumed(levelSystem.CurrentLevel))
            {
                return;
            }

            item.Consume();
            ItemCollected?.Invoke(item);
            fallingItems.Remove(item);
        }
    }

    public class HoleTriggerZone : MonoBehaviour
    {
        public enum ZoneType
        {
            LayerChange,
            ItemCollect,
        }

        [SerializeField] private HoleTriggerController owner;
        [SerializeField] private ZoneType zoneType;

        private void OnTriggerEnter(Collider other)
        {
            if (owner == null)
            {
                return;
            }

            if (zoneType == ZoneType.LayerChange)
            {
                owner.HandleLayerChangeEnter(other);
                return;
            }

            owner.HandleItemCollectEnter(other);
        }
    }

    public class HoleLevelSystem : MonoBehaviour
    {
        [SerializeField] private HoleLevelConfig config;

        public int CurrentLevel { get; private set; } = 1;
        public int TotalPoints { get; private set; }

        public event Action<int, HoleLevelDefinition> LevelUp;
        public event Action<int, int, int> ProgressChanged;

        public void Setup(HoleLevelConfig levelConfig)
        {
            config = levelConfig;
            CurrentLevel = 1;
            TotalPoints = 0;
            var def = config.GetByLevel(1);
            ProgressChanged?.Invoke(0, 0, def.PointsToNext);
        }

        public HoleLevelDefinition GetCurrentDefinition() => config.GetByLevel(CurrentLevel);

        public void AddPoints(int points)
        {
            if (points <= 0)
            {
                return;
            }

            TotalPoints += points;
            while (CurrentLevel < config.Levels.Count)
            {
                var current = config.GetByLevel(CurrentLevel);
                int nextPointsSum = current.PointsSum + current.PointsToNext;
                if (TotalPoints < nextPointsSum)
                {
                    break;
                }

                CurrentLevel++;
                var def = config.GetByLevel(CurrentLevel);
                LevelUp?.Invoke(CurrentLevel, def);
            }

            var active = config.GetByLevel(CurrentLevel);
            int currentSegmentStart = active.PointsSum;
            int currentInSegment = Mathf.Clamp(TotalPoints - currentSegmentStart, 0, active.PointsToNext);
            ProgressChanged?.Invoke(TotalPoints, currentInSegment, active.PointsToNext);
        }
    }

    public class CameraFollowController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private CameraConfig config;
        [SerializeField] private Vector3 offset = new Vector3(0f, 3.2f, -1.5f);

        private Coroutine moveRoutine;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            transform.position = target.position + offset;
        }

        public void OnLevelUpdated(HoleLevelDefinition def)
        {
            Vector3 newOffset = new Vector3(0f, def.CameraY, def.CameraZ);
            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
            }

            moveRoutine = StartCoroutine(CRMoveOffset(newOffset));
        }

        private IEnumerator CRMoveOffset(Vector3 targetOffset)
        {
            Vector3 start = offset;
            float duration = Mathf.Max(0.01f, config != null ? config.CameraMoveDuration : 0.25f);
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                offset = Vector3.Lerp(start, targetOffset, t / duration);
                yield return null;
            }

            offset = targetOffset;
        }
    }

    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private CanvasGroup rootGroup;
        [SerializeField] private RectTransform stickRoot;
        [SerializeField] private RectTransform knob;
        [SerializeField] private float maxRadius = 80f;

        public Vector2 InputVector { get; private set; }
        public event Action<Vector2> InputChanged;

        private Canvas canvas;

        private void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
            SetVisible(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            SetVisible(true);
            stickRoot.position = eventData.position;
            knob.position = eventData.position;
            UpdateInput(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateInput(eventData.position);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            InputVector = Vector2.zero;
            knob.position = stickRoot.position;
            InputChanged?.Invoke(InputVector);
            SetVisible(false);
        }

        private void UpdateInput(Vector2 pointerPos)
        {
            Vector2 delta = pointerPos - (Vector2)stickRoot.position;
            Vector2 clamped = Vector2.ClampMagnitude(delta, maxRadius);
            knob.position = stickRoot.position + (Vector3)clamped;
            InputVector = clamped / maxRadius;
            InputChanged?.Invoke(InputVector.normalized * Mathf.Clamp01(InputVector.magnitude));
        }

        private void SetVisible(bool visible)
        {
            rootGroup.alpha = visible ? 0.35f : 0f;
            rootGroup.blocksRaycasts = visible;
        }
    }
}
