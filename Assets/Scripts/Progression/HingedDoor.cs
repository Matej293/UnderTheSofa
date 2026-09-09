using System.Collections;
using UnityEngine;

public sealed class HingedDoor : MonoBehaviour
{
    [SerializeField] private Transform door;
    [SerializeField] private float openAngle = 90f;
    [SerializeField, Min(0.01f)] private float openDuration = 1f;

    private bool isOpen;

    public void Open()
    {
        if (isOpen)
        {
            return;
        }

        if (door == null)
        {
            Debug.LogError("Assign the door Transform in HingedDoor before playing.", this);
            return;
        }

        isOpen = true;
        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        Quaternion closedRotation = door.rotation;
        Quaternion openRotation = Quaternion.AngleAxis(openAngle, Vector3.up) * closedRotation;
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            door.rotation = Quaternion.Slerp(closedRotation, openRotation, elapsed / openDuration);
            yield return null;
        }

        door.rotation = openRotation;
    }
}
