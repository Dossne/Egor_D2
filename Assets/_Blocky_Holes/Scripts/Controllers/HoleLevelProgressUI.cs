using UnityEngine;
using UnityEngine.UI;

namespace ClawbearGames
{
    public class HoleLevelProgressUI : MonoBehaviour
    {
        [SerializeField] private Vector3 worldOffset = Vector3.zero;
        [SerializeField] private Vector2 screenOffset = new Vector2(0f, -90f);
        [SerializeField] private Vector2 rootSize = new Vector2(220f, 50f);
        [SerializeField] private string roundedProgressSpriteName = "Slider";

        private Camera targetCamera;
        private RectTransform rootRect;
        private Text levelText;
        private Image fillImage;
        private static Sprite roundedProgressSprite;
        private static Sprite whiteSprite;

        private void Awake()
        {
            EnsureUiBuilt();
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
            rootRect.position = screenPoint + (Vector3)screenOffset;
        }

        public void SetProgress(int level, float normalized)
        {
            EnsureUiBuilt();
            if (levelText != null)
            {
                levelText.text = $"Масштаб {level}";
            }

            if (fillImage != null)
            {
                fillImage.fillAmount = Mathf.Clamp01(normalized);
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

            Sprite progressSprite = GetRoundedProgressSprite(roundedProgressSpriteName);
            RectTransform bg = CreateImage("ProgressBg", rootRect, new Color(0.07f, 0.07f, 0.07f, 0.9f), progressSprite);
            bg.sizeDelta = new Vector2(rootSize.x, 18f);
            bg.anchoredPosition = new Vector2(0f, -13f);
            Image backgroundImage = bg.GetComponent<Image>();
            backgroundImage.type = progressSprite != null ? Image.Type.Sliced : Image.Type.Simple;

            RectTransform fill = CreateImage("ProgressFill", bg, new Color(0.26f, 0.95f, 0.22f, 1f), progressSprite);
            fillImage = fill.GetComponent<Image>();
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillAmount = 0f;

            levelText = CreateText("LevelText", rootRect);
            levelText.rectTransform.anchoredPosition = new Vector2(0f, 13f);
            levelText.fontSize = 24;
            levelText.text = "Масштаб 1";
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

        private static Sprite GetRoundedProgressSprite(string spriteName)
        {
            if (roundedProgressSprite != null)
            {
                return roundedProgressSprite;
            }

            if (string.IsNullOrEmpty(spriteName))
            {
                return null;
            }

            Sprite[] loadedSprites = Resources.FindObjectsOfTypeAll<Sprite>();
            for (int i = 0; i < loadedSprites.Length; i++)
            {
                if (loadedSprites[i] != null && loadedSprites[i].name == spriteName)
                {
                    roundedProgressSprite = loadedSprites[i];
                    break;
                }
            }

            return roundedProgressSprite;
        }
    }
}
