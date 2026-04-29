using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 关卡选择UI控制器
/// </summary>
public class LevelSelectUI : MonoBehaviour
{
    [Header("Level Buttons")]
    [SerializeField] private Button tutorialButton;
    [SerializeField] private Button level1Button;
    [SerializeField] private Button level2Button;
    [SerializeField] private Button level3Button;

    [Header("Lock Icons")]
    [SerializeField] private GameObject level1Lock;
    [SerializeField] private GameObject level2Lock;
    [SerializeField] private GameObject level3Lock;

    [Header("Navigation")]
    [SerializeField] private Button backButton;

    [Header("Level Preview")]
    [SerializeField] private Image levelPreviewImage;
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private TMP_Text levelDescriptionText;

    private void Awake()
    {
        // 绑定按钮事件
        if (tutorialButton != null)
            tutorialButton.onClick.AddListener(() => OnLevelSelected(0));

        if (level1Button != null)
            level1Button.onClick.AddListener(() => OnLevelSelected(1));

        if (level2Button != null)
            level2Button.onClick.AddListener(() => OnLevelSelected(2));

        if (level3Button != null)
            level3Button.onClick.AddListener(() => OnLevelSelected(3));

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
    }

    private void OnEnable()
    {
        UpdateLevelButtons();
    }

    /// <summary>
    /// 更新关卡按钮状态（锁定/解锁）
    /// </summary>
    private void UpdateLevelButtons()
    {
        if (GameManager.Instance == null) return;

        bool[] unlocked = GameManager.Instance.LevelUnlocked;

        // 更新锁定图标显示
        if (level1Lock != null)
            level1Lock.SetActive(!unlocked[1]);

        if (level2Lock != null)
            level2Lock.SetActive(!unlocked[2]);

        if (level3Lock != null)
            level3Lock.SetActive(!unlocked[3]);

        // 更新按钮可交互状态
        if (level1Button != null)
            level1Button.interactable = unlocked[1];

        if (level2Button != null)
            level2Button.interactable = unlocked[2];

        if (level3Button != null)
            level3Button.interactable = unlocked[3];
    }

    /// <summary>
    /// 关卡选中
    /// </summary>
    private void OnLevelSelected(int levelIndex)
    {
        Debug.Log($"[LevelSelect] Level {levelIndex} selected");
        GameManager.Instance?.StartLevel(levelIndex);
    }

    /// <summary>
    /// 返回按钮
    /// </summary>
    private void OnBackClicked()
    {
        GameManager.Instance?.ReturnToMainMenu();
    }

    /// <summary>
    /// 显示关卡预览
    /// </summary>
    public void ShowLevelPreview(int levelIndex)
    {
        // 可以在这里更新关卡预览图片和描述
        string levelName = levelIndex switch
        {
            0 => "教程关卡",
            1 => "第一关",
            2 => "第二关",
            3 => "第三关",
            _ => "未知关卡"
        };

        if (levelNameText != null)
            levelNameText.text = levelName;
    }
}
