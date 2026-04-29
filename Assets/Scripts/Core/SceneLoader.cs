using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// 场景加载器 - 异步加载场景
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string levelSelectSceneName = "LevelSelect";
    [SerializeField] private string levelScenePrefix = "Level0";

    [Header("Loading UI (Optional)")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider progressBar;

    [Header("Events")]
    public UnityEvent<float> OnLoadingProgress;
    public UnityEvent OnLoadingComplete;

    private bool isLoading = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (OnLoadingProgress == null) OnLoadingProgress = new UnityEvent<float>();
        if (OnLoadingComplete == null) OnLoadingComplete = new UnityEvent();
    }

    /// <summary>
    /// 加载主菜单场景
    /// </summary>
    public void LoadMainMenu()
    {
        LoadSceneAsync(mainMenuSceneName);
    }

    /// <summary>
    /// 加载关卡选择场景
    /// </summary>
    public void LoadLevelSelect()
    {
        LoadSceneAsync(levelSelectSceneName);
    }

    /// <summary>
    /// 加载指定关卡
    /// </summary>
    /// <param name="levelIndex">关卡索引 (1-3)</param>
    public void LoadLevel(int levelIndex)
    {
        string sceneName = levelIndex switch
        {
            0 => "Tutorial",
            1 => "Level01",
            2 => "Level02",
            3 => "Level03",
            _ => "Level01"
        };

        LoadSceneAsync(sceneName, true);
    }

    /// <summary>
    /// 加载教程关卡
    /// </summary>
    public void LoadTutorial()
    {
        LoadSceneAsync("Tutorial", true);
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="sceneName">场景名称</param>
    /// <param name="isGameLevel">是否为游戏关卡场景</param>
    public void LoadSceneAsync(string sceneName, bool isGameLevel = false)
    {
        if (isLoading)
        {
            Debug.LogWarning("[SceneLoader] Already loading a scene!");
            return;
        }

        StartCoroutine(LoadSceneCoroutine(sceneName, isGameLevel));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, bool isGameLevel)
    {
        isLoading = true;

        // 显示加载界面
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }

        // 开始异步加载
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        if (asyncLoad == null)
        {
            Debug.LogError($"[SceneLoader] Failed to load scene: {sceneName}");
            isLoading = false;
            yield break;
        }

        // 不允许场景自动激活（可以添加延迟）
        asyncLoad.allowSceneActivation = false;

        // 更新加载进度
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            if (progressBar != null)
            {
                progressBar.value = progress;
            }

            OnLoadingProgress?.Invoke(progress);

            // 当加载接近完成时
            if (asyncLoad.progress >= 0.9f)
            {
                // 可以在这里添加延迟或等待用户输入
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        // 加载完成
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }

        isLoading = false;
        OnLoadingComplete?.Invoke();

        // 如果是游戏关卡，更新游戏状态
        if (isGameLevel)
        {
            GameManager.Instance?.SetState(GameState.Playing);
        }
        else if (sceneName == mainMenuSceneName)
        {
            GameManager.Instance?.SetState(GameState.MainMenu);
        }
        else if (sceneName == levelSelectSceneName)
        {
            GameManager.Instance?.SetState(GameState.LevelSelect);
        }

        Debug.Log($"[SceneLoader] Scene loaded: {sceneName}");
    }

    /// <summary>
    /// 重新加载当前场景
    /// </summary>
    public void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        LoadSceneAsync(currentScene.name);
    }

    /// <summary>
    /// 获取当前场景名称
    /// </summary>
    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
}
