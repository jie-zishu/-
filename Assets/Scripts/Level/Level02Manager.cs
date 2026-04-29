using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关卡2管理器
/// 具体小游戏逻辑待实现
/// </summary>
public class Level02Manager : MinigameManager
{
    // 在这里添加关卡2特有的设置

protected override void Awake()
{
    base.Awake();
    levelIndex = 2;
    levelName = "Level 02";
}

    protected override void StartMinigame()
    {
        // TODO: 实现关卡2的小游戏启动逻辑
        Debug.Log("[Level02] Minigame started!");
    }

    protected override bool CheckMinigameComplete()
    {
        // TODO: 实现关卡2的完成条件检测
        return minigameScore >= targetScore;
    }

    // 在这里添加关卡2特有的方法
}
