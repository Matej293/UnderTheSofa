using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public sealed class LivingRoomGoal : MonoBehaviour
{
    [Header("Completion")]
    [SerializeField] private HingedDoor[] doorsToOpen;
    [SerializeField] private GameObject[] objectsToEnable;

    [Header("Message")]
    [SerializeField] private string completionMessage = "DOOR UNLOCKED";
    [SerializeField, Min(0f)] private float messageDuration = 2f;

    public bool IsCompleted { get; private set; }

    private Text messageText;
    private Coroutine hideMessageRoutine;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsCompleted || other.GetComponentInParent<ArcadeVehicleController>() == null)
        {
            return;
        }

        Complete();
    }

    public void Complete()
    {
        if (IsCompleted)
        {
            return;
        }

        IsCompleted = true;
        foreach (HingedDoor door in doorsToOpen)
        {
            if (door != null)
            {
                door.Open();
            }
        }

        foreach (GameObject target in objectsToEnable)
        {
            if (target != null)
            {
                target.SetActive(true);
            }
        }

        ShowMessage();
    }

    private void ShowMessage()
    {
        if (messageDuration <= 0f || string.IsNullOrWhiteSpace(completionMessage))
        {
            return;
        }

        if (messageText == null)
        {
            Canvas canvas = new GameObject("GoalMessageCanvas", typeof(Canvas), typeof(CanvasScaler)).GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 102;

            messageText = new GameObject("GoalMessage", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            messageText.transform.SetParent(canvas.transform, false);
            messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            messageText.fontSize = 28;
            messageText.alignment = TextAnchor.UpperCenter;
            messageText.color = Color.white;
            RectTransform rect = messageText.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -72f);
            rect.sizeDelta = new Vector2(500f, 44f);
        }

        messageText.text = completionMessage;
        messageText.gameObject.SetActive(true);
        if (hideMessageRoutine != null)
        {
            StopCoroutine(hideMessageRoutine);
        }

        hideMessageRoutine = StartCoroutine(HideMessageAfterDelay());
    }

    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }

        hideMessageRoutine = null;
    }
}
