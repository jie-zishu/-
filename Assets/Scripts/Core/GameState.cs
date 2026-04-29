using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏状态枚举
/// </summary>
public enum GameState
{
    None,           // 无状态
    Loading,        // 加载中
    MainMenu,       // 主菜单
    LevelSelect,    // 关卡选择
    Playing,        // 游戏进行中
    Paused,         // 暂停
    LevelComplete,  // 关卡完成
    GameOver        // 游戏结束
}
