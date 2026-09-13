using UnityEngine;
using UnityEngine.UI;

public static class RetroUiFactory
{
    public static readonly Color Ink = new(0.015f, 0.018f, 0.012f, 0.92f);
    public static readonly Color Olive = new(0.48f, 0.52f, 0.04f, 1f);
    public static readonly Color Cream = new(0.96f, 0.97f, 0.72f, 1f);
    public static readonly Color Muted = new(0.72f, 0.74f, 0.55f, 1f);
    public static readonly Color Orange = new(1f, 0.46f, 0.06f, 1f);

    public static Canvas CreateCanvas(string name, int sortingOrder)
    {
        Canvas canvas = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(640f, 360f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referencePixelsPerUnit = 100f;
        return canvas;
    }

    public static Image CreateImage(string name, Transform parent, Color color, Sprite sprite = null)
    {
        Image image = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
        image.transform.SetParent(parent, false);
        image.color = color;
        image.sprite = sprite;
        image.raycastTarget = false;
        return image;
    }

    public static Text CreateText(string name, Transform parent, Font font, string content, int size, TextAnchor alignment, Color color)
    {
        Text text = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Shadow)).GetComponent<Text>();
        text.transform.SetParent(parent, false);
        text.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = content;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        Shadow shadow = text.GetComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
        shadow.effectDistance = new Vector2(1f, -1f);
        return text;
    }

    public static Button CreateButton(string name, Transform parent, Font font, string content, int size, out Text label)
    {
        Image target = CreateImage(name, parent, Color.clear);
        target.raycastTarget = true;
        Button button = target.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;

        label = CreateText("Label", target.transform, font, content, size, TextAnchor.MiddleLeft, Muted);
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        return button;
    }

    public static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
