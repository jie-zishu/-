using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameFrameworkManager : MonoBehaviour
{
    public static GameFrameworkManager Instance { get; private set; }

    public enum GameState
    {
        None,
        Loading,
        StartScreen,
        BasicUIScene,
        Playing,
        LevelComplete
    }

    public GameState currentState { get; private set; }
    public int currentLevel { get; private set; }
    public bool[] levelCompleted { get; private set; }

    [SerializeField] private string startScreenScene = "StartScreen";
    [SerializeField] private string basicUIScene = "BasicUIScene";
    [SerializeField] private string bornScene = "BornScene";
    [SerializeField] private string level1Scene = "Level1";
    [SerializeField] private string level2Scene = "Level2";
    [SerializeField] private string level3Scene = "Level3";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeGame()
    {
        currentState = GameState.StartScreen;
        currentLevel = 0;
        levelCompleted = new bool[4];
        for (int i = 0; i < levelCompleted.Length; i++)
        {
            levelCompleted[i] = false;
        }
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
    }

    public void LoadScene(string scenePath)
    {
        SceneManager.LoadScene(NormalizeSceneName(scenePath));
    }

    private string NormalizeSceneName(string scenePath)
    {
        string normalized = scenePath.Replace('\\', '/');
        int slashIndex = normalized.LastIndexOf('/');
        if (slashIndex >= 0)
        {
            normalized = normalized.Substring(slashIndex + 1);
        }

        if (normalized.EndsWith(".unity"))
        {
            normalized = normalized.Substring(0, normalized.Length - 6);
        }

        return normalized;
    }

    // 0=BornScene, 1=Level1, 2=Level2, 3=Level3
    public void LoadLevel(int levelIndex)
    {
        switch (levelIndex)
        {
            case 0:
                LoadScene(bornScene);
                break;
            case 1:
                LoadScene(level1Scene);
                break;
            case 2:
                LoadScene(level2Scene);
                break;
            case 3:
                LoadScene(level3Scene);
                break;
            default:
                Debug.LogWarning($"Invalid level index: {levelIndex}");
                return;
        }

        currentLevel = levelIndex;
        ChangeState(GameState.Playing);
    }

    public void CompleteLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levelCompleted.Length)
        {
            return;
        }

        levelCompleted[levelIndex] = true;

        if (levelIndex >= 3)
        {
            LoadScene(startScreenScene);
            ChangeState(GameState.StartScreen);
        }
        else
        {
            LoadLevel(levelIndex + 1);
        }
    }

    public void GoToStartScreen()
    {
        LoadScene(startScreenScene);
        ChangeState(GameState.StartScreen);
    }

    public void GoToBasicUIScene()
    {
        LoadScene(basicUIScene);
        ChangeState(GameState.BasicUIScene);
    }

    void Start()
    {
    }

    void Update()
    {
    }
}
