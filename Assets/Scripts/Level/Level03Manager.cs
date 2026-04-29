using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关卡3管理器 - 收集物品 + Boss战
/// 两阶段关卡：先收集物品，然后与Boss"迷茫"战斗
/// </summary>
public class Level03Manager : LevelManager
{
    public enum Phase
    {
        Collection,     // 收集物品阶段
        BossFight       // Boss战斗阶段
    }

    [Header("Phase Settings")]
    [SerializeField] private Phase currentPhase = Phase.Collection;

    [Header("Collection Phase")]
    [SerializeField] private int totalItemsToCollect = 5;
    [SerializeField] private int itemsCollected = 0;
    [SerializeField] private GameObject[] collectibleItems;

    [Header("Boss Phase")]
    [SerializeField] private BossConfusion boss;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private GameObject bossHealthBar;

    [Header("Phase Transitions")]
    [SerializeField] private float phaseTransitionDelay = 2f;
    [SerializeField] private Animator phaseTransitionAnimator;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent<Phase> OnPhaseChanged;

    protected override void Awake()
    {
        base.Awake();
        levelIndex = 3;
        levelName = "Level 03";

        if (OnPhaseChanged == null)
            OnPhaseChanged = new UnityEngine.Events.UnityEvent<Phase>();
    }

    protected override void InitializeLevel()
    {
        base.InitializeLevel();
        currentPhase = Phase.Collection;
        itemsCollected = 0;

        // 初始化收集品
        InitializeCollectibles();

        // 隐藏Boss相关UI
        if (bossHealthBar != null)
            bossHealthBar.SetActive(false);

        Debug.Log("[Level03] Started in Collection Phase");
    }

    protected override void Update()
    {
        base.Update();

        switch (currentPhase)
        {
            case Phase.Collection:
                UpdateCollectionPhase();
                break;
            case Phase.BossFight:
                UpdateBossPhase();
                break;
        }
    }

    #region Collection Phase

    private void InitializeCollectibles()
    {
        foreach (var item in collectibleItems)
        {
            if (item != null)
                item.SetActive(true);
        }
    }

    private void UpdateCollectionPhase()
    {
        // 收集阶段的更新逻辑
    }

    /// <summary>
    /// 玩家收集到物品时调用
    /// </summary>
    public void OnItemCollected()
    {
        if (currentPhase != Phase.Collection) return;

        itemsCollected++;
        Debug.Log($"[Level03] Items collected: {itemsCollected}/{totalItemsToCollect}");

        if (itemsCollected >= totalItemsToCollect)
        {
            OnAllItemsCollected();
        }
    }

    private void OnAllItemsCollected()
    {
        Debug.Log("[Level03] All items collected! Transitioning to Boss Phase...");
        StartCoroutine(TransitionToBossPhase());
    }

    #endregion

    #region Boss Phase

    private IEnumerator TransitionToBossPhase()
    {
        // 过渡动画
        if (phaseTransitionAnimator != null)
        {
            phaseTransitionAnimator.SetTrigger("PhaseTransition");
        }

        yield return new WaitForSeconds(phaseTransitionDelay);

        StartBossFight();
    }

    private void StartBossFight()
    {
        currentPhase = Phase.BossFight;
        OnPhaseChanged?.Invoke(currentPhase);

        // 显示Boss血条
        if (bossHealthBar != null)
            bossHealthBar.SetActive(true);

        // 生成Boss
        if (boss != null)
        {
            boss.gameObject.SetActive(true);
            boss.StartBattle();
        }

        Debug.Log("[Level03] Boss Fight Started!");
    }

    private void UpdateBossPhase()
    {
        // Boss战斗阶段的更新逻辑
        // 可以在这里检测Boss状态
    }

    /// <summary>
    /// Boss被击败时调用
    /// </summary>
    public void OnBossDefeated()
    {
        if (currentPhase != Phase.BossFight) return;

        Debug.Log("[Level03] Boss defeated! Level complete!");
        CompleteLevel();
    }

    #endregion

    protected override bool CheckWinCondition()
    {
        // 胜利条件：击败Boss
        return currentPhase == Phase.BossFight && boss != null && boss.IsDefeated;
    }

    /// <summary>
    /// 获取当前阶段
    /// </summary>
    public Phase GetCurrentPhase()
    {
        return currentPhase;
    }

    /// <summary>
    /// 获取收集进度
    /// </summary>
    public float GetCollectionProgress()
    {
        return (float)itemsCollected / totalItemsToCollect;
    }

    public override void ResetLevel()
    {
        itemsCollected = 0;
        currentPhase = Phase.Collection;

        if (boss != null)
            boss.ResetBoss();

        base.ResetLevel();
    }
}
