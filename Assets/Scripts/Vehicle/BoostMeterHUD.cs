using UnityEngine;
using UnityEngine.UI;

public sealed class BoostMeterHUD : MonoBehaviour
{
    private sealed class HudParticle
    {
        public Image Image;
        public RectTransform Transform;
        public Vector2 Velocity;
        public float Age;
        public float Lifetime;
        public Color StartColor;
        public bool IsActive;
    }

    [SerializeField] private VehicleBoost boost;
    [SerializeField] private Vector2 position = new(24f, 24f);
    [SerializeField] private Vector2 size = new(220f, 22f);
    [SerializeField, Min(0f)] private float drainAnimationSpeed = 0.9f;
    [SerializeField, Min(0f)] private float rechargeAnimationSpeed = 0.45f;

    [Header("Boost Particles")]
    [SerializeField, Min(1)] private int particlePoolSize = 18;
    [SerializeField, Min(0f)] private float particleEmissionRate = 18f;
    [SerializeField] private Vector2 particleSizeRange = new(2f, 5f);
    [SerializeField] private Vector2 particleLifetimeRange = new(0.25f, 0.4f);
    [SerializeField] private Vector2 particleHorizontalSpeedRange = new(12f, 34f);
    [SerializeField] private Vector2 particleVerticalSpeedRange = new(-18f, 24f);
    [SerializeField] private Color particleColor = new(1f, 0.58f, 0.08f, 0.9f);

    private Image fill;
    private RectTransform frame;
    private RectTransform particleLayer;
    private HudParticle[] particles;
    private float displayedEnergy;
    private float particleEmissionAccumulator;
    private int nextParticleIndex;
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

        frame = CreateImage("Frame", canvas.transform, new Color(0.05f, 0.05f, 0.05f, 0.9f));
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

        CreateParticlePool();
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

        UpdateParticles(Time.unscaledDeltaTime);
        if (boost.IsBoosting && targetEnergy > 0f)
        {
            particleEmissionAccumulator += particleEmissionRate * Time.unscaledDeltaTime;
            while (particleEmissionAccumulator >= 1f)
            {
                EmitParticle();
                particleEmissionAccumulator -= 1f;
            }
        }
        else
        {
            particleEmissionAccumulator = 0f;
        }
    }

    private void CreateParticlePool()
    {
        particleLayer = new GameObject("BoostParticles", typeof(RectTransform)).GetComponent<RectTransform>();
        particleLayer.SetParent(frame, false);
        particleLayer.anchorMin = Vector2.zero;
        particleLayer.anchorMax = Vector2.one;
        particleLayer.offsetMin = Vector2.zero;
        particleLayer.offsetMax = Vector2.zero;

        particles = new HudParticle[Mathf.Max(1, particlePoolSize)];
        for (int index = 0; index < particles.Length; index++)
        {
            Image particleImage = new GameObject($"Fleck {index + 1}", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            particleImage.transform.SetParent(particleLayer, false);
            particleImage.sprite = uiSprite;
            particleImage.type = Image.Type.Simple;
            particleImage.raycastTarget = false;
            particleImage.gameObject.SetActive(false);

            RectTransform particleTransform = particleImage.rectTransform;
            particleTransform.anchorMin = Vector2.zero;
            particleTransform.anchorMax = Vector2.zero;
            particleTransform.pivot = new Vector2(0.5f, 0.5f);

            particles[index] = new HudParticle
            {
                Image = particleImage,
                Transform = particleTransform
            };
        }
    }

    private void EmitParticle()
    {
        HudParticle particle = particles[nextParticleIndex];
        nextParticleIndex = (nextParticleIndex + 1) % particles.Length;

        float particleSize = Random.Range(particleSizeRange.x, particleSizeRange.y);
        float frameWidth = frame.rect.width;
        float frameHeight = frame.rect.height;
        particle.Transform.sizeDelta = new Vector2(particleSize, particleSize);
        particle.Transform.anchoredPosition = new Vector2(
            3f + Mathf.Max(0f, frameWidth - 6f) * displayedEnergy,
            Random.Range(3f, Mathf.Max(3f, frameHeight - 3f)));
        particle.Velocity = new Vector2(
            Random.Range(particleHorizontalSpeedRange.x, particleHorizontalSpeedRange.y),
            Random.Range(particleVerticalSpeedRange.x, particleVerticalSpeedRange.y));
        particle.Age = 0f;
        particle.Lifetime = Random.Range(particleLifetimeRange.x, particleLifetimeRange.y);
        particle.StartColor = particleColor;
        particle.Image.color = particleColor;
        particle.IsActive = true;
        particle.Image.gameObject.SetActive(true);
    }

    private void UpdateParticles(float deltaTime)
    {
        if (particles == null)
        {
            return;
        }

        foreach (HudParticle particle in particles)
        {
            if (!particle.IsActive)
            {
                continue;
            }

            particle.Age += deltaTime;
            if (particle.Age >= particle.Lifetime)
            {
                particle.IsActive = false;
                particle.Image.gameObject.SetActive(false);
                continue;
            }

            particle.Transform.anchoredPosition += particle.Velocity * deltaTime;
            float remainingAlpha = 1f - particle.Age / particle.Lifetime;
            Color color = particle.StartColor;
            color.a *= remainingAlpha;
            particle.Image.color = color;
        }
    }

    private void OnDestroy()
    {
        if (uiSprite == null)
        {
            return;
        }

        Texture2D texture = uiSprite.texture;
        Destroy(uiSprite);
        Destroy(texture);
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
