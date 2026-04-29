using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 游戏内HUD控制器
/// 显示玩家状态、关卡信息等
/// </summary>
public class GameHUD : MonoBehaviour
{
    [Header("Health Display")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text healthText;

    [Header("Level Info")]
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private TMP_Text timerText;

    [Header("Boss Health (Level 3)")]
    [SerializeField] private GameObject bossHealthPanel;
    [SerializeField] private Slider bossHealthBar;
    [SerializeField] private TMP_Text bossNameText;

    [Header("Collection Progress (Level 3)")]
    [SerializeField] private Slider collectionBar;
    [SerializeField] private TMP_Text collectionText;

    [Header("Pause Button")]
    [SerializeField] private Button pauseButton;

    private void Awake()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(OnPauseClicked);
    }

    private void OnEnable()
    {
        // 订阅玩家生命值变化
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnHealthChanged.AddListener(UpdateHealthDisplay);
            UpdateHealthDisplay(PlayerHealth.Instance.CurrentHealth);
        }
    }

    private void OnDisable()
    {
        // 取消订阅
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnHealthChanged.RemoveListener(UpdateHealthDisplay);
        }
    }

    /// <summary>
    /// 更新生命值显示
    /// </summary>
    private void UpdateHealthDisplay(int currentHealth)
    {
        if (healthBar != null)
        {
            healthBar.value = PlayerHealth.Instance.GetHealthPercentage();
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{PlayerHealth.Instance.MaxHealth}";
        }
    }

    /// <summary>
    /// 设置关卡名称
    /// </summary>
    public void SetLevelName(string name)
    {
        if (levelNameText != null)
        {
            levelNameText.text = name;
        }
    }

    /// <summary>
    /// 更新计时器显示
    /// </summary>
    public void UpdateTimer(float remainingTime)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    /// <summary>
    /// 显示Boss血条
    /// </summary>
    public void ShowBossHealth(string bossName, int maxHealth)
    {
        if (bossHealthPanel != null)
        {
            bossHealthPanel.SetActive(true);
        }

        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }

        if (bossHealthBar != null)
        {
            bossHealthBar.maxValue = maxHealth;
            bossHealthBar.value = maxHealth;
        }
    }

    /// <summary>
    /// 更新Boss血条
    /// </summary>
    public void UpdateBossHealth(int currentHealth, int maxHealth)
    {
        if (bossHealthBar != null)
        {
            bossHealthBar.value = currentHealth;
        }
    }

    /// <summary>
    /// 隐藏Boss血条
    /// </summary>
    public void HideBossHealth()
    {
        if (bossHealthPanel != null)
        {
            bossHealthPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 更新收集进度
    /// </summary>
    public void UpdateCollectionProgress(int collected, int total)
    {
        if (collectionBar != null)
        {
            collectionBar.value = (float)collected / total;
        }

        if (collectionText != null)
        {
            collectionText.text = $"{collected}/{total}";
        }
    }

    /// <summary>
    /// 显示收集进度条
    /// </summary>
    public void ShowCollectionProgress()
    {
        if (collectionBar != null)
        {
            collectionBar.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 隐藏收集进度条
    /// </summary>
    public void HideCollectionProgress()
    {
        if (collectionBar != null)
        {
            collectionBar.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 暂停按钮点击
    /// </summary>
    private void OnPauseClicked()
    {
        GameManager.Instance?.Pause();
    }
}
