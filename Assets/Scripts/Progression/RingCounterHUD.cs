using UnityEngine;
using UnityEngine.UI;

public sealed class RingCounterHUD : MonoBehaviour
{
    [SerializeField] private RingWallet wallet;

    private Text counter;

    private void Awake()
    {
        if (wallet == null)
        {
            wallet = FindFirstObjectByType<RingWallet>();
        }

        Canvas canvas = new GameObject("RingCanvas", typeof(Canvas), typeof(CanvasScaler)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 101;

        counter = new GameObject("RingCounter", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        counter.transform.SetParent(canvas.transform, false);
        counter.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        counter.fontSize = 22;
        counter.alignment = TextAnchor.UpperRight;
        counter.color = Color.yellow;
        RectTransform rect = counter.rectTransform;
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-24f, -24f);
        rect.sizeDelta = new Vector2(260f, 36f);
    }

    private void OnEnable()
    {
        if (wallet != null)
        {
            wallet.RingsChanged += Refresh;
        }
    }

    private void Start()
    {
        Refresh(wallet != null ? wallet.Rings : 0);
    }

    private void OnDisable()
    {
        if (wallet != null)
        {
            wallet.RingsChanged -= Refresh;
        }
    }

    private void Refresh(int rings)
    {
        if (counter != null)
        {
            counter.text = $"RINGS: {rings}";
        }
    }
}
