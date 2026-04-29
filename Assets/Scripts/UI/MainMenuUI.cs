using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 主菜单UI控制器
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button levelSelectButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;

    private void Awake()
    {
        // 绑定按钮事件
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        if (levelSelectButton != null)
            levelSelectButton.onClick.AddListener(OnLevelSelectClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    /// <summary>
    /// 开始游戏按钮
    /// </summary>
    private void OnStartClicked()
    {
        // 直接开始新游戏（从教程开始）
        GameManager.Instance?.StartLevel(0); // 0 = 教程
    }

    /// <summary>
    /// 关卡选择按钮
    /// </summary>
    private void OnLevelSelectClicked()
    {
        GameManager.Instance?.SetState(GameState.LevelSelect);
        SceneLoader.Instance?.LoadLevelSelect();
    }

    /// <summary>
    /// 设置按钮
    /// </summary>
    private void OnSettingsClicked()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }

    /// <summary>
    /// 退出游戏按钮
    /// </summary>
    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
