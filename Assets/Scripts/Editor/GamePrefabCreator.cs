using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;

/// <summary>
/// 游戏预制体创建工具
/// 创建 GameManager、UI 等预制体
/// </summary>
public class GamePrefabCreator : EditorWindow
{
    private static string prefabPath = "Assets/Prefabs";

    [MenuItem("Tools/Game Setup/Create GameManager Prefab")]
    public static void CreateGameManagerPrefab()
    {
        Directory.CreateDirectory(prefabPath);
        Directory.CreateDirectory($"{prefabPath}/Managers");

        // 创建GameManager对象
        GameObject gameManager = new GameObject("GameManager");
        gameManager.AddComponent<GameManager>();
        gameManager.AddComponent<SceneLoader>();

        // 保存为预制体
        string path = $"{prefabPath}/Managers/GameManager.prefab";
        PrefabUtility.SaveAsPrefabAsset(gameManager, path);
        Object.DestroyImmediate(gameManager);

        Debug.Log($"[GamePrefabCreator] Created GameManager prefab at {path}");
    }

    [MenuItem("Tools/Game Setup/Create UI Prefabs")]
    public static void CreateUIPrefabs()
    {
        Directory.CreateDirectory($"{prefabPath}/UI");

        CreateMainMenuUIPrefab();
        CreateGameHUDPrefab();
        CreatePauseMenuPrefab();
        CreateInteractionPromptPrefab();

        Debug.Log("[GamePrefabCreator] All UI prefabs created!");
    }

    private static void CreateMainMenuUIPrefab()
    {
        GameObject canvas = CreateBaseCanvas("MainMenuCanvas");
        GameObject panel = CreatePanel(canvas.transform, "MainMenuPanel", new Color(0.1f, 0.1f, 0.15f, 0.95f));

        // 标题
        CreateText(panel.transform, "Title", "游戏标题", 64, new Vector2(0, 200), new Vector2(800, 100));

        // 按钮
        CreateUIButton(panel.transform, "StartButton", "开始游戏", new Vector2(0, 50));
        CreateUIButton(panel.transform, "LevelSelectButton", "关卡选择", new Vector2(0, -30));
        CreateUIButton(panel.transform, "QuitButton", "退出游戏", new Vector2(0, -110));

        // 添加MainMenuUI脚本
        panel.AddComponent<MainMenuUI>();

        string path = $"{prefabPath}/UI/MainMenuUI.prefab";
        PrefabUtility.SaveAsPrefabAsset(canvas, path);
        Object.DestroyImmediate(canvas);
    }

    private static void CreateGameHUDPrefab()
    {
        GameObject canvas = CreateBaseCanvas("GameHUDCanvas");
        GameObject panel = CreatePanel(canvas.transform, "HUDPanel", new Color(0, 0, 0, 0));

        // 生命值条
        GameObject healthBar = CreateSlider(panel.transform, "HealthBar", new Vector2(-700, 450), new Vector2(300, 30));
        CreateText(healthBar.transform, "HealthText", "100/100", 18, Vector2.zero, new Vector2(100, 20));

        // 关卡名称
        CreateText(panel.transform, "LevelName", "关卡 1", 24, new Vector2(0, 480), new Vector2(400, 40));

        // 暂停按钮
        CreateUIButton(panel.transform, "PauseButton", "暂停", new Vector2(850, 450));

        // 交互提示
        GameObject prompt = CreatePanel(panel.transform, "InteractionPrompt", new Color(0, 0, 0, 0.7f));
        prompt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -200);
        prompt.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 50);
        CreateText(prompt.transform, "PromptText", "按 F 键交互", 24, Vector2.zero, new Vector2(250, 40));
        prompt.SetActive(false);

        // 添加GameHUD脚本
        panel.AddComponent<GameHUD>();

        string path = $"{prefabPath}/UI/GameHUD.prefab";
        PrefabUtility.SaveAsPrefabAsset(canvas, path);
        Object.DestroyImmediate(canvas);
    }

    private static void CreatePauseMenuPrefab()
    {
        GameObject canvas = CreateBaseCanvas("PauseCanvas");
        GameObject panel = CreatePanel(canvas.transform, "PausePanel", new Color(0, 0, 0, 0.8f));

        // 标题
        CreateText(panel.transform, "Title", "游戏暂停", 48, new Vector2(0, 150), new Vector2(400, 80));

        // 按钮
        CreateUIButton(panel.transform, "ResumeButton", "继续游戏", new Vector2(0, 30));
        CreateUIButton(panel.transform, "RestartButton", "重新开始", new Vector2(0, -50));
        CreateUIButton(panel.transform, "MainMenuButton", "返回主菜单", new Vector2(0, -130));

        // 添加PauseMenuUI脚本
        panel.AddComponent<PauseMenuUI>();

        string path = $"{prefabPath}/UI/PauseMenu.prefab";
        PrefabUtility.SaveAsPrefabAsset(canvas, path);
        Object.DestroyImmediate(canvas);
    }

    private static void CreateInteractionPromptPrefab()
    {
        GameObject canvas = CreateBaseCanvas("InteractionPromptCanvas");
        canvas.GetComponent<Canvas>().sortingOrder = 200;

        GameObject panel = CreatePanel(canvas.transform, "PromptPanel", new Color(0.2f, 0.2f, 0.2f, 0.8f));
        panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -300);
        panel.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 50);

        CreateText(panel.transform, "PromptText", "按 F 键交互", 20, Vector2.zero, new Vector2(200, 40));

        string path = $"{prefabPath}/UI/InteractionPrompt.prefab";
        PrefabUtility.SaveAsPrefabAsset(canvas, path);
        Object.DestroyImmediate(canvas);
    }

    #region Helper Methods

    private static GameObject CreateBaseCanvas(string name)
    {
        GameObject canvasObj = new GameObject(name);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        return canvasObj;
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        UnityEngine.UI.Image image = panel.AddComponent<UnityEngine.UI.Image>();
        image.color = color;

        return panel;
    }

    private static GameObject CreateText(Transform parent, string name, string text, int fontSize, Vector2 position, Vector2 size)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        UnityEngine.UI.Text uiText = textObj.AddComponent<UnityEngine.UI.Text>();
        uiText.text = text;
        uiText.fontSize = fontSize;
        uiText.fontStyle = FontStyle.Bold;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.color = Color.white;

        return textObj;
    }

    private static GameObject CreateUIButton(Transform parent, string name, string text, Vector2 position)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(250, 50);
        rect.anchoredPosition = position;

        UnityEngine.UI.Image image = buttonObj.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.2f, 0.5f, 0.8f);

        buttonObj.AddComponent<UnityEngine.UI.Button>();

        // 添加文字
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        UnityEngine.UI.Text uiText = textObj.AddComponent<UnityEngine.UI.Text>();
        uiText.text = text;
        uiText.fontSize = 20;
        uiText.fontStyle = FontStyle.Bold;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.color = Color.white;

        return buttonObj;
    }

    private static GameObject CreateSlider(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent, false);

        RectTransform rect = sliderObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        UnityEngine.UI.Image bgImage = sliderObj.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.3f);

        UnityEngine.UI.Slider slider = sliderObj.AddComponent<UnityEngine.UI.Slider>();

        return sliderObj;
    }

    #endregion

    [MenuItem("Tools/Game Setup/Setup Current Scene For Gameplay")]
    public static void SetupCurrentSceneForGameplay()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        // 检查是否已有GameManager
        if (GameManager.Instance == null)
        {
            // 尝试加载预制体
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabPath}/Managers/GameManager.prefab");
            if (prefab != null)
            {
                PrefabUtility.InstantiatePrefab(prefab);
            }
            else
            {
                // 直接创建
                GameObject gameManager = new GameObject("GameManager");
                gameManager.AddComponent<GameManager>();
                gameManager.AddComponent<SceneLoader>();
            }
        }

        // 设置玩家
        SetupPlayer();

        EditorSceneManager.MarkSceneDirty(currentScene);
        Debug.Log($"[GamePrefabCreator] Scene '{currentScene.name}' setup complete!");
    }

    private static void SetupPlayer()
    {
        // 检查是否已有玩家
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer != null)
        {
            // 确保有PlayerInteraction组件
            if (existingPlayer.GetComponent<PlayerInteraction>() == null)
            {
                existingPlayer.AddComponent<PlayerInteraction>();
            }
            if (existingPlayer.GetComponent<PlayerHealth>() == null)
            {
                existingPlayer.AddComponent<PlayerHealth>();
            }
            return;
        }

        Debug.LogWarning("[GamePrefabCreator] No player found in scene. Please add PlayerArmature from StarterAssets.");
    }
}
