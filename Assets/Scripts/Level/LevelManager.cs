using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 关卡管理器基类
/// 所有具体关卡管理器都应继承此类
/// </summary>
public abstract class LevelManager : MonoBehaviour
{
    [Header("Level Info")]
    [SerializeField] protected int levelIndex = 1;
    public int LevelIndex => levelIndex;

    [SerializeField] protected string levelName = "Level";
    public string LevelName => levelName;

    [Header("Level Settings")]
    [SerializeField] protected float levelTimeLimit = 0f; // 0表示无时间限制

    [Header("Events")]
    public UnityEvent OnLevelStart;
    public UnityEvent OnLevelComplete;
    public UnityEvent OnLevelFailed;

    protected bool isLevelActive = false;
    protected float levelTimer = 0f;

    protected virtual void Awake()
    {
        if (OnLevelStart == null) OnLevelStart = new UnityEvent();
        if (OnLevelComplete == null) OnLevelComplete = new UnityEvent();
        if (OnLevelFailed == null) OnLevelFailed = new UnityEvent();
    }

    protected virtual void Start()
    {
        InitializeLevel();
    }

    protected virtual void Update()
    {
        if (!isLevelActive) return;

        // 更新计时器
        if (levelTimeLimit > 0)
        {
            levelTimer += Time.deltaTime;
            if (levelTimer >= levelTimeLimit)
            {
                OnTimeUp();
            }
        }
    }

    /// <summary>
    /// 初始化关卡
    /// </summary>
    protected virtual void InitializeLevel()
    {
        isLevelActive = true;
        levelTimer = 0f;

        // 注册到GameManager
        if (GameManager.Instance != null)
        {
            // GameManager会管理关卡状态
        }

        OnLevelStart?.Invoke();
        Debug.Log($"[LevelManager] {levelName} initialized");
    }

    /// <summary>
    /// 检查胜利条件（子类必须实现）
    /// </summary>
    protected abstract bool CheckWinCondition();

    /// <summary>
    /// 关卡完成
    /// </summary>
    protected virtual void CompleteLevel()
    {
        if (!isLevelActive) return;

        isLevelActive = false;
        OnLevelComplete?.Invoke();

        // 通知GameManager
        GameManager.Instance?.CompleteLevel();

        Debug.Log($"[LevelManager] {levelName} completed!");
    }

    /// <summary>
    /// 关卡失败
    /// </summary>
    protected virtual void FailLevel()
    {
        if (!isLevelActive) return;

        isLevelActive = false;
        OnLevelFailed?.Invoke();

        // 通知GameManager
        GameManager.Instance?.GameOver();

        Debug.Log($"[LevelManager] {levelName} failed!");
    }

    /// <summary>
    /// 时间耗尽
    /// </summary>
    protected virtual void OnTimeUp()
    {
        FailLevel();
    }

    /// <summary>
    /// 暂停关卡
    /// </summary>
    public virtual void PauseLevel()
    {
        isLevelActive = false;
    }

    /// <summary>
    /// 继续关卡
    /// </summary>
    public virtual void ResumeLevel()
    {
        isLevelActive = true;
    }

    /// <summary>
    /// 重置关卡
    /// </summary>
    public virtual void ResetLevel()
    {
        isLevelActive = false;
        levelTimer = 0f;
        InitializeLevel();
    }
}
