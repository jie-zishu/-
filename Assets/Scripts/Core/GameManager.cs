using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏总管理器 - 单例模式
/// 负责管理全局游戏状态、协调各个子系统、持久化跨场景数据
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.None;
    public GameState CurrentState => currentState;

    [Header("Level Info")]
    [SerializeField] private int currentLevelIndex = 0;
    public int CurrentLevelIndex => currentLevelIndex;

    // 关卡解锁状态（关卡1-3，索引从1开始）
    private bool[] levelUnlocked = new bool[4] { true, false, false, false };
    public bool[] LevelUnlocked => levelUnlocked;

    // 事件
    public UnityEvent<GameState> OnStateChanged;
    public UnityEvent<int> OnLevelUnlocked;

    private void Awake()
    {
        // 单例模式
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 初始化事件
        if (OnStateChanged == null) OnStateChanged = new UnityEvent<GameState>();
        if (OnLevelUnlocked == null) OnLevelUnlocked = new UnityEvent<int>();
    }

    private void Start()
    {
        SetState(GameState.MainMenu);
    }

    /// <summary>
    /// 设置游戏状态
    /// </summary>
    public void SetState(GameState newState)
    {
        if (currentState == newState) return;

        GameState previousState = currentState;
        currentState = newState;

        // 状态切换逻辑
        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.LevelComplete:
                // 解锁下一关
                UnlockNextLevel();
                break;
        }

        OnStateChanged?.Invoke(newState);
        Debug.Log($"[GameManager] State changed: {previousState} -> {newState}");
    }

    /// <summary>
    /// 开始指定关卡
    /// </summary>
    public void StartLevel(int levelIndex)
    {
        if (levelIndex < 1 || levelIndex > 3)
        {
            Debug.LogError($"[GameManager] Invalid level index: {levelIndex}");
            return;
        }

        if (!levelUnlocked[levelIndex])
        {
            Debug.LogWarning($"[GameManager] Level {levelIndex} is locked!");
            return;
        }

        currentLevelIndex = levelIndex;
        SetState(GameState.Loading);
        SceneLoader.Instance?.LoadLevel(levelIndex);
    }

    /// <summary>
    /// 关卡完成
    /// </summary>
    public void CompleteLevel()
    {
        SetState(GameState.LevelComplete);
    }

    /// <summary>
    /// 游戏结束（失败）
    /// </summary>
    public void GameOver()
    {
        SetState(GameState.GameOver);
    }

    /// <summary>
    /// 解锁下一关
    /// </summary>
    private void UnlockNextLevel()
    {
        int nextLevel = currentLevelIndex + 1;
        if (nextLevel <= 3 && !levelUnlocked[nextLevel])
        {
            levelUnlocked[nextLevel] = true;
            OnLevelUnlocked?.Invoke(nextLevel);
            Debug.Log($"[GameManager] Level {nextLevel} unlocked!");
        }
    }

    /// <summary>
    /// 检查关卡是否解锁
    /// </summary>
    public bool IsLevelUnlocked(int levelIndex)
    {
        if (levelIndex < 1 || levelIndex > 3) return false;
        return levelUnlocked[levelIndex];
    }

    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void Pause()
    {
        if (currentState == GameState.Playing)
        {
            SetState(GameState.Paused);
        }
    }

    /// <summary>
    /// 继续游戏
    /// </summary>
    public void Resume()
    {
        if (currentState == GameState.Paused)
        {
            SetState(GameState.Playing);
        }
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void ReturnToMainMenu()
    {
        SetState(GameState.Loading);
        SceneLoader.Instance?.LoadMainMenu();
    }

    /// <summary>
    /// 重新开始当前关卡
    /// </summary>
    public void RestartCurrentLevel()
    {
        SetState(GameState.Loading);
        SceneLoader.Instance?.LoadLevel(currentLevelIndex);
    }
}
