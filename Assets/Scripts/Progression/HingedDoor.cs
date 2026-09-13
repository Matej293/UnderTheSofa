using System.Collections;
using UnityEngine;

public sealed class HingedDoor : MonoBehaviour
{
    [SerializeField] private Transform door;
    [SerializeField] private float openAngle = 90f;
    [SerializeField, Min(0.01f)] private float openDuration = 1f;
    [SerializeField] private AudioSource openingAudio;

    [Header("Optional Hinge Pivot")]
    [SerializeField] private bool useLocalBoundsHinge;
    [SerializeField] private bool hingeAtPositiveLocalX;

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
        openingAudio?.Play();
        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        Vector3 closedPosition = door.position;
        Quaternion closedRotation = door.rotation;
        Quaternion openRotation = Quaternion.AngleAxis(openAngle, Vector3.up) * closedRotation;
        Vector3 hingePosition = GetHingePosition(closedPosition);
        Vector3 hingeToDoor = closedPosition - hingePosition;
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / openDuration);
            Quaternion rotationOffset = Quaternion.AngleAxis(openAngle * progress, Vector3.up);
            door.SetPositionAndRotation(
                hingePosition + rotationOffset * hingeToDoor,
                Quaternion.Slerp(closedRotation, openRotation, progress));
            yield return null;
        }

        door.SetPositionAndRotation(
            hingePosition + Quaternion.AngleAxis(openAngle, Vector3.up) * hingeToDoor,
            openRotation);
    }

    private Vector3 GetHingePosition(Vector3 fallbackPosition)
    {
        if (!useLocalBoundsHinge || !TryGetLocalBounds(out Bounds localBounds))
        {
            return fallbackPosition;
        }

        float hingeX = hingeAtPositiveLocalX ? localBounds.max.x : localBounds.min.x;
        Vector3 localHinge = new Vector3(hingeX, localBounds.center.y, localBounds.center.z);
        return door.TransformPoint(localHinge);
    }

    private bool TryGetLocalBounds(out Bounds localBounds)
    {
        MeshFilter[] meshFilters = door.GetComponentsInChildren<MeshFilter>();
        localBounds = default;
        bool hasBounds = false;

        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh == null)
            {
                continue;
            }

            Bounds meshBounds = meshFilter.sharedMesh.bounds;
            Vector3 min = meshBounds.min;
            Vector3 max = meshBounds.max;

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        Vector3 meshCorner = new Vector3(
                            x == 0 ? min.x : max.x,
                            y == 0 ? min.y : max.y,
                            z == 0 ? min.z : max.z);
                        Vector3 localCorner = door.InverseTransformPoint(meshFilter.transform.TransformPoint(meshCorner));

                        if (hasBounds)
                        {
                            localBounds.Encapsulate(localCorner);
                        }
                        else
                        {
                            localBounds = new Bounds(localCorner, Vector3.zero);
                            hasBounds = true;
                        }
                    }
                }
            }
        }

        return hasBounds;
    }
}
