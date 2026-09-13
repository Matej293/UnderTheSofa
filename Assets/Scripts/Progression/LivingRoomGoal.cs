using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class LivingRoomGoal : MonoBehaviour
{
    [Header("Completion")]
    [SerializeField] private HingedDoor[] doorsToOpen;
    [SerializeField] private GameObject[] objectsToEnable;

    [Header("Message")]
    [SerializeField] private string completionMessage = "DOOR UNLOCKED";
    [SerializeField, Min(0f)] private float messageDuration = 2f;
    [SerializeField] private bool isGameCompletion;
    [SerializeField] private bool playNotificationSound = true;

    public bool IsCompleted { get; private set; }

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

        if (RetroGameHUD.Instance != null)
        {
            RetroGameHUD.Instance.ShowNotification(completionMessage, messageDuration, isGameCompletion, playNotificationSound);
        }
        else
        {
            Debug.Log(completionMessage);
        }
    }
}
