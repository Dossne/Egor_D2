using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ClawbearGames
{
    public class ComboBarView : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private Vector2 screenOffset = new Vector2(0f, 145f);
        [SerializeField] private Vector2 rootSize = new Vector2(220f, 62f);

        [Header("Sprites")]
        [SerializeField] private Sprite frameSprite;
        [SerializeField] private Sprite fillSprite;

        [Header("Animation")]
        [SerializeField][Range(0.05f, 0.6f)] private float showDuration = 0.18f;
        [SerializeField][Range(0.05f, 0.6f)] private float hideDuration = 0.2f;
        [SerializeField][Range(0.02f, 0.4f)] private float fillSmoothTime = 0.12f;
        [SerializeField][Range(1f, 1.35f)] private float showScaleMultiplier = 1.1f;
        [SerializeField][Range(1f, 1.2f)] private float levelUpScaleMultiplier = 1.04f;
        [SerializeField][Range(1f, 1.45f)] private float labelShowScaleMultiplier = 1.25f;
        [SerializeField][Range(1f, 1.35f)] private float labelLevelScaleMultiplier = 1.16f;

        private Camera targetCamera;
        private RectTransform rootRect;
        private RectTransform fillRect;
        private Image fillImage;
        private Text comboText;
        private CanvasGroup canvasGroup;
        private Coroutine showHideRoutine;
        private Coroutine barPulseRoutine;
        private Coroutine labelPulseRoutine;
        private static Sprite whiteSprite;

        private readonly Color baseFillColor = new Color(1f, 0.62f, 0.12f);
        private readonly Color accentFillColor = new Color(1f, 0.8f, 0.25f);
        private readonly Color baseTextColor = Color.white;
        private readonly Color accentTextColor = new Color(1f, 0.85f, 0.2f);

        private float currentFill;
        private float targetFill;
        private float fillVelocity;
        private bool isVisible;

        private void Awake()
        {
            EnsureUiBuilt();
            SetHiddenImmediate();
        }

        private void LateUpdate()
        {
            if (rootRect == null)
            {
                return;
            }

            SmoothFill();
            UpdateWorldFollow();
        }

        public void Configure(ComboConfig config)
        {
            if (config == null)
            {
                return;
            }

            screenOffset = config.UiScreenOffset;
        }

        public void PlayShow(int level, float normalizedFill, float flashDuration)
        {
            EnsureUiBuilt();
            isVisible = true;

            SetLevel(level, false);
            SetFillImmediate(0f);
            SetFillSmoothTarget(normalizedFill);

            StartShowAnimation(Mathf.Max(0.05f, flashDuration));
        }

        public void PlayLevelUp(int level, float normalizedFill, float flashDuration)
        {
            EnsureUiBuilt();
            isVisible = true;
            EnsureShownForUpdate();

            SetLevel(level, false);
            SetFillSmoothTarget(normalizedFill);
            PlayBarPulse(Mathf.Max(0.05f, flashDuration), levelUpScaleMultiplier, false);
            PlayLabelPulse(Mathf.Max(0.05f, flashDuration), labelLevelScaleMultiplier, true);
        }

        public void ApplyIdleUpdate(int level, float normalizedFill)
        {
            EnsureUiBuilt();

            if (!isVisible)
            {
                return;
            }

            bool levelChanged = GetCurrentLabelLevel() != level;
            SetLevel(level, false);
            SetFillSmoothTarget(normalizedFill);

            if (levelChanged && level <= 0)
            {
                comboText.text = string.Empty;
            }
        }

        public void PlayHide()
        {
            EnsureUiBuilt();
            if (!isVisible && !rootRect.gameObject.activeSelf)
            {
                return;
            }

            isVisible = false;
            SetLevel(0, false);
            SetFillSmoothTarget(0f);

            if (showHideRoutine != null)
            {
                StopCoroutine(showHideRoutine);
            }

            showHideRoutine = StartCoroutine(CRHide());
        }

        public void SetFillImmediate(float normalized)
        {
            EnsureUiBuilt();
            currentFill = Mathf.Clamp01(normalized);
            targetFill = currentFill;
            fillVelocity = 0f;
            ApplyFill(currentFill);
        }

        public void SetFillSmoothTarget(float normalized)
        {
            EnsureUiBuilt();
            targetFill = Mathf.Clamp01(normalized);
        }

        private void StartShowAnimation(float flashDuration)
        {
            if (showHideRoutine != null)
            {
                StopCoroutine(showHideRoutine);
            }

            showHideRoutine = StartCoroutine(CRShow(flashDuration));
        }

        private IEnumerator CRShow(float flashDuration)
        {
            EnsureShownForUpdate();
            rootRect.localScale = Vector3.one * 0.86f;
            canvasGroup.alpha = 0f;

            float elapsed = 0f;
            while (elapsed < showDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / showDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);

                canvasGroup.alpha = Mathf.Lerp(0f, 1f, eased);
                float scale = Mathf.Lerp(0.86f, 1f, eased);
                rootRect.localScale = Vector3.one * scale;
                yield return null;
            }

            canvasGroup.alpha = 1f;
            rootRect.localScale = Vector3.one;
            PlayBarPulse(flashDuration, showScaleMultiplier, true);
            PlayLabelPulse(flashDuration, labelShowScaleMultiplier, true);
            showHideRoutine = null;
        }

        private IEnumerator CRHide()
        {
            if (barPulseRoutine != null)
            {
                StopCoroutine(barPulseRoutine);
                barPulseRoutine = null;
            }

            if (labelPulseRoutine != null)
            {
                StopCoroutine(labelPulseRoutine);
                labelPulseRoutine = null;
            }

            float startAlpha = canvasGroup.alpha;
            Vector3 startScale = rootRect.localScale;
            Vector3 endScale = Vector3.one * 0.92f;

            float elapsed = 0f;
            while (elapsed < hideDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / hideDuration);
                float eased = t * t;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, eased);
                rootRect.localScale = Vector3.Lerp(startScale, endScale, eased);
                yield return null;
            }

            SetHiddenImmediate();
            showHideRoutine = null;
        }

        private void PlayBarPulse(float duration, float scaleMultiplier, bool accentText)
        {
            if (barPulseRoutine != null)
            {
                StopCoroutine(barPulseRoutine);
            }

            barPulseRoutine = StartCoroutine(CRBarPulse(duration, scaleMultiplier, accentText));
        }

        private IEnumerator CRBarPulse(float duration, float scaleMultiplier, bool accentText)
        {
            float t = 0f;
            Vector3 startScale = rootRect.localScale;
            Vector3 peakScale = Vector3.one * Mathf.Max(1f, scaleMultiplier);

            while (t < duration)
            {
                t += Time.deltaTime;
                float n = Mathf.Clamp01(t / duration);
                float pulse = Mathf.Sin(n * Mathf.PI);
                rootRect.localScale = Vector3.Lerp(startScale, peakScale, pulse);
                fillImage.color = Color.Lerp(baseFillColor, accentFillColor, pulse);

                if (accentText)
                {
                    comboText.color = Color.Lerp(baseTextColor, accentTextColor, pulse);
                }

                yield return null;
            }

            rootRect.localScale = Vector3.one;
            fillImage.color = baseFillColor;

            if (accentText)
            {
                comboText.color = baseTextColor;
            }

            barPulseRoutine = null;
        }

        private void PlayLabelPulse(float duration, float scaleMultiplier, bool accent)
        {
            if (comboText == null || string.IsNullOrEmpty(comboText.text))
            {
                return;
            }

            if (labelPulseRoutine != null)
            {
                StopCoroutine(labelPulseRoutine);
            }

            labelPulseRoutine = StartCoroutine(CRLabelPulse(duration, scaleMultiplier, accent));
        }

        private IEnumerator CRLabelPulse(float duration, float scaleMultiplier, bool accent)
        {
            RectTransform labelRect = comboText.rectTransform;
            float t = 0f;
            Vector3 startScale = Vector3.one;
            Vector3 peakScale = Vector3.one * Mathf.Max(1f, scaleMultiplier);

            while (t < duration)
            {
                t += Time.deltaTime;
                float n = Mathf.Clamp01(t / duration);
                float pulse = Mathf.Sin(n * Mathf.PI);
                labelRect.localScale = Vector3.Lerp(startScale, peakScale, pulse);
                if (accent)
                {
                    comboText.color = Color.Lerp(baseTextColor, accentTextColor, pulse);
                }

                yield return null;
            }

            labelRect.localScale = Vector3.one;
            comboText.color = baseTextColor;
            labelPulseRoutine = null;
        }

        private void SetLevel(int level, bool playLabelPulse)
        {
            comboText.text = level > 0 ? $"+{level}" : string.Empty;
            if (playLabelPulse && level > 0)
            {
                PlayLabelPulse(showDuration, labelLevelScaleMultiplier, true);
            }
        }

        private int GetCurrentLabelLevel()
        {
            if (comboText == null || string.IsNullOrEmpty(comboText.text) || comboText.text.Length < 2)
            {
                return 0;
            }

            if (int.TryParse(comboText.text.Substring(1), out int level))
            {
                return level;
            }

            return 0;
        }

        private void SmoothFill()
        {
            if (Mathf.Approximately(currentFill, targetFill))
            {
                return;
            }

            currentFill = Mathf.SmoothDamp(currentFill, targetFill, ref fillVelocity, fillSmoothTime, Mathf.Infinity, Time.deltaTime);
            if (Mathf.Abs(currentFill - targetFill) <= 0.0005f)
            {
                currentFill = targetFill;
                fillVelocity = 0f;
            }

            ApplyFill(currentFill);
        }

        private void ApplyFill(float normalized)
        {
            if (fillRect == null)
            {
                return;
            }

            fillRect.anchorMax = new Vector2(Mathf.Clamp01(normalized), 1f);
        }

        private void UpdateWorldFollow()
        {
            if (!isVisible || rootRect == null || !rootRect.gameObject.activeSelf)
            {
                return;
            }

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera == null)
            {
                return;
            }

            Vector3 screenPoint = targetCamera.WorldToScreenPoint(transform.position);
            if (screenPoint.z <= 0f)
            {
                canvasGroup.alpha = 0f;
                return;
            }

            if (canvasGroup.alpha <= 0f && showHideRoutine == null)
            {
                canvasGroup.alpha = 1f;
            }

            rootRect.position = screenPoint + (Vector3)screenOffset;
        }

        private void EnsureShownForUpdate()
        {
            rootRect.gameObject.SetActive(true);
            if (canvasGroup.alpha <= 0f)
            {
                canvasGroup.alpha = 1f;
            }
        }

        private void SetHiddenImmediate()
        {
            isVisible = false;
            currentFill = 0f;
            targetFill = 0f;
            fillVelocity = 0f;
            ApplyFill(0f);

            if (comboText != null)
            {
                comboText.text = string.Empty;
                comboText.color = baseTextColor;
                comboText.rectTransform.localScale = Vector3.one;
            }

            if (fillImage != null)
            {
                fillImage.color = baseFillColor;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            if (rootRect != null)
            {
                rootRect.localScale = Vector3.one;
                rootRect.gameObject.SetActive(false);
            }
        }

        private void EnsureUiBuilt()
        {
            if (rootRect != null)
            {
                return;
            }

            ResolveSprites();

            Canvas canvas = FindOverlayCanvas();
            rootRect = new GameObject("ComboBar", typeof(RectTransform), typeof(CanvasGroup)).GetComponent<RectTransform>();
            rootRect.SetParent(canvas.transform, false);
            rootRect.sizeDelta = rootSize;
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            canvasGroup = rootRect.GetComponent<CanvasGroup>();

            RectTransform barRoot = new GameObject("ComboBarRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            barRoot.SetParent(rootRect, false);
            barRoot.anchorMin = new Vector2(0f, 0.5f);
            barRoot.anchorMax = new Vector2(1f, 0.5f);
            barRoot.pivot = new Vector2(0.5f, 0.5f);
            barRoot.offsetMin = new Vector2(0f, -14f);
            barRoot.offsetMax = new Vector2(-46f, 14f);

            RectTransform fillMask = new GameObject("ComboFillMask", typeof(RectTransform), typeof(Image), typeof(RectMask2D)).GetComponent<RectTransform>();
            fillMask.SetParent(barRoot, false);
            fillMask.anchorMin = Vector2.zero;
            fillMask.anchorMax = Vector2.one;
            fillMask.offsetMin = new Vector2(8f, 5f);
            fillMask.offsetMax = new Vector2(-8f, -5f);
            Image maskImage = fillMask.GetComponent<Image>();
            maskImage.sprite = GetWhiteSprite();
            maskImage.color = Color.black;
            maskImage.raycastTarget = false;

            fillRect = CreateImage("ComboFill", fillMask, Color.white, fillSprite);
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillImage = fillRect.GetComponent<Image>();
            fillImage.type = Image.Type.Simple;
            fillImage.raycastTarget = false;

            RectTransform frameRect = CreateImage("ComboFrame", barRoot, Color.white, frameSprite);
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;
            Image frameImage = frameRect.GetComponent<Image>();
            frameImage.type = Image.Type.Simple;
            frameImage.preserveAspect = true;
            frameImage.raycastTarget = false;

            comboText = CreateText("ComboText", rootRect);
            comboText.rectTransform.anchorMin = new Vector2(1f, 0.5f);
            comboText.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            comboText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            comboText.rectTransform.anchoredPosition = new Vector2(-20f, 0f);
            comboText.rectTransform.sizeDelta = new Vector2(52f, 40f);
            comboText.fontSize = 24;
            comboText.text = string.Empty;
            comboText.raycastTarget = false;

            SetFillImmediate(0f);
        }

        private void ResolveSprites()
        {
            if (fillSprite == null)
            {
                fillSprite = TryLoadProgressSprite("progressbar_fill_orange");
            }

            if (fillSprite == null)
            {
                fillSprite = TryLoadProgressSprite("progressbar_fill");
            }

            if (frameSprite == null)
            {
                frameSprite = TryLoadProgressSprite("progressbar_frame");
            }
        }

        private static Sprite TryLoadProgressSprite(string spriteName)
        {
            Sprite sprite = Resources.Load<Sprite>(spriteName);
            if (sprite != null)
            {
                return sprite;
            }

#if UNITY_EDITOR
            const string basePath = "Assets/_Blocky_Holes/Sprites/UI/Progressbar/";
            sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"{basePath}{spriteName}.png");
#endif
            return sprite;
        }

        private Canvas FindOverlayCanvas()
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            for (int i = 0; i < canvases.Length; i++)
            {
                if (canvases[i].renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    return canvases[i];
                }
            }

            Canvas canvas = new GameObject("ComboCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 210;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private static RectTransform CreateImage(string name, RectTransform parent, Color color, Sprite sprite = null)
        {
            Image image = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            image.transform.SetParent(parent, false);
            image.rectTransform.anchorMin = new Vector2(0f, 0f);
            image.rectTransform.anchorMax = new Vector2(1f, 1f);
            image.rectTransform.offsetMin = Vector2.zero;
            image.rectTransform.offsetMax = Vector2.zero;
            image.sprite = sprite != null ? sprite : GetWhiteSprite();
            image.color = color;
            return image.rectTransform;
        }

        private static Text CreateText(string name, RectTransform parent)
        {
            Text text = new GameObject(name, typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            text.transform.SetParent(parent, false);
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontStyle = FontStyle.Bold;
            return text;
        }

        private static Sprite GetWhiteSprite()
        {
            if (whiteSprite != null)
            {
                return whiteSprite;
            }

            Texture2D texture = Texture2D.whiteTexture;
            whiteSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return whiteSprite;
        }
    }
}
