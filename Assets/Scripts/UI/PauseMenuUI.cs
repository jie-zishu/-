using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 暂停菜单UI控制器
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button settingsButton;

    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;

    private void Awake()
    {
        // 绑定按钮事件
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);
    }

    private void OnEnable()
    {
        // 暂停菜单显示时，确保游戏暂停
        Time.timeScale = 0f;
    }

    private void OnDisable()
    {
        // 暂停菜单隐藏时，恢复游戏
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // 按ESC键切换暂停状态
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.CurrentState == GameState.Paused)
                {
                    OnResumeClicked();
                }
                else if (GameManager.Instance.CurrentState == GameState.Playing)
                {
                    GameManager.Instance.Pause();
                }
            }
        }
    }

    /// <summary>
    /// 继续游戏
    /// </summary>
    private void OnResumeClicked()
    {
        GameManager.Instance?.Resume();
    }

    /// <summary>
    /// 重新开始
    /// </summary>
    private void OnRestartClicked()
    {
        GameManager.Instance?.RestartCurrentLevel();
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    private void OnMainMenuClicked()
    {
        GameManager.Instance?.ReturnToMainMenu();
    }

    /// <summary>
    /// 设置
    /// </summary>
    private void OnSettingsClicked()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }
}
