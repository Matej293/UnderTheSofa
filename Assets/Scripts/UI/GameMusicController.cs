using System.Collections;
using UnityEngine;

public sealed class GameMusicController : MonoBehaviour
{
    private enum MusicMode
    {
        None,
        Menu,
        WaitingForGameplay,
        Gameplay
    }

    public static GameMusicController Instance { get; private set; }

    private AudioSource source;
    private AudioClip menuTrack;
    private AudioClip[] gameplayTracks;
    private MusicMode mode;
    private Coroutine transitionRoutine;
    private int gameplayTrackIndex = -1;
    private float fadeLevel;
    private float menuFadeInDuration;
    private float gameplayFadeInDuration;
    private float gameplayTrackFadeOutDuration;
    private bool gameplayTrackTransitionStarted;
    private bool manuallyPaused;

    public static void Ensure(
        AudioClip menuMusic,
        AudioClip[] gameMusic,
        bool useMenuMusic,
        float menuFadeIn,
        float gameplayFadeIn,
        float gameplayFadeOut)
    {
        if (Instance == null)
        {
            GameObject root = new("Persistent Music");
            Instance = root.AddComponent<GameMusicController>();
            DontDestroyOnLoad(root);
        }

        Instance.Configure(menuMusic, gameMusic, useMenuMusic, menuFadeIn, gameplayFadeIn, gameplayFadeOut);
    }

    public void BeginGameplayTransition(float fadeOutDuration)
    {
        if (mode == MusicMode.WaitingForGameplay || mode == MusicMode.Gameplay)
        {
            return;
        }

        mode = MusicMode.WaitingForGameplay;
        BeginTransition(FadeOutAndStop(Mathf.Max(0.05f, fadeOutDuration)));
    }

    public void PauseGameplayMusic()
    {
        if (mode != MusicMode.Gameplay || manuallyPaused || source == null)
        {
            return;
        }

        manuallyPaused = true;
        source.Pause();
    }

    public void ResumeGameplayMusic()
    {
        if (!manuallyPaused || source == null)
        {
            return;
        }

        manuallyPaused = false;
        if (mode == MusicMode.Gameplay && source.clip != null)
        {
            source.UnPause();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.ignoreListenerPause = true;
        source.spatialBlend = 0f;
    }

    private void Update()
    {
        if (source == null)
        {
            return;
        }

        source.volume = fadeLevel * GameSettings.MusicVolume;

        if (mode != MusicMode.Gameplay || manuallyPaused || gameplayTrackTransitionStarted || source.clip == null)
        {
            return;
        }

        if (!source.isPlaying)
        {
            StartNextGameplayTrack();
            return;
        }

        float remaining = source.clip.length - source.time;
        if (source.time > 0.1f && remaining <= gameplayTrackFadeOutDuration)
        {
            gameplayTrackTransitionStarted = true;
            BeginTransition(TransitionToNextGameplayTrack(Mathf.Max(0.05f, remaining)));
        }
    }

    private void Configure(
        AudioClip newMenuTrack,
        AudioClip[] newGameplayTracks,
        bool useMenuMusic,
        float menuFadeIn,
        float gameplayFadeIn,
        float gameplayFadeOut)
    {
        menuTrack = newMenuTrack;
        gameplayTracks = newGameplayTracks;
        menuFadeInDuration = Mathf.Max(0.05f, menuFadeIn);
        gameplayFadeInDuration = Mathf.Max(0.05f, gameplayFadeIn);
        gameplayTrackFadeOutDuration = Mathf.Max(0.05f, gameplayFadeOut);
        GameSettings.Initialize();

        if (useMenuMusic)
        {
            manuallyPaused = false;
            if (mode != MusicMode.Menu || source.clip != menuTrack || !source.isPlaying)
            {
                mode = MusicMode.Menu;
                BeginTransition(SwitchToMenuMusic());
            }
        }
        else if (mode != MusicMode.Gameplay)
        {
            StartGameplayPlaylist();
        }
    }

    private IEnumerator SwitchToMenuMusic()
    {
        if (source.isPlaying)
        {
            yield return FadeTo(0f, 0.35f);
        }

        source.Stop();
        source.clip = menuTrack;
        source.loop = true;
        fadeLevel = 0f;
        if (menuTrack == null)
        {
            yield break;
        }

        source.Play();
        yield return FadeTo(1f, menuFadeInDuration);
    }

    private void StartGameplayPlaylist()
    {
        mode = MusicMode.Gameplay;
        manuallyPaused = false;
        gameplayTrackIndex = -1;
        source.Stop();
        StartNextGameplayTrack();
    }

    private void StartNextGameplayTrack()
    {
        AudioClip nextTrack = GetNextGameplayTrack();
        if (nextTrack == null)
        {
            source.Stop();
            source.clip = null;
            fadeLevel = 0f;
            return;
        }

        gameplayTrackTransitionStarted = true;
        source.Stop();
        source.clip = nextTrack;
        source.loop = false;
        fadeLevel = 0f;
        source.Play();
        BeginTransition(FadeInGameplayTrack());
    }

    private AudioClip GetNextGameplayTrack()
    {
        if (gameplayTracks == null || gameplayTracks.Length == 0)
        {
            return null;
        }

        for (int attempt = 0; attempt < gameplayTracks.Length; attempt++)
        {
            gameplayTrackIndex = (gameplayTrackIndex + 1) % gameplayTracks.Length;
            AudioClip nextTrack = gameplayTracks[gameplayTrackIndex];
            if (nextTrack != null)
            {
                return nextTrack;
            }
        }

        return null;
    }

    private IEnumerator FadeInGameplayTrack()
    {
        yield return FadeTo(1f, gameplayFadeInDuration);
        gameplayTrackTransitionStarted = false;
    }

    private IEnumerator TransitionToNextGameplayTrack(float fadeOutDuration)
    {
        yield return FadeTo(0f, fadeOutDuration);
        source.Stop();

        AudioClip nextTrack = GetNextGameplayTrack();
        if (nextTrack == null)
        {
            source.clip = null;
            gameplayTrackTransitionStarted = false;
            yield break;
        }

        source.clip = nextTrack;
        source.loop = false;
        fadeLevel = 0f;
        source.Play();
        yield return FadeTo(1f, gameplayFadeInDuration);
        gameplayTrackTransitionStarted = false;
    }

    private IEnumerator FadeOutAndStop(float duration)
    {
        yield return FadeTo(0f, duration);
        source.Stop();
        source.clip = null;
    }

    private IEnumerator FadeTo(float target, float duration)
    {
        float start = fadeLevel;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (manuallyPaused && mode == MusicMode.Gameplay)
            {
                yield return null;
                continue;
            }

            elapsed += Time.unscaledDeltaTime;
            fadeLevel = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }

        fadeLevel = target;
    }

    private void BeginTransition(IEnumerator routine)
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
        }

        transitionRoutine = StartCoroutine(routine);
    }
}
