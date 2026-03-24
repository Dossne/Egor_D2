using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ClawbearGames
{
    public class ComboBarView : MonoBehaviour
    {
        [SerializeField] private Vector2 screenOffset = new Vector2(0f, 145f);
        [SerializeField] private Vector2 rootSize = new Vector2(220f, 62f);
        [SerializeField] private Sprite frameSprite;
        [SerializeField] private Sprite fillSprite;

        private Camera targetCamera;
        private RectTransform rootRect;
        private RectTransform fillRect;
        private Image fillImage;
        private Text comboText;
        private CanvasGroup canvasGroup;
        private Coroutine flashRoutine;
        private static Sprite whiteSprite;

        private void Awake()
        {
            EnsureUiBuilt();
            SetVisible(false);
        }

        private void LateUpdate()
        {
            if (rootRect == null)
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
                rootRect.gameObject.SetActive(false);
                return;
            }

            rootRect.gameObject.SetActive(true);
            rootRect.position = screenPoint + (Vector3)screenOffset;
        }

        public void Configure(ComboConfig config)
        {
            if (config == null)
            {
                return;
            }

            screenOffset = config.UiScreenOffset;
        }

        public void SetVisible(bool visible)
        {
            EnsureUiBuilt();
            rootRect.gameObject.SetActive(visible);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
            }
        }

        public void SetFill(float normalized)
        {
            EnsureUiBuilt();
            fillRect.anchorMax = new Vector2(Mathf.Clamp01(normalized), 1f);
        }

        public void SetLevel(int level)
        {
            EnsureUiBuilt();
            comboText.text = level > 0 ? $"+{level}" : string.Empty;
        }

        public void PlayFlash(float duration)
        {
            EnsureUiBuilt();
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(CRFlash(Mathf.Max(0.05f, duration)));
        }

        private IEnumerator CRFlash(float duration)
        {
            float t = 0f;
            Vector3 startScale = rootRect.localScale;
            Vector3 peakScale = startScale * 1.08f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float n = Mathf.Clamp01(t / duration);
                float pulse = Mathf.Sin(n * Mathf.PI);
                rootRect.localScale = Vector3.Lerp(startScale, peakScale, pulse);

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                }

                if (fillImage != null)
                {
                    fillImage.color = Color.Lerp(Color.white, new Color(1f, 0.95f, 0.55f), pulse);
                }

                if (comboText != null)
                {
                    comboText.color = Color.Lerp(Color.white, new Color(1f, 0.85f, 0.2f), pulse);
                }

                yield return null;
            }

            rootRect.localScale = startScale;
            if (fillImage != null)
            {
                fillImage.color = Color.white;
            }

            if (comboText != null)
            {
                comboText.color = Color.white;
            }

            flashRoutine = null;
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

            SetFill(0f);
        }

        private void ResolveSprites()
        {
            if (fillSprite == null)
            {
                fillSprite = TryLoadProgressSprite("progressbar_fill_orange");
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
