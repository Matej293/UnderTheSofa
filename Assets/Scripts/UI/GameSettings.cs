using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public static class GameSettings
{
    public readonly struct ResolutionOption
    {
        public ResolutionOption(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; }
        public int Height { get; }
        public override string ToString() => $"{Width} X {Height}";
    }

    private const string MasterKey = "settings.masterVolume";
    private const string MusicKey = "settings.musicVolume";
    private const string FullscreenKey = "settings.fullscreen";
    private const string WidthKey = "settings.resolutionWidth";
    private const string HeightKey = "settings.resolutionHeight";
    private const string MusicMixerParameter = "MusicVolume";

    private static bool loaded;
    private static AudioMixer audioMixer;
    private static List<ResolutionOption> resolutions;

    public static float MasterVolume { get; private set; } = 1f;
    public static float MusicVolume { get; private set; } = 0.8f;
    public static bool Fullscreen { get; private set; } = true;
    public static int ResolutionWidth { get; private set; }
    public static int ResolutionHeight { get; private set; }
    public static IReadOnlyList<ResolutionOption> Resolutions => resolutions ??= BuildResolutionList();

    public static void Initialize(AudioMixer mixer = null)
    {
        if (mixer != null)
        {
            audioMixer = mixer;
        }

        if (!loaded)
        {
            MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterKey, 1f));
            MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicKey, 0.8f));
            Fullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) != 0;
            ResolutionWidth = PlayerPrefs.GetInt(WidthKey, Screen.width);
            ResolutionHeight = PlayerPrefs.GetInt(HeightKey, Screen.height);
            loaded = true;
        }

        ApplyAudio();
    }

    public static void SetMasterVolume(float value)
    {
        Initialize();
        MasterVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterKey, MasterVolume);
        ApplyAudio();
        PlayerPrefs.Save();
    }

    public static void SetMusicVolume(float value)
    {
        Initialize();
        MusicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicKey, MusicVolume);
        ApplyAudio();
        PlayerPrefs.Save();
    }

    public static void SetFullscreen(bool value)
    {
        Initialize();
        Fullscreen = value;
        PlayerPrefs.SetInt(FullscreenKey, Fullscreen ? 1 : 0);
        ApplyDisplay();
        PlayerPrefs.Save();
    }

    public static bool SetResolution(ResolutionOption option)
    {
        Initialize();
        try
        {
            ResolutionWidth = option.Width;
            ResolutionHeight = option.Height;
            PlayerPrefs.SetInt(WidthKey, ResolutionWidth);
            PlayerPrefs.SetInt(HeightKey, ResolutionHeight);
            ApplyDisplay();
            PlayerPrefs.Save();
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not apply resolution {option}: {exception.Message}");
            return false;
        }
    }

    public static int GetCurrentResolutionIndex()
    {
        Initialize();
        IReadOnlyList<ResolutionOption> options = Resolutions;
        for (int index = 0; index < options.Count; index++)
        {
            if (options[index].Width == ResolutionWidth && options[index].Height == ResolutionHeight)
            {
                return index;
            }
        }

        return Mathf.Max(0, options.Count - 1);
    }

    private static void ApplyAudio()
    {
        AudioListener.volume = MasterVolume;
        if (audioMixer != null)
        {
            audioMixer.SetFloat(MusicMixerParameter, LinearToDecibels(MusicVolume));
        }
    }

    private static void ApplyDisplay()
    {
        FullScreenMode mode = Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.SetResolution(Mathf.Max(320, ResolutionWidth), Mathf.Max(240, ResolutionHeight), mode);
    }

    private static float LinearToDecibels(float value)
    {
        return value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f;
    }

    private static List<ResolutionOption> BuildResolutionList()
    {
        List<ResolutionOption> result = Screen.resolutions
            .Select(value => new ResolutionOption(value.width, value.height))
            .GroupBy(value => (value.Width, value.Height))
            .Select(group => group.First())
            .OrderBy(value => value.Width)
            .ThenBy(value => value.Height)
            .ToList();

        if (!result.Any(value => value.Width == Screen.width && value.Height == Screen.height))
        {
            result.Add(new ResolutionOption(Screen.width, Screen.height));
            result = result.OrderBy(value => value.Width).ThenBy(value => value.Height).ToList();
        }

        if (result.Count == 0)
        {
            result.Add(new ResolutionOption(1280, 720));
        }

        return result;
    }
}
