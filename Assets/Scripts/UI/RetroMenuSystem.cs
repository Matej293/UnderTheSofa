using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class RetroMenuSystem : MonoBehaviour
{
    private enum Page
    {
        Main,
        Pause,
        Options,
        Controls,
        Credits,
        QuitConfirm,
        Completion
    }

    private sealed class Entry
    {
        public Button Button;
        public Text Label;
        public Func<string> Display;
        public Action Activate;
        public Action<int> Adjust;
    }

    [Header("Mode")]
    [SerializeField] private bool startMenu;
    [SerializeField] private string gameplaySceneName = "Vehicle";
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField, Min(0.1f)] private float newGameZoomDuration = 1.35f;
    [SerializeField, Min(0.1f)] private float newGameFadeDuration = 0.45f;
    [SerializeField, Min(0.1f)] private float newGameGameplayFadeInDuration = 1.25f;

    [Header("Skin 06")]
    [SerializeField] private Font regularFont;
    [SerializeField] private Font boldFont;
    [SerializeField] private Sprite brushSprite;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioClip selectSound;
    [SerializeField] private AudioClip okSound;
    [SerializeField] private AudioClip cancelSound;
    [SerializeField] private AudioClip backSound;
    [SerializeField] private AudioClip menuSound;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private AudioClip notificationSound;

    [Header("Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip[] gameplayMusic;
    [SerializeField, Min(0.05f)] private float menuMusicFadeInDuration = 0.6f;
    [SerializeField, Min(0.05f)] private float menuMusicFadeOutDuration = 0.3f;
    [SerializeField, Min(0.05f)] private float gameplayMusicFadeInDuration = 1.25f;
    [SerializeField, Min(0.05f)] private float gameplayTrackFadeOutDuration = 0.75f;

    private readonly List<Entry> entries = new();
    private readonly Dictionary<Page, int> selectedIndices = new();
    private Canvas canvas;
    private RectTransform content;
    private AudioSource audioSource;
    private Page page;
    private Page returnPage;
    private int selectedIndex;
    private float previousTimeScale = 1f;
    private VehicleInput vehicleInput;
    private bool vehicleInputWasEnabled;
    private string completionMessage;
    private bool transitionInProgress;

    public bool IsMenuOpen => canvas != null && canvas.gameObject.activeSelf;

    private void Awake()
    {
        GameSettings.Initialize(audioMixer);
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.ignoreListenerPause = true;
        audioSource.outputAudioMixerGroup = sfxGroup;
        GameMusicController.Ensure(
            menuMusic,
            gameplayMusic,
            startMenu,
            menuMusicFadeInDuration,
            gameplayMusicFadeInDuration,
            gameplayTrackFadeOutDuration);

        CreateCanvas();
        if (startMenu)
        {
            ShowPage(Page.Main);
        }
        else
        {
            vehicleInput = FindAnyObjectByType<VehicleInput>();
            canvas.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (transitionInProgress)
        {
            return;
        }

        if (!IsMenuOpen)
        {
            if (!startMenu && keyboard.escapeKey.wasPressedThisFrame)
            {
                OpenPause();
            }
            return;
        }

        if (keyboard.upArrowKey.wasPressedThisFrame)
        {
            MoveSelection(-1);
        }
        else if (keyboard.downArrowKey.wasPressedThisFrame)
        {
            MoveSelection(1);
        }
        else if (keyboard.leftArrowKey.wasPressedThisFrame)
        {
            AdjustSelection(-1);
        }
        else if (keyboard.rightArrowKey.wasPressedThisFrame)
        {
            AdjustSelection(1);
        }
        else if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
        {
            ActivateSelection();
        }
        else if (keyboard.escapeKey.wasPressedThisFrame)
        {
            HandleBack();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            PauseForFocusLoss();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            PauseForFocusLoss();
        }
    }

    private void PauseForFocusLoss()
    {
        if (!startMenu && !transitionInProgress && !IsMenuOpen)
        {
            OpenPause();
        }
    }

    public void ShowCompletion(string message)
    {
        completionMessage = string.IsNullOrWhiteSpace(message) ? "GAME COMPLETE" : message;
        SuspendGameplay();
        canvas.gameObject.SetActive(true);
        ShowPage(Page.Completion);
        Play(notificationSound);
    }

    private void OpenPause()
    {
        SuspendGameplay();
        canvas.gameObject.SetActive(true);
        ShowPage(Page.Pause);
        Play(menuSound);
    }

    private void SuspendGameplay()
    {
        previousTimeScale = Time.timeScale <= 0f ? 1f : Time.timeScale;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        GameMusicController.Instance?.PauseGameplayMusic();
        if (vehicleInput != null)
        {
            vehicleInputWasEnabled = vehicleInput.enabled;
            vehicleInput.enabled = false;
        }
    }

    private void ResumeGameplay()
    {
        canvas.gameObject.SetActive(false);
        Time.timeScale = previousTimeScale;
        AudioListener.pause = false;
        GameMusicController.Instance?.ResumeGameplayMusic();
        if (vehicleInput != null && vehicleInputWasEnabled)
        {
            vehicleInput.enabled = true;
        }
        Play(backSound);
    }

    private void CreateCanvas()
    {
        canvas = RetroUiFactory.CreateCanvas(startMenu ? "Main Menu Canvas" : "Pause Menu Canvas", 220);
        canvas.transform.SetParent(transform, false);
        content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
        content.SetParent(canvas.transform, false);
        RetroUiFactory.Stretch(content);
    }

    private void ShowPage(Page nextPage, Page backDestination = Page.Main)
    {
        if (entries.Count > 0)
        {
            selectedIndices[page] = selectedIndex;
        }

        page = nextPage;
        returnPage = backDestination;
        selectedIndex = selectedIndices.TryGetValue(nextPage, out int previousIndex) ? previousIndex : 0;
        RebuildPage();
    }

    private void RebuildPage()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        entries.Clear();

        Image shade = RetroUiFactory.CreateImage("Shade", content, new Color(0f, 0f, 0f, startMenu ? 0.58f : 0.74f));
        RetroUiFactory.Stretch(shade.rectTransform);

        bool leftAligned = page == Page.Main && startMenu;
        bool plainDarkPage = leftAligned || page == Page.Pause || page == Page.Options || page == Page.Controls || page == Page.Credits;
        Image brush = RetroUiFactory.CreateImage("Skin06 Brush", content, new Color(0.7f, 0.74f, 0.08f, 0.72f), brushSprite);
        RectTransform brushRect = brush.rectTransform;
        if (page == Page.Completion)
        {
            RetroUiFactory.Stretch(brushRect);
            brush.color = new Color(0.7f, 0.74f, 0.08f, 0.58f);
        }
        else
        {
            brushRect.anchorMin = brushRect.anchorMax = leftAligned ? new Vector2(0f, 0.5f) : new Vector2(0.5f, 0.5f);
            brushRect.pivot = leftAligned ? new Vector2(0f, 0.5f) : new Vector2(0.5f, 0.5f);
            brushRect.anchoredPosition = leftAligned ? new Vector2(0f, -30f) : Vector2.zero;
            brushRect.sizeDelta = leftAligned ? new Vector2(390f, 255f) : new Vector2(430f, 250f);
        }

        if (plainDarkPage)
        {
            brush.gameObject.SetActive(false);
            Image menuPanel = RetroUiFactory.CreateImage("Main Menu Panel", content, new Color(0f, 0f, 0f, 0.55f));
            RectTransform menuPanelRect = menuPanel.rectTransform;
            menuPanelRect.anchorMin = menuPanelRect.anchorMax = leftAligned ? new Vector2(0f, 0.5f) : new Vector2(0.5f, 0.5f);
            menuPanelRect.pivot = leftAligned ? new Vector2(0f, 0.5f) : new Vector2(0.5f, 0.5f);
            menuPanelRect.anchoredPosition = leftAligned ? new Vector2(0f, -30f) : new Vector2(0f, -22f);
            menuPanelRect.sizeDelta = leftAligned ? new Vector2(390f, 255f) : new Vector2(430f, 250f);
        }

        RectTransform list = new GameObject("Menu Entries", typeof(RectTransform)).GetComponent<RectTransform>();
        list.SetParent(content, false);
        list.anchorMin = list.anchorMax = leftAligned ? new Vector2(0f, 0.5f) : new Vector2(0.5f, 0.5f);
        list.pivot = leftAligned ? new Vector2(0f, 0.5f) : new Vector2(0.5f, 0.5f);
        list.anchoredPosition = leftAligned ? new Vector2(34f, -18f) : new Vector2(0f, -12f);
        list.sizeDelta = new Vector2(leftAligned ? 350f : 390f, 220f);

        string title = GetPageTitle();
        Text titleText = RetroUiFactory.CreateText("Title", content, boldFont, title, leftAligned ? 27 : 21,
            leftAligned ? TextAnchor.UpperLeft : TextAnchor.UpperCenter, RetroUiFactory.Cream);
        RectTransform titleRect = titleText.rectTransform;
        titleRect.anchorMin = titleRect.anchorMax = leftAligned ? new Vector2(0f, 1f) : new Vector2(0.5f, 1f);
        titleRect.pivot = leftAligned ? new Vector2(0f, 1f) : new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = leftAligned ? new Vector2(32f, -25f) : new Vector2(0f, -34f);
        titleRect.sizeDelta = new Vector2(560f, 48f);

        switch (page)
        {
            case Page.Main:
                AddEntry(list, () => "NEW GAME", StartGame);
                AddEntry(list, () => "OPTIONS", () => OpenSubPage(Page.Options));
                AddEntry(list, () => "CONTROLS", () => OpenSubPage(Page.Controls));
                AddEntry(list, () => "CREDITS", () => OpenSubPage(Page.Credits));
                AddEntry(list, () => "QUIT", () => OpenSubPage(Page.QuitConfirm));
                break;
            case Page.Pause:
                AddEntry(list, () => "RESUME", ResumeGameplay);
                AddEntry(list, () => "OPTIONS", () => OpenSubPage(Page.Options));
                AddEntry(list, () => "MAIN MENU", LoadMainMenu);
                AddEntry(list, () => "QUIT", () => OpenSubPage(Page.QuitConfirm));
                break;
            case Page.Options:
                AddEntry(list, () => $"MASTER VOLUME   {Mathf.RoundToInt(GameSettings.MasterVolume * 100f),3}%", () => ChangeMaster(1), ChangeMaster);
                AddEntry(list, () => $"MUSIC VOLUME    {Mathf.RoundToInt(GameSettings.MusicVolume * 100f),3}%", () => ChangeMusic(1), ChangeMusic);
                AddEntry(list, () => $"FULLSCREEN      {(GameSettings.Fullscreen ? "ON" : "OFF")}", ToggleFullscreen, _ => ToggleFullscreen());
                AddEntry(list, () => $"RESOLUTION      {CurrentResolutionLabel()}", () => ChangeResolution(1), ChangeResolution);
                AddEntry(list, () => "BACK", ReturnFromSubPage);
                break;
            case Page.Controls:
                CreateControls(list);
                AddEntry(list, () => "BACK", ReturnFromSubPage, null, 190f);
                break;
            case Page.Credits:
                CreateCredits(list);
                AddEntry(list, () => "BACK", ReturnFromSubPage, null, 150f);
                break;
            case Page.QuitConfirm:
                AddEntry(list, () => "YES", QuitGame);
                AddEntry(list, () => "NO", CancelQuit);
                break;
            case Page.Completion:
                CreateCompletionMessage(content);
                AddEntry(list, () => "CONTINUE EXPLORING", ResumeGameplay, null, 128f);
                AddEntry(list, () => "MAIN MENU", LoadMainMenu, null, 156f);
                break;
        }

        RefreshSelection();
    }

    private string GetPageTitle()
    {
        return page switch
        {
            Page.Main => "UNDER THE SOFA",
            Page.Pause => "PAUSED",
            Page.Options => "OPTIONS",
            Page.Controls => "CONTROLS",
            Page.Credits => "CREDITS",
            Page.QuitConfirm => "EXIT GAME?",
            Page.Completion => "GAME COMPLETE",
            _ => string.Empty
        };
    }

    private void AddEntry(RectTransform parent, Func<string> display, Action activate, Action<int> adjust = null, float? customY = null)
    {
        int index = entries.Count;
        Button button = RetroUiFactory.CreateButton($"Entry {index + 1}", parent, regularFont, display(), 16, out Text label);
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -(customY ?? (30f + index * 34f)));
        rect.sizeDelta = new Vector2(0f, 30f);

        Entry entry = new() { Button = button, Label = label, Display = display, Activate = activate, Adjust = adjust };
        button.onClick.AddListener(() => InvokeEntry(entry));
        entries.Add(entry);
    }

    private void CreateCredits(RectTransform parent)
    {
        Text credits = RetroUiFactory.CreateText("Credits Copy", parent, regularFont,
            "UNDER THE SOFA\n\nCREATED BY\nMATEJ SPAJIC", 14, TextAnchor.UpperCenter, RetroUiFactory.Cream);
        RectTransform rect = credits.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -18f);
        rect.sizeDelta = new Vector2(0f, 120f);
    }

    private void CreateControls(RectTransform parent)
    {
        Text keys = RetroUiFactory.CreateText("Control Keys", parent, regularFont,
            "W\nA\nS\nD\nR\nU\nSPACE", 13, TextAnchor.UpperLeft, RetroUiFactory.Cream);
        RectTransform keysRect = keys.rectTransform;
        keysRect.anchorMin = keysRect.anchorMax = new Vector2(0f, 1f);
        keysRect.pivot = new Vector2(0f, 1f);
        keysRect.anchoredPosition = new Vector2(28f, -12f);
        keysRect.sizeDelta = new Vector2(90f, 164f);

        Text actions = RetroUiFactory.CreateText("Control Actions", parent, regularFont,
            "ACCELERATE\n" +
            "STEER LEFT\n" +
            "BRAKE / REVERSE\n" +
            "STEER RIGHT\n" +
            "REWIND\n" +
            "UNSTUCK\n" +
            "HOP", 13, TextAnchor.UpperLeft, RetroUiFactory.Cream);
        RectTransform actionsRect = actions.rectTransform;
        actionsRect.anchorMin = actionsRect.anchorMax = new Vector2(0f, 1f);
        actionsRect.pivot = new Vector2(0f, 1f);
        actionsRect.anchoredPosition = new Vector2(135f, -12f);
        actionsRect.sizeDelta = new Vector2(230f, 164f);
    }

    private void CreateCompletionMessage(RectTransform parent)
    {
        Text message = RetroUiFactory.CreateText("Completion Copy", parent, regularFont, completionMessage, 13, TextAnchor.UpperCenter, RetroUiFactory.Cream);
        RectTransform rect = message.rectTransform;
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 38f);
        rect.sizeDelta = new Vector2(0f, 92f);
    }

    private void MoveSelection(int direction)
    {
        if (entries.Count == 0) return;
        selectedIndex = (selectedIndex + direction + entries.Count) % entries.Count;
        RefreshSelection();
        Play(selectSound);
    }

    private void AdjustSelection(int direction)
    {
        if (entries.Count == 0) return;
        Entry entry = entries[selectedIndex];
        if (entry.Adjust == null) return;
        entry.Adjust(direction);
        RefreshSelection();
        Play(selectSound);
    }

    private void ActivateSelection()
    {
        if (entries.Count == 0) return;
        InvokeEntry(entries[selectedIndex]);
    }

    private void InvokeEntry(Entry entry)
    {
        Play(okSound);
        entry.Activate?.Invoke();
    }

    private void RefreshSelection()
    {
        for (int index = 0; index < entries.Count; index++)
        {
            bool selected = index == selectedIndex;
            entries[index].Label.text = $"{(selected ? ">> " : "   ")}{entries[index].Display()}";
            entries[index].Label.color = selected ? RetroUiFactory.Cream : RetroUiFactory.Muted;
        }
    }

    private void OpenSubPage(Page next)
    {
        Page destination = page;
        ShowPage(next, destination);
        Play(menuSound);
    }

    private void ReturnFromSubPage()
    {
        ShowPage(returnPage);
        Play(backSound);
    }

    private void HandleBack()
    {
        switch (page)
        {
            case Page.Options:
            case Page.Controls:
            case Page.Credits:
                ReturnFromSubPage();
                break;
            case Page.QuitConfirm:
                CancelQuit();
                break;
            case Page.Pause:
            case Page.Completion:
                ResumeGameplay();
                break;
        }
    }

    private void ChangeMaster(int direction)
    {
        GameSettings.SetMasterVolume(GameSettings.MasterVolume + direction * 0.1f);
    }

    private void ChangeMusic(int direction)
    {
        GameSettings.SetMusicVolume(GameSettings.MusicVolume + direction * 0.1f);
    }

    private void ToggleFullscreen()
    {
        GameSettings.SetFullscreen(!GameSettings.Fullscreen);
    }

    private void ChangeResolution(int direction)
    {
        IReadOnlyList<GameSettings.ResolutionOption> options = GameSettings.Resolutions;
        int index = (GameSettings.GetCurrentResolutionIndex() + direction + options.Count) % options.Count;
        if (!GameSettings.SetResolution(options[index]))
        {
            Play(errorSound);
        }
    }

    private string CurrentResolutionLabel()
    {
        return $"{GameSettings.ResolutionWidth} X {GameSettings.ResolutionHeight}";
    }

    private void StartGame()
    {
        if (!transitionInProgress)
        {
            GameMusicController.Instance?.BeginGameplayTransition(menuMusicFadeOutDuration);
            StartCoroutine(NewGameTransition());
        }
    }

    private IEnumerator NewGameTransition()
    {
        transitionInProgress = true;
        foreach (Entry entry in entries)
        {
            entry.Button.interactable = false;
        }

        Camera menuCamera = Camera.main;
        Transform car = GameObject.Find("PlayerCar")?.transform;
        Vector3 startPosition = menuCamera != null ? menuCamera.transform.position : Vector3.zero;
        Quaternion startRotation = menuCamera != null ? menuCamera.transform.rotation : Quaternion.identity;
        float startFov = menuCamera != null ? menuCamera.fieldOfView : 60f;
        Vector3 targetPosition = startPosition;
        Quaternion targetRotation = startRotation;
        if (menuCamera != null && car != null)
        {
            Vector3 offset = menuCamera.transform.position - car.position;
            targetPosition = car.position + offset * 0.55f + Vector3.up * 0.2f;
            targetRotation = Quaternion.LookRotation(car.position + Vector3.up * 0.65f - targetPosition, Vector3.up);
        }

        float elapsed = 0f;
        while (elapsed < newGameZoomDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / newGameZoomDuration));
            if (menuCamera != null)
            {
                menuCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
                menuCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);
                menuCamera.fieldOfView = Mathf.Lerp(startFov, Mathf.Max(42f, startFov - 12f), progress);
            }
            yield return null;
        }

        RestoreRuntimeState();
        RetroSceneTransition.BeginLoad(
            gameplaySceneName,
            newGameFadeDuration,
            newGameGameplayFadeInDuration,
            menuCamera);
    }

    private void LoadMainMenu()
    {
        RestoreRuntimeState();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void CancelQuit()
    {
        ShowPage(returnPage);
        Play(cancelSound);
    }

    private void QuitGame()
    {
        RestoreRuntimeState();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void RestoreRuntimeState()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    private void Play(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
