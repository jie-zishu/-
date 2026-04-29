using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 教程关卡管理器
/// 负责引导玩家学习基础操作
/// </summary>
public class TutorialManager : LevelManager
{
    [Header("Tutorial Steps")]
    [SerializeField] private TutorialStep[] tutorialSteps;
    [SerializeField] private int currentStepIndex = 0;

    [Header("Tutorial UI")]
    [SerializeField] private GameObject tutorialUI;
    [SerializeField] private TMPro.TextMeshProUGUI instructionText;

    [Header("Tutorial Objects")]
    [SerializeField] private GameObject[] tutorialObjects; // 教程中需要交互的对象

    private bool[] stepsCompleted;

    [System.Serializable]
    public struct TutorialStep
    {
        public string instruction;
        public string actionType; // "Move", "Interact", "Collect" 等
        public bool isCompleted;
    }

    protected override void Awake()
    {
        base.Awake();
        levelIndex = 0;
        levelName = "Tutorial";

        stepsCompleted = new bool[tutorialSteps.Length];
    }

    protected override void InitializeLevel()
    {
        base.InitializeLevel();
        currentStepIndex = 0;
        ShowCurrentStep();
    }

    protected override void Update()
    {
        base.Update();

        // 检测当前步骤是否完成
        CheckCurrentStepCompletion();
    }

    /// <summary>
    /// 显示当前教程步骤
    /// </summary>
    private void ShowCurrentStep()
    {
        if (currentStepIndex >= tutorialSteps.Length)
        {
            // 所有步骤完成
            CompleteLevel();
            return;
        }

        if (instructionText != null)
        {
            instructionText.text = tutorialSteps[currentStepIndex].instruction;
        }

        Debug.Log($"[Tutorial] Step {currentStepIndex + 1}: {tutorialSteps[currentStepIndex].instruction}");
    }

    /// <summary>
    /// 检查当前步骤是否完成
    /// </summary>
    private void CheckCurrentStepCompletion()
    {
        if (currentStepIndex >= tutorialSteps.Length) return;

        // 这里可以根据actionType检测不同的完成条件
        // 子类可以重写此方法实现具体逻辑
    }

    /// <summary>
    /// 完成当前步骤，进入下一步
    /// </summary>
    public void CompleteCurrentStep()
    {
        if (currentStepIndex >= tutorialSteps.Length) return;

        stepsCompleted[currentStepIndex] = true;
        tutorialSteps[currentStepIndex].isCompleted = true;

        Debug.Log($"[Tutorial] Step {currentStepIndex + 1} completed!");

        currentStepIndex++;
        ShowCurrentStep();
    }

    /// <summary>
    /// 玩家完成移动教程
    /// </summary>
    public void OnPlayerMoved()
    {
        if (currentStepIndex < tutorialSteps.Length &&
            tutorialSteps[currentStepIndex].actionType == "Move")
        {
            CompleteCurrentStep();
        }
    }

    /// <summary>
    /// 玩家完成交互教程
    /// </summary>
    public void OnPlayerInteracted()
    {
        if (currentStepIndex < tutorialSteps.Length &&
            tutorialSteps[currentStepIndex].actionType == "Interact")
        {
            CompleteCurrentStep();
        }
    }

    protected override bool CheckWinCondition()
    {
        // 所有教程步骤都完成
        foreach (var step in tutorialSteps)
        {
            if (!step.isCompleted) return false;
        }
        return true;
    }
}
