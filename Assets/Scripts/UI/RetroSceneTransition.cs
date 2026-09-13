using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class RetroSceneTransition : MonoBehaviour
{
    private static RetroSceneTransition instance;

    private CanvasGroup fadeGroup;
    private string pendingSceneName;
    private bool hasPendingCameraPose;
    private Vector3 pendingCameraPosition;
    private Quaternion pendingCameraRotation;
    private float pendingCameraFieldOfView;
    private Camera incomingCamera;
    private float incomingCameraFieldOfView;
    private bool cameraPoseApplied;

    public static void BeginLoad(string sceneName, float fadeDuration, Camera outgoingCamera = null)
    {
        RetroSceneTransition transition = EnsureInstance();
        transition.CaptureCameraPose(outgoingCamera);
        transition.StartCoroutine(transition.LoadRoutine(sceneName, Mathf.Max(0.1f, fadeDuration)));
    }

    private static RetroSceneTransition EnsureInstance()
    {
        if (instance != null) return instance;

        GameObject root = new GameObject("Persistent Scene Transition");
        instance = root.AddComponent<RetroSceneTransition>();
        DontDestroyOnLoad(root);
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Canvas canvas = RetroUiFactory.CreateCanvas("Persistent Scene Transition Canvas", 1000);
        canvas.transform.SetParent(transform, false);
        Image fade = RetroUiFactory.CreateImage("Fade", canvas.transform, Color.black);
        RetroUiFactory.Stretch(fade.rectTransform);
        fade.raycastTarget = true;
        fadeGroup = fade.gameObject.AddComponent<CanvasGroup>();
        fadeGroup.alpha = 0f;
        fade.gameObject.SetActive(false);
    }

    private void CaptureCameraPose(Camera outgoingCamera)
    {
        hasPendingCameraPose = outgoingCamera != null;
        if (!hasPendingCameraPose)
        {
            return;
        }

        pendingCameraPosition = outgoingCamera.transform.position;
        pendingCameraRotation = outgoingCamera.transform.rotation;
        pendingCameraFieldOfView = outgoingCamera.fieldOfView;
    }

    private IEnumerator LoadRoutine(string sceneName, float fadeDuration)
    {
        fadeGroup.gameObject.SetActive(true);
        fadeGroup.alpha = 0f;

        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        if (load == null)
        {
            fadeGroup.gameObject.SetActive(false);
            yield break;
        }

        load.allowSceneActivation = false;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        while (load.progress < 0.9f)
        {
            yield return null;
        }

        fadeGroup.alpha = 1f;
        pendingSceneName = sceneName;
        cameraPoseApplied = !hasPendingCameraPose;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        load.allowSceneActivation = true;
        while (SceneManager.GetActiveScene().name != sceneName)
        {
            yield return null;
        }

        // Keep the screen covered until the outgoing pose has been applied to the new camera.
        while (!cameraPoseApplied)
        {
            ApplyCameraPose();
            yield return null;
        }

        // Give the render texture and chase camera one covered frame to initialize.
        yield return null;
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / fadeDuration));
            fadeGroup.alpha = 1f - progress;
            if (incomingCamera != null)
            {
                incomingCamera.fieldOfView = Mathf.Lerp(pendingCameraFieldOfView, incomingCameraFieldOfView, progress);
            }
            yield return null;
        }

        if (incomingCamera != null)
        {
            incomingCamera.fieldOfView = incomingCameraFieldOfView;
        }
        fadeGroup.alpha = 0f;
        fadeGroup.gameObject.SetActive(false);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != pendingSceneName)
        {
            return;
        }

        ApplyCameraPose();
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void ApplyCameraPose()
    {
        if (!hasPendingCameraPose)
        {
            cameraPoseApplied = true;
            return;
        }

        Camera gameplayCamera = Camera.main;
        if (gameplayCamera == null || gameplayCamera.gameObject.scene.name != pendingSceneName)
        {
            return;
        }

        incomingCamera = gameplayCamera;
        incomingCameraFieldOfView = gameplayCamera.fieldOfView;
        gameplayCamera.transform.SetPositionAndRotation(pendingCameraPosition, pendingCameraRotation);
        gameplayCamera.fieldOfView = pendingCameraFieldOfView;
        cameraPoseApplied = true;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }
}
