using UnityEngine;
using UnityEngine.UI;

public class StartScreenController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject developerWordsPanel;
    [SerializeField] private GameObject storyMenuPanel;
    [SerializeField] private GameObject settingWordsPanel;

    [Header("Buttons")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button developerWordsButton;
    [SerializeField] private Button unlockedStoriesButton;
    [SerializeField] private Button backWardsButton;
    [SerializeField] private Button settingWordsButton;

    [Header("Setting Words Buttons")]
    [SerializeField] private Button backMainPanelButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button deleteSettingButton;

    private enum PanelState { MainMenu, DeveloperWords, StoryMenu }
    private PanelState currentState;

    private void Start()
    {
        // Main menu buttons
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGame);

        if (developerWordsButton != null)
            developerWordsButton.onClick.AddListener(() => SwitchTo(PanelState.DeveloperWords));

        if (unlockedStoriesButton != null)
            unlockedStoriesButton.onClick.AddListener(() => SwitchTo(PanelState.StoryMenu));

        if (backWardsButton != null)
            backWardsButton.onClick.AddListener(GoBack);

        // Settings toggle — overlay, doesn't affect other panels
        if (settingWordsButton != null)
            settingWordsButton.onClick.AddListener(ToggleSettings);

        // Settings panel buttons
        if (backMainPanelButton != null)
            backMainPanelButton.onClick.AddListener(BackToMainAndCloseSettings);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitGame);

        if (deleteSettingButton != null)
            deleteSettingButton.onClick.AddListener(CloseSettings);

        SwitchTo(PanelState.MainMenu);
    }

    private void OnStartGame()
    {
        GameFrameworkManager.Instance.LoadLevel(0);
    }

    private void SwitchTo(PanelState state)
    {
        currentState = state;

        mainMenuPanel.SetActive(state == PanelState.MainMenu);
        developerWordsPanel.SetActive(state == PanelState.DeveloperWords);
        storyMenuPanel.SetActive(state == PanelState.StoryMenu);

        backWardsButton.gameObject.SetActive(state != PanelState.MainMenu);
    }

    private void GoBack()
    {
        SwitchTo(PanelState.MainMenu);
    }

    private void ToggleSettings()
    {
        if (settingWordsPanel != null)
            settingWordsPanel.SetActive(!settingWordsPanel.activeSelf);
    }

    private void CloseSettings()
    {
        if (settingWordsPanel != null)
            settingWordsPanel.SetActive(false);
    }

    private void BackToMainAndCloseSettings()
    {
        CloseSettings();
        SwitchTo(PanelState.MainMenu);
    }

    private void OnExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
