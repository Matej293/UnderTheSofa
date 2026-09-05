using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(BoxCollider))]
public sealed class VehicleChassisColliderFitter : MonoBehaviour
{
    [SerializeField] private Transform modelRoot;
    [SerializeField, Min(0f)] private float padding = 0.04f;

    private BoxCollider chassisCollider;

    private void Awake()
    {
        FitCollider();
    }

    private void OnValidate()
    {
        FitCollider();
    }

    [ContextMenu("Fit Chassis Collider To Model")]
    public void FitCollider()
    {
        chassisCollider ??= GetComponent<BoxCollider>();
        Transform root = modelRoot != null ? modelRoot : transform;
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);

        bool hasBounds = false;
        Bounds localBounds = default;
        foreach (Renderer modelRenderer in renderers)
        {
            Bounds worldBounds = modelRenderer.bounds;
            Vector3 min = worldBounds.min;
            Vector3 max = worldBounds.max;

            for (int x = 0; x <= 1; x++)
            {
                for (int y = 0; y <= 1; y++)
                {
                    for (int z = 0; z <= 1; z++)
                    {
                        Vector3 worldCorner = new Vector3(
                            x == 0 ? min.x : max.x,
                            y == 0 ? min.y : max.y,
                            z == 0 ? min.z : max.z);
                        Vector3 localCorner = transform.InverseTransformPoint(worldCorner);

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

        if (!hasBounds)
        {
            return;
        }

        chassisCollider.center = localBounds.center;
        chassisCollider.size = localBounds.size + Vector3.one * (padding * 2f);
    }
}
