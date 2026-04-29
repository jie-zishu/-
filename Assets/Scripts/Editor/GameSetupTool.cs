using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;

/// <summary>
/// 游戏场景设置工具
/// 在Unity菜单 Tools > Game Setup 中使用
/// </summary>
public class GameSetupTool : EditorWindow
{
    [MenuItem("Tools/Game Setup/Create All Scenes")]
    public static void CreateAllScenes()
    {
        // 创建所有场景
        CreateMainMenuScene();
        CreateLevelSelectScene();
        CreateTutorialScene();
        CreateGameLevelScene("Level01", "Level01", 1);
        CreateGameLevelScene("Level02", "Level02", 2);
        CreateGameLevelScene("Level03", "Level03", 3);

        Debug.Log("[GameSetupTool] All scenes created! Check the Scenes folder.");
    }

    [MenuItem("Tools/Game Setup/Create Main Menu Scene")]
    public static void CreateMainMenuScene()
    {
        string path = "Assets/Scenes/MainMenu.unity";
        CreateSceneWithUI(path, "MainMenu");
    }

    [MenuItem("Tools/Game Setup/Create Level Select Scene")]
    public static void CreateLevelSelectScene()
    {
        string path = "Assets/Scenes/LevelSelect.unity";
        CreateSceneWithUI(path, "LevelSelect");
    }

    [MenuItem("Tools/Game Setup/Create Tutorial Scene")]
    public static void CreateTutorialScene()
    {
        string path = "Assets/Scenes/Levels/Tutorial.unity";
        CreateGameLevelScene(path, "Tutorial", 0);
    }

    [MenuItem("Tools/Game Setup/Create Level01 Scene")]
    public static void CreateLevel01Scene()
    {
        string path = "Assets/Scenes/Levels/Level01.unity";
        CreateGameLevelScene(path, "Level01", 1);
    }

    [MenuItem("Tools/Game Setup/Create Level02 Scene")]
    public static void CreateLevel02Scene()
    {
        string path = "Assets/Scenes/Levels/Level02.unity";
        CreateGameLevelScene(path, "Level02", 2);
    }

    [MenuItem("Tools/Game Setup/Create Level03 Scene")]
    public static void CreateLevel03Scene()
    {
        string path = "Assets/Scenes/Levels/Level03.unity";
        CreateGameLevelScene(path, "Level03", 3);
    }

    private static void CreateSceneWithUI(string path, string sceneName)
    {
        // 确保目录存在
        Directory.CreateDirectory(Path.GetDirectoryName(path));

        // 创建新场景
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 创建主相机
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            mainCamera = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }
        mainCamera.transform.position = new Vector3(0, 1, -10);
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.2f);

        // 创建UI Canvas
        GameObject canvasObj = CreateUICanvas();

        // 创建简单UI面板
        CreateSimpleUIPanel(canvasObj, sceneName);

        // 保存场景
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"[GameSetupTool] Created scene: {path}");
    }

    private static void CreateGameLevelScene(string path, string sceneName, int levelIndex)
    {
        // 确保目录存在
        Directory.CreateDirectory(Path.GetDirectoryName(path));

        // 创建新场景
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 创建地面
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(5, 1, 5);
        ground.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f);

        // 创建一些墙壁/障碍物
        CreateSimpleEnvironment();

        // 创建玩家出生点标记
        GameObject spawnPoint = new GameObject("PlayerSpawnPoint");
        spawnPoint.transform.position = new Vector3(0, 0, 0);

        // 创建红色可交互立方体（关卡终点）
        CreateNextLevelTrigger(levelIndex);

        // 添加灯光
        Light mainLight = Object.FindObjectOfType<Light>();
        if (mainLight == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            mainLight = lightObj.AddComponent<Light>();
            mainLight.type = LightType.Directional;
        }
        mainLight.transform.rotation = Quaternion.Euler(50, -30, 0);
        mainLight.intensity = 1;

        // 保存场景
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"[GameSetupTool] Created game level: {path}");
    }

    private static void CreateSimpleEnvironment()
    {
        // 创建几面简单的墙
        for (int i = 0; i < 4; i++)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = $"Wall_{i}";
            wall.transform.localScale = new Vector3(10, 2, 0.5f);
            wall.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.6f);

            switch (i)
            {
                case 0:
                    wall.transform.position = new Vector3(0, 1, 10);
                    break;
                case 1:
                    wall.transform.position = new Vector3(0, 1, -10);
                    break;
                case 2:
                    wall.transform.position = new Vector3(10, 1, 0);
                    wall.transform.rotation = Quaternion.Euler(0, 90, 0);
                    break;
                case 3:
                    wall.transform.position = new Vector3(-10, 1, 0);
                    wall.transform.rotation = Quaternion.Euler(0, 90, 0);
                    break;
            }
        }

        // 添加几个简单的障碍物
        for (int i = 0; i < 5; i++)
        {
            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = $"Obstacle_{i}";
            obstacle.transform.position = new Vector3(
                Random.Range(-8, 8),
                0.5f,
                Random.Range(-8, 8)
            );
            obstacle.transform.localScale = new Vector3(
                Random.Range(1, 3),
                Random.Range(1, 3),
                Random.Range(1, 3)
            );
            obstacle.GetComponent<Renderer>().material.color = new Color(0.4f, 0.4f, 0.5f);
        }
    }

    private static void CreateNextLevelTrigger(int levelIndex)
    {
        GameObject trigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trigger.name = "NextLevelTrigger";
        trigger.transform.position = new Vector3(5, 0.5f, 5);
        trigger.transform.localScale = new Vector3(1, 1, 1);
        trigger.GetComponent<Renderer>().material.color = Color.red;

        // 添加BoxCollider作为触发器
        BoxCollider collider = trigger.AddComponent<BoxCollider>();
        collider.isTrigger = true;

        // 设置下一关索引
        Debug.Log($"[GameSetupTool] Created NextLevelTrigger for level {levelIndex}");
    }

    private static GameObject CreateUICanvas()
    {
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        return canvasObj;
    }

    private static void CreateSimpleUIPanel(GameObject canvas, string sceneName)
    {
        // 创建面板
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        // 添加背景
        UnityEngine.UI.Image bgImage = panel.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

        // 创建标题文字
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.sizeDelta = new Vector2(600, 100);
        titleRect.anchoredPosition = Vector2.zero;

        UnityEngine.UI.Text titleText = titleObj.AddComponent<UnityEngine.UI.Text>();
        titleText.text = sceneName;
        titleText.fontStyle = FontStyle.Bold;
        titleText.fontSize = 48;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;

        // 创建按钮
        CreateButton(panel, "StartButton", "Start Game", new Vector2(0, -100));
        CreateButton(panel, "QuitButton", "Quit", new Vector2(0, -200));
    }

    private static void CreateButton(GameObject parent, string buttonName, string buttonText, Vector2 position)
    {
        GameObject buttonObj = new GameObject(buttonName);
        buttonObj.transform.SetParent(parent.transform, false);

        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(300, 60);
        rectTransform.anchoredPosition = position;

        UnityEngine.UI.Image buttonImage = buttonObj.AddComponent<UnityEngine.UI.Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.8f);

        UnityEngine.UI.Button button = buttonObj.AddComponent<UnityEngine.UI.Button>();

        // 创建按钮文字
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
        text.text = buttonText;
        text.fontStyle = FontStyle.Bold;
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
    }
}
