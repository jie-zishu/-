using UnityEngine;
using UnityEngine.UI;

public class SimpleUIManager : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button basicUISceneButton;
    [SerializeField] private Button returnToStartScreenButton;

    private void Awake()
    {
        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }
        }

        if (startGameButton == null && basicUISceneButton == null && returnToStartScreenButton == null)
        {
            CreateDefaultUI();
        }
    }

    private void CreateDefaultUI()
    {
        // 创建默认的UI元素
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(canvas.transform);
        buttonContainer.AddComponent<RectTransform>();

        // 创建开始游戏按钮
        GameObject startButton = new GameObject("StartGameButton");
        startButton.transform.SetParent(buttonContainer.transform);
        startButton.AddComponent<RectTransform>();
        startButton.AddComponent<Button>();

        Text startButtonText = startButton.AddComponent<Text>();
        startButtonText.text = "开始游戏";
        startButtonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        startButtonText.fontSize = 24;
        startButtonText.alignment = TextAnchor.MiddleCenter;

        // 创建进入基本UI场景按钮
        GameObject basicUISceneButtonObj = new GameObject("BasicUISceneButton");
        basicUISceneButtonObj.transform.SetParent(buttonContainer.transform);
        basicUISceneButtonObj.AddComponent<RectTransform>();
        basicUISceneButtonObj.AddComponent<Button>();

        Text basicUISceneButtonText = basicUISceneButtonObj.AddComponent<Text>();
        basicUISceneButtonText.text = "进入基本UI场景";
        basicUISceneButtonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        basicUISceneButtonText.fontSize = 24;
        basicUISceneButtonText.alignment = TextAnchor.MiddleCenter;

        // 设置按钮位置
        RectTransform startButtonRect = startButton.GetComponent<RectTransform>();
        startButtonRect.anchoredPosition = new Vector2(0, 100);
        startButtonRect.sizeDelta = new Vector2(200, 50);

        RectTransform basicUISceneButtonRect = basicUISceneButtonObj.GetComponent<RectTransform>();
        basicUISceneButtonRect.anchoredPosition = new Vector2(0, -100);
        basicUISceneButtonRect.sizeDelta = new Vector2(200, 50);

        // 添加按钮点击事件
        startButton.GetComponent<Button>().onClick.AddListener(OnStartGameButton);
        basicUISceneButtonObj.GetComponent<Button>().onClick.AddListener(OnBasicUISceneButton);
    }

    private void OnStartGameButton()
    {
        GameFrameworkManager.Instance.LoadLevel(0);
    }

    private void OnBasicUISceneButton()
    {
        GameFrameworkManager.Instance.LoadScene("BasicUIScene");
        GameFrameworkManager.Instance.ChangeState(GameFrameworkManager.GameState.BasicUIScene);
    }

    public void OnReturnToStartScreen()
    {
        GameFrameworkManager.Instance.LoadScene("StartScreen");
        GameFrameworkManager.Instance.ChangeState(GameFrameworkManager.GameState.StartScreen);
    }
}