using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ClawbearGames
{
    public class HoleLevelProgressUI : MonoBehaviour
    {
        private static HoleLevelProgressUI activeInstance;

        [SerializeField] private Vector3 worldOffset = Vector3.zero;
        [SerializeField] private Vector2 screenOffset = Vector2.zero;
        [SerializeField][Min(0f)] private float holeRadiusOffsetMultiplier = 1.1f;
        [SerializeField][Min(0f)] private float edgePadding = 8f;
        [SerializeField][Min(0f)] private float holeGap = 16f;
        [SerializeField][Min(0f)] private float rootTopClearance = 54f;
        [SerializeField] private Vector2 rootSize = new Vector2(420f, 122f);
        [SerializeField] private Sprite progressFillSprite = null;
        [SerializeField] private Sprite progressFrameSprite = null;
        [SerializeField][Range(0.03f, 0.5f)] private float levelFlashDuration = 0.18f;
        [SerializeField][Range(0f, 1f)] private float flashMaxAlpha = 0.75f;
        
        private Camera targetCamera;
        private RectTransform rootRect;
        private Text levelText;
        private Image fillImage;
        private RectTransform fillRect;
        private Image frameImage;
        private Image flashImage;
        private Coroutine flashRoutine;
        private PlayerController playerController;
        private float currentFill;
        private int currentLevel;
        private static Sprite whiteSprite;

        private void Awake()
        {
            if (activeInstance != null && activeInstance != this)
            {
                Destroy(this);
                return;
            }

            activeInstance = this;
            EnsureUiBuilt();
        }

        private void OnDestroy()
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
            }

            if (rootRect != null)
            {
                Destroy(rootRect.gameObject);
                rootRect = null;
            }

            if (activeInstance == this)
            {
                activeInstance = null;
            }
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

            Vector3 screenPoint = targetCamera.WorldToScreenPoint(transform.position + worldOffset);
            if (screenPoint.z <= 0f)
            {
                rootRect.gameObject.SetActive(false);
                return;
            }

            rootRect.gameObject.SetActive(true);
            float dynamicYOffset = (CalculateHoleScreenRadius(targetCamera) * holeRadiusOffsetMultiplier) + edgePadding + holeGap + rootTopClearance;
            Vector3 finalOffset = (Vector3)screenOffset + Vector3.down * dynamicYOffset;
            rootRect.position = screenPoint + finalOffset;
        }

        public void SetProgress(int level, float normalized)
        {
            EnsureUiBuilt();
            if (levelText != null)
            {
                levelText.text = $"Hole Level {level}";
            }

            SetFillAmount(normalized);

            bool fillIncreased = normalized > (currentFill + 0.0001f);
            bool levelIncreased = level > currentLevel;
            if (fillIncreased || levelIncreased)
            {
                PlayFlash();
            }

            currentFill = Mathf.Clamp01(normalized);
            currentLevel = level;
        }

        public void ConfigureSprites(Sprite frame, Sprite fill)
        {
            if (frame != null)
            {
                progressFrameSprite = frame;
            }

            if (fill != null)
            {
                progressFillSprite = fill;
            }

            if (frameImage != null && progressFrameSprite != null)
            {
                frameImage.sprite = progressFrameSprite;
                frameImage.type = Image.Type.Simple;
                frameImage.preserveAspect = true;
            }

            if (fillImage != null && progressFillSprite != null)
            {
                fillImage.sprite = progressFillSprite;
                fillImage.type = Image.Type.Simple;
                fillImage.preserveAspect = false;
            }
        }

        private void EnsureUiBuilt()
        {
            if (rootRect != null)
            {
                return;
            }

            Canvas canvas = FindOverlayCanvas();
            rootRect = new GameObject("HoleLevelProgress", typeof(RectTransform)).GetComponent<RectTransform>();
            rootRect.SetParent(canvas.transform, false);
            rootRect.sizeDelta = rootSize;
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);

            ResolveProgressSprites();

            RectTransform barRoot = new GameObject("ProgressBarRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            barRoot.SetParent(rootRect, false);
            barRoot.anchorMin = new Vector2(0.5f, 0.5f);
            barRoot.anchorMax = new Vector2(0.5f, 0.5f);
            barRoot.pivot = new Vector2(0.5f, 0.5f);
            barRoot.sizeDelta = new Vector2(rootSize.x, 58f);
            barRoot.anchoredPosition = new Vector2(0f, 24f);

            RectTransform fillMask = new GameObject("ProgressFillMask", typeof(RectTransform), typeof(Image), typeof(RectMask2D)).GetComponent<RectTransform>();
            fillMask.SetParent(barRoot, false);
            fillMask.anchorMin = Vector2.zero;
            fillMask.anchorMax = Vector2.one;
            fillMask.offsetMin = new Vector2(8f, 5f);
            fillMask.offsetMax = new Vector2(-8f, -5f);
            Image fillMaskImage = fillMask.GetComponent<Image>();
            fillMaskImage.sprite = GetWhiteSprite();
            fillMaskImage.color = Color.black;
            fillMaskImage.raycastTarget = false;

            fillRect = CreateImage("ProgressFill", fillMask, Color.white, progressFillSprite);
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillImage = fillRect.GetComponent<Image>();
            fillImage.type = Image.Type.Simple;
            fillImage.raycastTarget = false;

            RectTransform frameRect = CreateImage("ProgressFrame", barRoot, Color.white, progressFrameSprite);
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;
            frameImage = frameRect.GetComponent<Image>();
            frameImage.type = Image.Type.Simple;
            frameImage.raycastTarget = false;
            
            RectTransform flashRect = CreateImage("ProgressFlash", barRoot, new Color(1f, 1f, 1f, 0f));
            flashRect.anchorMin = Vector2.zero;
            flashRect.anchorMax = Vector2.one;
            flashRect.offsetMin = Vector2.zero;
            flashRect.offsetMax = Vector2.zero;
            flashImage = flashRect.GetComponent<Image>();
            flashImage.raycastTarget = false;
            flashImage.gameObject.SetActive(false);

            levelText = CreateText("LevelText", rootRect);
            levelText.rectTransform.anchoredPosition = new Vector2(0f, -38f);
            levelText.rectTransform.sizeDelta = new Vector2(0f, 42f);
            levelText.fontSize = 36;
            levelText.text = "Hole Level 1";
            levelText.raycastTarget = false;

            ConfigureSprites(progressFrameSprite, progressFillSprite);
            SetFillAmount(0f);
        }

        private void ResolveProgressSprites()
        {
            if (progressFillSprite == null)
            {
                progressFillSprite = TryLoadProgressSprite("progressbar_fill");
            }

            if (progressFrameSprite == null)
            {
                progressFrameSprite = TryLoadProgressSprite("progressbar_frame");
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

            Canvas canvas = new GameObject("HoleProgressCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

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
            text.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            text.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            text.rectTransform.sizeDelta = new Vector2(0f, 30f);
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


        private float CalculateHoleScreenRadius(Camera cameraRef)
        {
            if (cameraRef == null)
            {
                return 0f;
            }

            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }

            float worldRadius = playerController != null
                ? playerController.GetHoleWorldRadius()
                : Mathf.Max(transform.lossyScale.x, transform.lossyScale.z) * 0.5f;

            if (worldRadius <= 0f)
            {
                return 0f;
            }

            Vector3 centerScreen = cameraRef.WorldToScreenPoint(transform.position + worldOffset);
            Vector3 edgeWorld = transform.position + worldOffset + cameraRef.transform.right * worldRadius;
            Vector3 edgeScreen = cameraRef.WorldToScreenPoint(edgeWorld);
            return Mathf.Abs(edgeScreen.x - centerScreen.x);
        }

        private void SetFillAmount(float normalized)
        {
            if (fillRect == null)
            {
                return;
            }

            float clamped = Mathf.Clamp01(normalized);
            fillRect.anchorMax = new Vector2(clamped, 1f);
        }

        private void PlayFlash()
        {
            if (flashImage == null)
            {
                return;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(CRFlash());
        }

        private IEnumerator CRFlash()
        {
            flashImage.gameObject.SetActive(true);
            Color color = flashImage.color;
            float t = 0f;

            while (t < levelFlashDuration)
            {
                t += Time.deltaTime;
                float normalized = Mathf.Clamp01(t / levelFlashDuration);
                color.a = Mathf.Lerp(flashMaxAlpha, 0f, normalized);
                flashImage.color = color;
                yield return null;
            }

            color.a = 0f;
            flashImage.color = color;
            flashImage.gameObject.SetActive(false);
            flashRoutine = null;
        }
    }
}
