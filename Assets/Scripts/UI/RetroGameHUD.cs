using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public sealed class RetroGameHUD : MonoBehaviour
{
    private readonly struct NotificationRequest
    {
        public NotificationRequest(string message, float duration, bool completion, bool playSound)
        {
            Message = message;
            Duration = duration;
            Completion = completion;
            PlaySound = playSound;
        }

        public string Message { get; }
        public float Duration { get; }
        public bool Completion { get; }
        public bool PlaySound { get; }
    }

    private sealed class BoostParticle
    {
        public Image Image;
        public RectTransform Rect;
        public Vector2 Velocity;
        public float Age;
        public float Lifetime;
    }

    public static RetroGameHUD Instance { get; private set; }

    [Header("Data")]
    [SerializeField] private VehicleBoost boost;
    [SerializeField] private RingWallet wallet;
    [SerializeField] private RetroMenuSystem menuSystem;

    [Header("Skin 06")]
    [SerializeField] private Font regularFont;
    [SerializeField] private Font boldFont;
    [SerializeField] private Sprite brushSprite;

    [Header("Notification Audio")]
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioClip notificationSound;

    [Header("Boost")]
    [SerializeField, Min(0f)] private float drainAnimationSpeed = 0.9f;
    [SerializeField, Min(0f)] private float rechargeAnimationSpeed = 0.45f;
    [SerializeField, Min(1)] private int particlePoolSize = 18;
    [SerializeField, Min(0f)] private float particleEmissionRate = 18f;

    private readonly Queue<NotificationRequest> notifications = new();
    private readonly List<BoostParticle> particles = new();
    private Canvas canvas;
    private Image boostFill;
    private RectTransform boostFrame;
    private RectTransform particleLayer;
    private Text ringText;
    private RectTransform ringPanel;
    private GameObject notificationPanel;
    private CanvasGroup notificationGroup;
    private Text notificationText;
    private AudioSource audioSource;
    private float displayedEnergy;
    private float emissionAccumulator;
    private int nextParticle;
    private float ringPulse;
    private NotificationRequest currentNotification;
    private float notificationElapsed;
    private bool showingNotification;

    private void Awake()
    {
        Instance = this;
        boost ??= FindAnyObjectByType<VehicleBoost>();
        wallet ??= FindAnyObjectByType<RingWallet>();
        menuSystem ??= FindAnyObjectByType<RetroMenuSystem>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.ignoreListenerPause = true;
        audioSource.outputAudioMixerGroup = sfxGroup;
        BuildHud();
        displayedEnergy = boost != null ? boost.NormalizedEnergy : 0f;
    }

    private void OnEnable()
    {
        if (wallet != null) wallet.RingsChanged += OnRingsChanged;
    }

    private void Start()
    {
        OnRingsChanged(wallet != null ? wallet.Rings : 0);
    }

    private void OnDisable()
    {
        if (wallet != null) wallet.RingsChanged -= OnRingsChanged;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        UpdateBoost(deltaTime);
        UpdateRingPulse(deltaTime);
        UpdateNotification(deltaTime);
    }

    public void ShowNotification(string message, float duration, bool completion = false, bool playSound = true)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        notifications.Enqueue(new NotificationRequest(message, Mathf.Max(0.75f, duration), completion, playSound));
        if (!showingNotification) BeginNextNotification();
    }

    private void BuildHud()
    {
        canvas = RetroUiFactory.CreateCanvas("Retro Game HUD Canvas", 120);
        canvas.transform.SetParent(transform, false);
        BuildBoost();
        BuildRingCounter();
        BuildNotification();
    }

    private void BuildBoost()
    {
        Image back = RetroUiFactory.CreateImage("Boost Back", canvas.transform, new Color(0.01f, 0.01f, 0.01f, 0.88f));
        boostFrame = back.rectTransform;
        boostFrame.anchorMin = boostFrame.anchorMax = Vector2.zero;
        boostFrame.pivot = Vector2.zero;
        boostFrame.anchoredPosition = new Vector2(18f, 18f);
        boostFrame.sizeDelta = new Vector2(190f, 16f);

        Image inner = RetroUiFactory.CreateImage("Boost Track", boostFrame, new Color(0.18f, 0.12f, 0.04f, 1f));
        RectTransform innerRect = inner.rectTransform;
        innerRect.anchorMin = Vector2.zero;
        innerRect.anchorMax = Vector2.one;
        innerRect.offsetMin = new Vector2(2f, 2f);
        innerRect.offsetMax = new Vector2(-2f, -2f);

        boostFill = RetroUiFactory.CreateImage("Boost Fill", inner.transform, RetroUiFactory.Orange);
        RectTransform fillRect = boostFill.rectTransform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Text label = RetroUiFactory.CreateText("Boost Label", boostFrame, boldFont, "BOOST", 11, TextAnchor.LowerLeft, RetroUiFactory.Cream);
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.pivot = Vector2.zero;
        labelRect.anchoredPosition = new Vector2(0f, 3f);
        labelRect.sizeDelta = new Vector2(0f, 18f);

        particleLayer = new GameObject("Orange Boost Particles", typeof(RectTransform)).GetComponent<RectTransform>();
        particleLayer.SetParent(boostFrame, false);
        RetroUiFactory.Stretch(particleLayer);
        for (int index = 0; index < particlePoolSize; index++)
        {
            Image image = RetroUiFactory.CreateImage($"Particle {index + 1}", particleLayer, RetroUiFactory.Orange);
            image.gameObject.SetActive(false);
            particles.Add(new BoostParticle { Image = image, Rect = image.rectTransform });
        }
    }

    private void BuildRingCounter()
    {
        Image panel = RetroUiFactory.CreateImage("Ring Counter Back", canvas.transform, new Color(0.01f, 0.01f, 0.01f, 0.78f));
        ringPanel = panel.rectTransform;
        ringPanel.anchorMin = ringPanel.anchorMax = Vector2.one;
        ringPanel.pivot = Vector2.one;
        ringPanel.anchoredPosition = new Vector2(-18f, -18f);
        ringPanel.sizeDelta = new Vector2(188f, 28f);

        Image accent = RetroUiFactory.CreateImage("Ring Accent", ringPanel, RetroUiFactory.Olive);
        RectTransform accentRect = accent.rectTransform;
        accentRect.anchorMin = new Vector2(0f, 0f);
        accentRect.anchorMax = new Vector2(1f, 0f);
        accentRect.pivot = new Vector2(0.5f, 0f);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(0f, 2f);

        ringText = RetroUiFactory.CreateText("Ring Count", ringPanel, boldFont, "RINGS >> 00", 14, TextAnchor.MiddleRight, RetroUiFactory.Cream);
        RetroUiFactory.Stretch(ringText.rectTransform);
        ringText.rectTransform.offsetMin = new Vector2(8f, 2f);
        ringText.rectTransform.offsetMax = new Vector2(-8f, 0f);
    }

    private void BuildNotification()
    {
        Image panel = RetroUiFactory.CreateImage("Notification", canvas.transform, Color.white, brushSprite);
        notificationPanel = panel.gameObject;
        RectTransform rect = panel.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -52f);
        rect.sizeDelta = new Vector2(360f, 76f);
        notificationGroup = panel.gameObject.AddComponent<CanvasGroup>();

        notificationText = RetroUiFactory.CreateText("Message", panel.transform, boldFont, string.Empty, 14, TextAnchor.MiddleCenter, RetroUiFactory.Cream);
        RetroUiFactory.Stretch(notificationText.rectTransform);
        notificationText.rectTransform.offsetMin = new Vector2(22f, 10f);
        notificationText.rectTransform.offsetMax = new Vector2(-22f, -10f);
        notificationPanel.SetActive(false);
    }

    private void UpdateBoost(float deltaTime)
    {
        if (boost == null || boostFill == null) return;
        float target = boost.NormalizedEnergy;
        float speed = target < displayedEnergy ? drainAnimationSpeed : rechargeAnimationSpeed;
        displayedEnergy = Mathf.MoveTowards(displayedEnergy, target, speed * deltaTime);
        RectTransform fillRect = boostFill.rectTransform;
        fillRect.anchorMax = new Vector2(displayedEnergy, 1f);
        boostFill.color = boost.IsBoosting ? new Color(1f, 0.67f, 0.1f, 1f) : RetroUiFactory.Orange;

        UpdateParticles(deltaTime);
        if (boost.IsBoosting && target > 0f)
        {
            emissionAccumulator += particleEmissionRate * deltaTime;
            while (emissionAccumulator >= 1f)
            {
                EmitParticle();
                emissionAccumulator -= 1f;
            }
        }
        else emissionAccumulator = 0f;
    }

    private void EmitParticle()
    {
        if (particles.Count == 0) return;
        BoostParticle particle = particles[nextParticle];
        nextParticle = (nextParticle + 1) % particles.Count;
        float size = Random.Range(2f, 4.5f);
        particle.Rect.anchorMin = particle.Rect.anchorMax = Vector2.zero;
        particle.Rect.pivot = new Vector2(0.5f, 0.5f);
        particle.Rect.sizeDelta = new Vector2(size, size);
        particle.Rect.anchoredPosition = new Vector2(2f + 186f * displayedEnergy, Random.Range(2f, 14f));
        particle.Velocity = new Vector2(Random.Range(12f, 32f), Random.Range(-16f, 22f));
        particle.Age = 0f;
        particle.Lifetime = Random.Range(0.22f, 0.4f);
        particle.Image.color = RetroUiFactory.Orange;
        particle.Image.gameObject.SetActive(true);
    }

    private void UpdateParticles(float deltaTime)
    {
        foreach (BoostParticle particle in particles)
        {
            if (!particle.Image.gameObject.activeSelf) continue;
            particle.Age += deltaTime;
            if (particle.Age >= particle.Lifetime)
            {
                particle.Image.gameObject.SetActive(false);
                continue;
            }
            particle.Rect.anchoredPosition += particle.Velocity * deltaTime;
            Color color = RetroUiFactory.Orange;
            color.a = 1f - particle.Age / particle.Lifetime;
            particle.Image.color = color;
        }
    }

    private void OnRingsChanged(int count)
    {
        if (ringText != null) ringText.text = $"RINGS >> {count:00}";
        ringPulse = 0.22f;
    }

    private void UpdateRingPulse(float deltaTime)
    {
        if (ringPanel == null) return;
        ringPulse = Mathf.Max(0f, ringPulse - deltaTime);
        float amount = ringPulse <= 0f ? 0f : Mathf.Sin(ringPulse / 0.22f * Mathf.PI);
        ringPanel.localScale = Vector3.one * (1f + amount * 0.08f);
        ringText.color = Color.Lerp(RetroUiFactory.Cream, new Color(1f, 0.83f, 0.18f), amount);
    }

    private void BeginNextNotification()
    {
        if (notifications.Count == 0)
        {
            showingNotification = false;
            notificationPanel.SetActive(false);
            return;
        }

        currentNotification = notifications.Dequeue();
        notificationElapsed = 0f;
        showingNotification = true;
        notificationText.text = currentNotification.Message;
        notificationPanel.SetActive(true);
        if (currentNotification.PlaySound && notificationSound != null) audioSource.PlayOneShot(notificationSound);
    }

    private void UpdateNotification(float deltaTime)
    {
        if (!showingNotification) return;
        notificationElapsed += deltaTime;
        float fadeIn = Mathf.Clamp01(notificationElapsed / 0.15f);
        float fadeOut = Mathf.Clamp01((currentNotification.Duration - notificationElapsed) / 0.25f);
        float alpha = Mathf.Min(fadeIn, fadeOut);
        notificationGroup.alpha = alpha;
        notificationPanel.transform.localScale = Vector3.one * Mathf.Lerp(0.9f, 1f, fadeIn);

        if (notificationElapsed < currentNotification.Duration) return;
        bool completion = currentNotification.Completion;
        string message = currentNotification.Message;
        showingNotification = false;
        notificationPanel.SetActive(false);
        if (completion && menuSystem != null) menuSystem.ShowCompletion(message);
        BeginNextNotification();
    }
}
