using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 小游戏管理器基类
/// 关卡1、2继承此类实现具体小游戏逻辑
/// </summary>
public abstract class MinigameManager : LevelManager
{
    [Header("Minigame Settings")]
    [SerializeField] protected int minigameScore = 0;
    [SerializeField] protected int targetScore = 100;
    [SerializeField] protected bool hasTimeLimit = false;
    [SerializeField] protected float timeLimit = 60f;

    protected float currentTime = 0f;
    protected bool isMinigameActive = false;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void InitializeLevel()
    {
        base.InitializeLevel();
        minigameScore = 0;
        currentTime = timeLimit;
        isMinigameActive = true;

        StartMinigame();
    }

    protected override void Update()
    {
        if (!isMinigameActive) return;

        base.Update();

        // 时间限制
        if (hasTimeLimit)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                currentTime = 0;
                OnTimeUp();
            }
        }

        // 检查小游戏是否完成
        if (CheckMinigameComplete())
        {
            OnMinigameComplete();
        }
    }

    /// <summary>
    /// 开始小游戏（子类实现具体逻辑）
    /// </summary>
    protected abstract void StartMinigame();

    /// <summary>
    /// 检查小游戏是否完成（子类实现具体逻辑）
    /// </summary>
    protected abstract bool CheckMinigameComplete();

    /// <summary>
    /// 小游戏完成
    /// </summary>
    protected virtual void OnMinigameComplete()
    {
        isMinigameActive = false;
        CompleteLevel();
    }

    /// <summary>
    /// 增加分数
    /// </summary>
    protected virtual void AddScore(int amount)
    {
        minigameScore += amount;
        Debug.Log($"[Minigame] Score: {minigameScore}/{targetScore}");
    }

    /// <summary>
    /// 获取当前分数
    /// </summary>
    public int GetScore()
    {
        return minigameScore;
    }

    /// <summary>
    /// 获取剩余时间
    /// </summary>
    public float GetRemainingTime()
    {
        return currentTime;
    }

    /// <summary>
    /// 获取进度百分比
    /// </summary>
    public float GetProgress()
    {
        return (float)minigameScore / targetScore;
    }

    protected override bool CheckWinCondition()
    {
        return minigameScore >= targetScore;
    }

    protected override void OnTimeUp()
    {
        if (!hasTimeLimit) return;

        isMinigameActive = false;
        FailLevel();
    }
}
