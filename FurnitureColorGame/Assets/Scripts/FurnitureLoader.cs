using System.Collections.Generic;
using System.Threading.Tasks;
using GLTFast;
using UnityEngine;
using UnityEngine.Networking;

namespace FurnitureWorkshop
{
    [System.Serializable]
    public class ModelsManifest
    {
        public List<string> models = new();
    }

    public class FurnitureLoader : MonoBehaviour
    {
        [SerializeField] private Transform spawnParent;
        [SerializeField] private Vector3 defaultSpawnPosition = Vector3.zero;

        public async Task<GameObject> LoadAsync(string fileName)
        {
            string url = BuildUrl(fileName);
            Debug.Log($"[FurnitureLoader] Loading: {url}");

            var root = new GameObject(System.IO.Path.GetFileNameWithoutExtension(fileName));
            root.transform.SetParent(spawnParent);
            root.transform.localPosition = defaultSpawnPosition;

            var gltf = new GltfImport();
            bool success = await gltf.Load(url);

            if (!success)
            {
                Debug.LogError($"[FurnitureLoader] glTFast failed to load: {url}");
                Destroy(root);
                return null;
            }

            await gltf.InstantiateMainSceneAsync(root.transform);
            root.tag = "FurnitureRoot";
            TagAndCollideAllParts(root);

            return root;
        }

        public async Task<List<string>> GetAvailableModels()
        {
            string manifestUrl = BuildUrl("../models_manifest.json");
            using var req = UnityWebRequest.Get(manifestUrl);
            var op = req.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[FurnitureLoader] Could not read manifest: {req.error}");
                return new List<string>();
            }

            var manifest = JsonUtility.FromJson<ModelsManifest>(req.downloadHandler.text);
            return manifest?.models ?? new List<string>();
        }

        private string BuildUrl(string fileName)
        {
            string basePath = System.IO.Path.Combine(Application.streamingAssetsPath, "models", fileName);
#if UNITY_EDITOR || UNITY_STANDALONE
            if (!basePath.StartsWith("http"))
                basePath = "file://" + basePath;
#endif
            return basePath;
        }

        private void TagAndCollideAllParts(GameObject root)
        {
            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                var go = renderer.gameObject;
                go.tag = "FurniturePart";
                if (go.GetComponent<MeshCollider>() == null)
                {
                    var col = go.AddComponent<MeshCollider>();
                    col.sharedMesh = go.GetComponent<MeshFilter>()?.sharedMesh;
                }
            }
        }
    }
}
