using UnityEditor;
using UnityEngine;

public sealed class EnvironmentMeshColliderImporter : AssetPostprocessor
{
    private const string EnvironmentModelsPath = "Assets/Art/Environments/";

    private void OnPreprocessModel()
    {
        if (assetPath.StartsWith(EnvironmentModelsPath))
        {
            ((ModelImporter)assetImporter).addCollider = true;
        }
    }
}

[InitializeOnLoad]
public static class SceneModelMeshColliderAdder
{
    static SceneModelMeshColliderAdder()
    {
        EditorApplication.hierarchyChanged += AddMissingColliders;
        EditorApplication.delayCall += AddMissingColliders;
    }

    private static void AddMissingColliders()
    {
        foreach (MeshFilter meshFilter in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include))
        {
            if (!meshFilter.gameObject.scene.IsValid() ||
                !PrefabUtility.IsPartOfModelPrefab(meshFilter.gameObject) ||
                meshFilter.GetComponentInParent<Rigidbody>() != null ||
                meshFilter.sharedMesh == null ||
                meshFilter.TryGetComponent<MeshCollider>(out _))
            {
                continue;
            }

            Undo.AddComponent<MeshCollider>(meshFilter.gameObject).sharedMesh = meshFilter.sharedMesh;
        }
    }
}
