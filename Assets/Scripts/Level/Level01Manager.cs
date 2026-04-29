using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关卡1管理器
/// 具体小游戏逻辑待实现
/// </summary>
public class Level01Manager : MinigameManager
{
    // 在这里添加关卡1特有的设置

protected override void Awake()
{
    base.Awake();
    levelIndex = 1;
    levelName = "Level 01";
}

    protected override void StartMinigame()
    {
        // TODO: 实现关卡1的小游戏启动逻辑
        Debug.Log("[Level01] Minigame started!");
    }

    protected override bool CheckMinigameComplete()
    {
        // TODO: 实现关卡1的完成条件检测
        return minigameScore >= targetScore;
    }

    // 在这里添加关卡1特有的方法
}
