// ================================================================
// GLBファイルを StreamingAssets/models/ に置くだけで
// models_manifest.json が自動更新されます。手動編集不要！
// ================================================================
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ModelManifestUpdater : AssetPostprocessor
{
    private const string ModelsFolder = "Assets/StreamingAssets/models";
    private const string ManifestPath = "Assets/StreamingAssets/models_manifest.json";

    // GLBファイルが追加・削除・移動されたときに自動実行
    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        bool needsUpdate = false;

        foreach (var path in importedAssets)
            if (IsGlbInModelsFolder(path)) needsUpdate = true;

        foreach (var path in deletedAssets)
            if (IsGlbInModelsFolder(path)) needsUpdate = true;

        foreach (var path in movedAssets)
            if (IsGlbInModelsFolder(path)) needsUpdate = true;

        foreach (var path in movedFromAssetPaths)
            if (IsGlbInModelsFolder(path)) needsUpdate = true;

        if (needsUpdate) RebuildManifest();
    }

    private static bool IsGlbInModelsFolder(string path)
    {
        return path.StartsWith(ModelsFolder) &&
               (path.EndsWith(".glb") || path.EndsWith(".gltf"));
    }

    [MenuItem("FurnitureWorkshop/🔄 マニフェストを今すぐ更新")]
    public static void RebuildManifest()
    {
        if (!Directory.Exists(ModelsFolder))
            Directory.CreateDirectory(ModelsFolder);

        var files = Directory.GetFiles(ModelsFolder, "*.glb");
        var gltfFiles = Directory.GetFiles(ModelsFolder, "*.gltf");

        var models = new List<string>();
        foreach (var f in files)   models.Add(Path.GetFileName(f));
        foreach (var f in gltfFiles) models.Add(Path.GetFileName(f));
        models.Sort();

        var json = "{\n  \"models\": [\n";
        for (int i = 0; i < models.Count; i++)
        {
            json += $"    \"{models[i]}\"";
            if (i < models.Count - 1) json += ",";
            json += "\n";
        }
        json += "  ]\n}";

        File.WriteAllText(ManifestPath, json);
        AssetDatabase.Refresh();

        if (models.Count == 0)
            Debug.Log($"[FurnitureWorkshop] モデルが見つかりません。{ModelsFolder} に .glb を入れてください。");
        else
            Debug.Log($"[FurnitureWorkshop] ✅ {models.Count} 個のモデルを登録しました: {string.Join(", ", models)}");
    }
}
