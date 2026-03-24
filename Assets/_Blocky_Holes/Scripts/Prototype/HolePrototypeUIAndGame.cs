using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BlockyHoles.Prototype
{
    public class ArrowIndicatorController : MonoBehaviour
    {
        [SerializeField] private Transform hole;
        [SerializeField] private Transform arrowVisual;
        [SerializeField] private ArrowIndicatorConfig config;

        private Vector2 input;
        private float holeScale = 1f;

        public void SetInput(Vector2 value)
        {
            input = value;
            if (arrowVisual != null)
            {
                arrowVisual.gameObject.SetActive(value.sqrMagnitude > 0.001f);
            }
        }

        public void SetHoleScale(float scale)
        {
            holeScale = scale;
        }

        private void LateUpdate()
        {
            if (hole == null || arrowVisual == null || input.sqrMagnitude < 0.001f)
            {
                return;
            }

            Vector3 dir = new Vector3(input.x, 0f, input.y).normalized;
            float distance = config.DistanceFromCenter * holeScale;
            arrowVisual.position = hole.position + dir * distance + Vector3.up * 0.05f;
            arrowVisual.forward = dir;
            float size = config.BaseSize + (holeScale * config.ScaleMultiplier);
            arrowVisual.localScale = new Vector3(size, size, size);
        }
    }

    public class ProgressBarController : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private ProgressBarConfig config;

        private Coroutine fillRoutine;

        public void AnimateTo(int current, int max)
        {
            float target = max <= 0 ? 1f : Mathf.Clamp01(current / (float)max);
            if (fillRoutine != null)
            {
                StopCoroutine(fillRoutine);
            }

            fillRoutine = StartCoroutine(CRFill(target));
        }

        public void Pulse()
        {
            StartCoroutine(CRPulse());
        }

        private IEnumerator CRFill(float target)
        {
            float start = fillImage.fillAmount;
            float delta = Mathf.Abs(target - start);
            float duration = Mathf.Lerp(config.MinFillDuration, config.MaxFillDuration, delta);
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                fillImage.fillAmount = Mathf.Lerp(start, target, t / duration);
                yield return null;
            }

            fillImage.fillAmount = target;
        }

        private IEnumerator CRPulse()
        {
            Vector3 start = transform.localScale;
            Vector3 peak = start * 1.08f;
            float t = 0f;
            while (t < 0.09f)
            {
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(start, peak, t / 0.09f);
                yield return null;
            }

            t = 0f;
            while (t < 0.1f)
            {
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(peak, start, t / 0.1f);
                yield return null;
            }

            transform.localScale = start;
        }
    }

    public class HoleLevelTextController : MonoBehaviour
    {
        [SerializeField] private Text levelText;
        [SerializeField] private ProgressBarConfig config;

        private Coroutine bounceRoutine;

        public void SetLevel(int level)
        {
            levelText.text = $"Size {level}";
            levelText.color = Color.Lerp(Color.white, new Color(0.2f, 0.2f, 0.2f), (level - 1f) / 14f);

            if (bounceRoutine != null)
            {
                StopCoroutine(bounceRoutine);
            }

            bounceRoutine = StartCoroutine(CRBounce());
        }

        private IEnumerator CRBounce()
        {
            Vector3 start = levelText.transform.localScale;
            Vector3 peak = Vector3.one * config.LevelTextBounceScale;
            float duration = config.LevelTextBounceDuration;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float n = t / duration;
                levelText.transform.localScale = Vector3.Lerp(start, peak, n);
                yield return null;
            }

            t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                levelText.transform.localScale = Vector3.Lerp(peak, start, t / duration);
                yield return null;
            }

            levelText.transform.localScale = start;
        }
    }

    public class PrototypeGameController : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private HoleMovementController movement;
        [SerializeField] private HoleTriggerController triggerController;
        [SerializeField] private HoleLevelSystem levelSystem;
        [SerializeField] private CameraFollowController cameraFollow;
        [SerializeField] private ArrowIndicatorController arrowIndicator;

        [Header("Input")]
        [SerializeField] private VirtualJoystick joystick;

        [Header("UI")]
        [SerializeField] private ProgressBarController progressBar;
        [SerializeField] private HoleLevelTextController levelText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text resultText;

        [Header("Config")]
        [SerializeField] private HoleLevelConfig holeLevelConfig;
        [SerializeField] private HoleGrowthConfig growthConfig;
        [SerializeField] private float levelDuration = 75f;

        [Header("VFX/SFX")]
        [SerializeField] private ParticleSystem levelUpSparkles;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip sound1;
        [SerializeField] private AudioClip sound2;
        [SerializeField] private AudioClip levelUpSound;

        private readonly IHapticsService haptics = new DummyHapticsService();
        private readonly List<CollectableItem> targetItems = new List<CollectableItem>();

        private PrototypeGameState state = PrototypeGameState.Playing;
        private float timer;

        private void Awake()
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Application.targetFrameRate = 60;
            resultText.gameObject.SetActive(false);
        }

        private void Start()
        {
            timer = levelDuration;

            levelSystem.Setup(holeLevelConfig);
            var first = levelSystem.GetCurrentDefinition();
            movement.SetSpeed(first.HeroMoveSpeed);
            movement.SetScale(first.HeroScale);
            cameraFollow.OnLevelUpdated(first);
            levelText.SetLevel(1);

            joystick.InputChanged += HandleInput;
            triggerController.ItemCollected += HandleItemCollected;
            levelSystem.LevelUp += HandleLevelUp;
            levelSystem.ProgressChanged += (_, inSegment, toNext) => progressBar.AnimateTo(inSegment, toNext);

            foreach (var item in FindObjectsOfType<CollectableItem>())
            {
                if (item.IsTarget)
                {
                    targetItems.Add(item);
                }
            }
        }

        private void OnDestroy()
        {
            joystick.InputChanged -= HandleInput;
            triggerController.ItemCollected -= HandleItemCollected;
            levelSystem.LevelUp -= HandleLevelUp;
        }

        private void Update()
        {
            if (state != PrototypeGameState.Playing)
            {
                return;
            }

            timer -= Time.deltaTime;
            timerText.text = Mathf.CeilToInt(Mathf.Max(0f, timer)).ToString();
            if (timer <= 0f)
            {
                SetState(PrototypeGameState.Lose);
            }
        }

        private void HandleInput(Vector2 input)
        {
            if (state != PrototypeGameState.Playing)
            {
                return;
            }

            movement.SetInput(input);
            arrowIndicator.SetInput(input);
        }

        private void HandleItemCollected(CollectableItem item)
        {
            if (state != PrototypeGameState.Playing)
            {
                return;
            }

            levelSystem.AddPoints(item.Points);

            if (item.IsTarget)
            {
                TryPlay(sound2);
                targetItems.Remove(item);
                if (targetItems.Count == 0)
                {
                    SetState(PrototypeGameState.Win);
                }
            }
            else
            {
                TryPlay(sound1);
                haptics.Selection();
            }
        }

        private void HandleLevelUp(int level, HoleLevelDefinition definition)
        {
            movement.SetSpeed(definition.HeroMoveSpeed);
            StartCoroutine(CRGrow(definition.HeroScale));
            cameraFollow.OnLevelUpdated(definition);
            levelText.SetLevel(level);
            progressBar.Pulse();
            arrowIndicator.SetHoleScale(definition.HeroScale);

            if (levelUpSparkles != null)
            {
                levelUpSparkles.Play();
            }

            TryPlay(levelUpSound);
            haptics.Medium();
        }

        private IEnumerator CRGrow(float targetScale)
        {
            float duration = Mathf.Max(0.05f, growthConfig != null ? growthConfig.GrowthDuration : 0.2f);
            float start = movement.transform.localScale.x;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(start, targetScale, t / duration);
                movement.SetScale(s);
                yield return null;
            }

            movement.SetScale(targetScale);
        }

        private void SetState(PrototypeGameState newState)
        {
            if (state == newState)
            {
                return;
            }

            state = newState;
            movement.SetInput(Vector2.zero);
            arrowIndicator.SetInput(Vector2.zero);
            resultText.gameObject.SetActive(true);
            resultText.text = newState == PrototypeGameState.Win ? "WIN" : "LOSE";
        }

        private void TryPlay(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }
    }
}
