// ================================================================
// 家具色替え工房 — シーン自動セットアップ
// Unity Editor メニュー: FurnitureWorkshop > Setup Scene
// ================================================================
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using FurnitureWorkshop;

public static class SceneBootstrap
{
    [MenuItem("FurnitureWorkshop/🪑 Setup Scene (全自動セットアップ)")]
    public static void SetupScene()
    {
        // --- 既存シーンをクリア ---
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ============================================================
        // 1. ライティング
        // ============================================================
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = new Color(1f, 0.97f, 0.88f);
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // 環境光
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.25f, 0.25f, 0.3f);

        // ============================================================
        // 2. カメラ
        // ============================================================
        var cameraGO = new GameObject("Main Camera");
        cameraGO.tag = "MainCamera";
        var cam = cameraGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.17f, 0.17f, 0.2f);
        cam.fieldOfView = 45f;
        cameraGO.transform.position = new Vector3(0f, 1.2f, -3.5f);
        cameraGO.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
        cameraGO.AddComponent<AudioListener>();

        var colorController = cameraGO.AddComponent<FurnitureColorController>();

        // ============================================================
        // 3. GameManager (空のGameObject)
        // ============================================================
        var managerGO = new GameObject("GameManager");
        var gameManager = managerGO.AddComponent<GameManager>();
        var loader = managerGO.AddComponent<FurnitureLoader>();

        // ============================================================
        // 4. 家具スポーン地点
        // ============================================================
        var spawnPoint = new GameObject("FurnitureSpawnPoint");
        spawnPoint.transform.position = Vector3.zero;

        // FurnitureLoader の spawnParent に設定
        var loaderSO = new SerializedObject(loader);
        loaderSO.FindProperty("spawnParent").objectReferenceValue = spawnPoint.transform;
        loaderSO.ApplyModifiedProperties();

        // ============================================================
        // 5. プレースホルダー家具（椅子）
        // ============================================================
        var chairRoot = new GameObject("PlaceholderChair");
        chairRoot.tag = "FurnitureRoot";
        chairRoot.transform.SetParent(spawnPoint.transform);
        chairRoot.transform.localPosition = Vector3.zero;

        CreateChairPart(chairRoot, "Seat",
            new Vector3(0, 0.45f, 0), new Vector3(0.7f, 0.08f, 0.7f));
        CreateChairPart(chairRoot, "Back",
            new Vector3(0, 0.85f, 0.31f), new Vector3(0.7f, 0.75f, 0.06f));
        CreateChairPart(chairRoot, "Leg_FL",
            new Vector3(-0.28f, 0.2f, -0.28f), new Vector3(0.07f, 0.4f, 0.07f));
        CreateChairPart(chairRoot, "Leg_FR",
            new Vector3(0.28f, 0.2f, -0.28f), new Vector3(0.07f, 0.4f, 0.07f));
        CreateChairPart(chairRoot, "Leg_BL",
            new Vector3(-0.28f, 0.2f, 0.28f), new Vector3(0.07f, 0.4f, 0.07f));
        CreateChairPart(chairRoot, "Leg_BR",
            new Vector3(0.28f, 0.2f, 0.28f), new Vector3(0.07f, 0.4f, 0.07f));

        // ============================================================
        // 6. UI Canvas
        // ============================================================
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem
        var eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<EventSystem>();
        eventSystemGO.AddComponent<StandaloneInputModule>();

        // --- 操作説明パネル（上部左） ---
        var instructPanel = CreatePanel(canvasGO.transform, "Panel_Instructions",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20),
            new Vector2(400, 50));
        var instructText = CreateText(instructPanel.transform, "Text_Instructions",
            "家具のパーツをクリックして色を選ぼう！\nClick a furniture part, then pick a color.",
            14, TextAnchor.MiddleLeft);
        instructText.rectTransform.anchorMin = Vector2.zero;
        instructText.rectTransform.anchorMax = Vector2.one;
        instructText.rectTransform.offsetMin = new Vector2(8, 2);
        instructText.rectTransform.offsetMax = new Vector2(-8, -2);

        // --- モデル選択パネル（上部右） ---
        var modelPanel = CreatePanel(canvasGO.transform, "Panel_ModelSelector",
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -20),
            new Vector2(280, 50));
        var modelSelector = modelPanel.AddComponent<ModelSelectorUI>();

        var dropdown = CreateDropdown(modelPanel.transform, "Dropdown_Models",
            new Vector2(-75, 0), new Vector2(130, 36));
        var loadBtn = CreateButton(modelPanel.transform, "Button_Load", "読み込む",
            new Vector2(85, 0), new Vector2(80, 36));

        var modelSelectorSO = new SerializedObject(modelSelector);
        modelSelectorSO.FindProperty("modelDropdown").objectReferenceValue = dropdown;
        modelSelectorSO.FindProperty("loadButton").objectReferenceValue = loadBtn;
        modelSelectorSO.ApplyModifiedProperties();

        // --- カラーパレットパネル（下部中央） ---
        var palettePanel = CreatePanel(canvasGO.transform, "Panel_ColorPalette",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 20),
            new Vector2(700, 120));
        palettePanel.GetComponent<Image>().color = new Color(0.12f, 0.12f, 0.15f, 0.92f);

        var paletteLabel = CreateText(palettePanel.transform, "Label",
            "カラーパレット", 12, TextAnchor.UpperCenter);
        paletteLabel.rectTransform.anchorMin = new Vector2(0, 1);
        paletteLabel.rectTransform.anchorMax = new Vector2(1, 1);
        paletteLabel.rectTransform.sizeDelta = new Vector2(0, 20);
        paletteLabel.rectTransform.anchoredPosition = new Vector2(0, -2);

        var swatchContainer = new GameObject("SwatchContainer");
        var containerRT = swatchContainer.AddComponent<RectTransform>();
        containerRT.SetParent(palettePanel.transform, false);
        containerRT.anchorMin = new Vector2(0, 0);
        containerRT.anchorMax = new Vector2(1, 1);
        containerRT.offsetMin = new Vector2(8, 8);
        containerRT.offsetMax = new Vector2(-8, -24);

        var grid = swatchContainer.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(52, 52);
        grid.spacing = new Vector2(6, 6);
        grid.childAlignment = TextAnchor.MiddleCenter;

        // スウォッチボタンプレハブを動的生成
        var swatchPrefab = CreateSwatchPrefab();

        var colorPaletteUI = palettePanel.AddComponent<ColorPaletteUI>();
        var paletteSO = new SerializedObject(colorPaletteUI);
        paletteSO.FindProperty("swatchContainer").objectReferenceValue = swatchContainer.transform;
        paletteSO.FindProperty("swatchButtonPrefab").objectReferenceValue = swatchPrefab;
        paletteSO.ApplyModifiedProperties();

        // --- ローディングオーバーレイ ---
        var loadingPanel = CreatePanel(canvasGO.transform, "Panel_Loading",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        loadingPanel.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        loadingPanel.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        loadingPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.7f);
        loadingPanel.SetActive(false);

        var loadingText = CreateText(loadingPanel.transform, "Text_Loading",
            "読み込み中...", 24, TextAnchor.MiddleCenter);
        loadingText.rectTransform.anchorMin = new Vector2(0.3f, 0.4f);
        loadingText.rectTransform.anchorMax = new Vector2(0.7f, 0.6f);
        loadingText.rectTransform.offsetMin = Vector2.zero;
        loadingText.rectTransform.offsetMax = Vector2.zero;

        // ============================================================
        // 7. GameManager にコンポーネント参照をセット
        // ============================================================
        var gmSO = new SerializedObject(gameManager);
        gmSO.FindProperty("furnitureLoader").objectReferenceValue = loader;
        gmSO.FindProperty("colorPaletteUI").objectReferenceValue = colorPaletteUI;
        gmSO.FindProperty("colorController").objectReferenceValue = colorController;
        gmSO.ApplyModifiedProperties();

        // ModelSelectorUI に GameManager を接続
        var msSO = new SerializedObject(modelSelector);
        msSO.FindProperty("gameManager").objectReferenceValue = gameManager;
        msSO.FindProperty("loadingPanel").objectReferenceValue = loadingPanel;
        msSO.ApplyModifiedProperties();

        // ============================================================
        // 8. シーン保存
        // ============================================================
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainScene.unity");
        AssetDatabase.Refresh();

        Debug.Log("✅ シーンセットアップ完了！ Assets/Scenes/MainScene.unity に保存しました。");
        EditorUtility.DisplayDialog(
            "セットアップ完了！",
            "シーンの作成が完了しました。\n\n" +
            "▶ Play ボタンで動作確認できます。\n" +
            "▶ 自作 .glb を StreamingAssets/models/ に入れてください。",
            "OK");
    }

    // ============================================================
    // ヘルパー: 椅子パーツ
    // ============================================================
    private static void CreateChairPart(GameObject parent, string partName,
        Vector3 localPos, Vector3 scale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = partName;
        go.tag = "FurniturePart";
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;

        var mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.76f, 0.60f, 0.42f); // 木目ベージュ
        go.GetComponent<Renderer>().sharedMaterial = mat;
    }

    // ============================================================
    // ヘルパー: UI
    // ============================================================
    private static GameObject CreatePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.18f, 0.85f);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        return go;
    }

    private static Text CreateText(Transform parent, string name, string content,
        int fontSize, TextAnchor alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.text = content;
        t.fontSize = fontSize;
        t.alignment = alignment;
        t.color = Color.white;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return t;
    }

    private static Dropdown CreateDropdown(Transform parent, string name,
        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        go.AddComponent<Image>().color = new Color(0.25f, 0.25f, 0.3f);
        var dd = go.AddComponent<Dropdown>();

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var labelText = labelGO.AddComponent<Text>();
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.color = Color.white;
        labelText.fontSize = 12;
        labelText.alignment = TextAnchor.MiddleLeft;
        var labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = new Vector2(8, 2);
        labelRT.offsetMax = new Vector2(-24, -2);
        dd.captionText = labelText;

        var templateGO = new GameObject("Template");
        templateGO.transform.SetParent(go.transform, false);
        templateGO.SetActive(false);
        templateGO.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f);
        var templateRT = templateGO.GetComponent<RectTransform>();
        templateRT.anchorMin = new Vector2(0, 0);
        templateRT.anchorMax = new Vector2(1, 0);
        templateRT.pivot = new Vector2(0.5f, 1f);
        templateRT.sizeDelta = new Vector2(0, 120);
        templateGO.AddComponent<ScrollRect>();
        dd.template = templateRT;

        var itemGO = new GameObject("Item");
        itemGO.transform.SetParent(templateGO.transform, false);
        itemGO.AddComponent<Toggle>();
        itemGO.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.35f);
        var itemRT = itemGO.GetComponent<RectTransform>();
        itemRT.anchorMin = new Vector2(0, 0.5f);
        itemRT.anchorMax = new Vector2(1, 0.5f);
        itemRT.sizeDelta = new Vector2(0, 24);

        var itemLabelGO = new GameObject("Item Label");
        itemLabelGO.transform.SetParent(itemGO.transform, false);
        var itemLabel = itemLabelGO.AddComponent<Text>();
        itemLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        itemLabel.color = Color.white;
        itemLabel.fontSize = 11;
        itemLabel.alignment = TextAnchor.MiddleLeft;
        var itemLabelRT = itemLabelGO.GetComponent<RectTransform>();
        itemLabelRT.anchorMin = Vector2.zero;
        itemLabelRT.anchorMax = Vector2.one;
        itemLabelRT.offsetMin = new Vector2(8, 1);
        itemLabelRT.offsetMax = new Vector2(-8, -1);
        dd.itemText = itemLabel;

        return dd;
    }

    private static Button CreateButton(Transform parent, string name, string label,
        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        var img = go.AddComponent<Image>();
        img.color = new Color(0.3f, 0.55f, 0.9f);
        var btn = go.AddComponent<Button>();

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var t = textGO.AddComponent<Text>();
        t.text = label;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 12;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        return btn;
    }

    private static GameObject CreateSwatchPrefab()
    {
        var go = new GameObject("SwatchButton");
        go.AddComponent<Image>();
        go.AddComponent<Button>();
        return go;
    }
}
