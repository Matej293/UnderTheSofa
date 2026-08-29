using UnityEngine;
using UnityEngine.UI;

public sealed class BoostMeterHUD : MonoBehaviour
{
    [SerializeField] private VehicleBoost boost;
    [SerializeField] private Vector2 position = new(24f, 24f);
    [SerializeField] private Vector2 size = new(220f, 22f);
    [SerializeField, Min(0f)] private float drainAnimationSpeed = 0.9f;
    [SerializeField, Min(0f)] private float rechargeAnimationSpeed = 0.45f;

    private Image fill;
    private float displayedEnergy;
    private Sprite uiSprite;

    private void Awake()
    {
        Canvas canvas = new GameObject("BoostCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;
        Texture2D texture = new(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        uiSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));

        RectTransform frame = CreateImage("Frame", canvas.transform, new Color(0.05f, 0.05f, 0.05f, 0.9f));
        frame.anchorMin = Vector2.zero;
        frame.anchorMax = Vector2.zero;
        frame.pivot = Vector2.zero;
        frame.anchoredPosition = position;
        frame.sizeDelta = size;

        RectTransform fillTransform = CreateImage("Fill", frame, new Color(0.25f, 0.75f, 1f));
        fill = fillTransform.GetComponent<Image>();
        fillTransform.anchorMin = new Vector2(0f, 0f);
        fillTransform.anchorMax = new Vector2(1f, 1f);
        fillTransform.offsetMin = new Vector2(3f, 3f);
        fillTransform.offsetMax = new Vector2(-3f, -3f);

        Text label = new GameObject("Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        label.transform.SetParent(frame, false);
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = "BOOST";
        label.alignment = TextAnchor.MiddleLeft;
        label.color = Color.white;
        RectTransform labelTransform = label.rectTransform;
        labelTransform.anchorMin = new Vector2(0f, 1f);
        labelTransform.anchorMax = new Vector2(1f, 1f);
        labelTransform.pivot = new Vector2(0f, 0f);
        labelTransform.anchoredPosition = new Vector2(0f, 4f);
        labelTransform.sizeDelta = new Vector2(0f, 20f);

        displayedEnergy = boost != null ? boost.NormalizedEnergy : 0f;
    }

    private void Update()
    {
        if (boost == null || fill == null)
        {
            return;
        }

        float targetEnergy = boost.NormalizedEnergy;
        float animationSpeed = targetEnergy < displayedEnergy ? drainAnimationSpeed : rechargeAnimationSpeed;
        displayedEnergy = Mathf.MoveTowards(displayedEnergy, targetEnergy, animationSpeed * Time.unscaledDeltaTime);
        fill.fillAmount = displayedEnergy;
        fill.color = boost.IsBoosting ? new Color(1f, 0.7f, 0.15f) : new Color(0.25f, 0.75f, 1f);
    }

    private RectTransform CreateImage(string name, Transform parent, Color color)
    {
        Image image = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        image.transform.SetParent(parent, false);
        image.sprite = uiSprite;
        image.color = color;
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillOrigin = 0;
        return image.rectTransform;
    }
}
