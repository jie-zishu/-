using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// UI管理器 - 管理所有UI面板
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameHUDPanel;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private UnityEngine.UI.Slider loadingBar;

    [Header("Events")]
    public UnityEvent OnUIInitialized;

    private Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 注册所有面板
        RegisterPanel("MainMenu", mainMenuPanel);
        RegisterPanel("LevelSelect", levelSelectPanel);
        RegisterPanel("Pause", pausePanel);
        RegisterPanel("GameHUD", gameHUDPanel);
        RegisterPanel("LevelComplete", levelCompletePanel);
        RegisterPanel("GameOver", gameOverPanel);
        RegisterPanel("Loading", loadingScreen);

        if (OnUIInitialized == null) OnUIInitialized = new UnityEvent();
    }

    private void Start()
    {
        // 订阅游戏状态变化
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged.AddListener(OnGameStateChanged);
        }

        // 订阅加载进度
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnLoadingProgress.AddListener(UpdateLoadingProgress);
        }

        OnUIInitialized?.Invoke();
    }

    private void RegisterPanel(string name, GameObject panel)
    {
        if (panel != null)
        {
            panels[name] = panel;
        }
    }

    /// <summary>
    /// 显示指定面板
    /// </summary>
    public void ShowPanel(string panelName)
    {
        if (panels.TryGetValue(panelName, out GameObject panel))
        {
            panel.SetActive(true);
        }
    }

    /// <summary>
    /// 隐藏指定面板
    /// </summary>
    public void HidePanel(string panelName)
    {
        if (panels.TryGetValue(panelName, out GameObject panel))
        {
            panel.SetActive(false);
        }
    }

    /// <summary>
    /// 隐藏所有面板
    /// </summary>
    public void HideAllPanels()
    {
        foreach (var panel in panels.Values)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 游戏状态变化回调
    /// </summary>
    private void OnGameStateChanged(GameState newState)
    {
        HideAllPanels();

        switch (newState)
        {
            case GameState.MainMenu:
                ShowPanel("MainMenu");
                break;
            case GameState.LevelSelect:
                ShowPanel("LevelSelect");
                break;
            case GameState.Playing:
                ShowPanel("GameHUD");
                break;
            case GameState.Paused:
                ShowPanel("GameHUD");
                ShowPanel("Pause");
                break;
            case GameState.LevelComplete:
                ShowPanel("LevelComplete");
                break;
            case GameState.GameOver:
                ShowPanel("GameOver");
                break;
            case GameState.Loading:
                ShowPanel("Loading");
                break;
        }
    }

    /// <summary>
    /// 更新加载进度
    /// </summary>
    private void UpdateLoadingProgress(float progress)
    {
        if (loadingBar != null)
        {
            loadingBar.value = progress;
        }
    }

    /// <summary>
    /// 暂停按钮点击
    /// </summary>
    public void OnPauseButtonClicked()
    {
        GameManager.Instance?.Pause();
    }

    /// <summary>
    /// 继续按钮点击
    /// </summary>
    public void OnResumeButtonClicked()
    {
        GameManager.Instance?.Resume();
    }

    /// <summary>
    /// 返回主菜单按钮点击
    /// </summary>
    public void OnMainMenuButtonClicked()
    {
        GameManager.Instance?.ReturnToMainMenu();
    }

    /// <summary>
    /// 重试按钮点击
    /// </summary>
    public void OnRetryButtonClicked()
    {
        GameManager.Instance?.RestartCurrentLevel();
    }
}
