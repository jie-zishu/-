using UnityEngine;
using UnityEngine.UI;
using StarterAssets;

public class InGameSettingsPanel : MonoBehaviour
{
    [SerializeField] private GameObject settingWordsPanel;
    [SerializeField] private Button settingWordsButton;
    [SerializeField] private Button backMainPanelButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button deleteButton;

    private ThirdPersonController playerController;
    private StarterAssetsInputs playerInputs;
    private UnityEngine.InputSystem.PlayerInput playerInputComp;
    private bool savedCursorLocked;
    private bool savedCursorForLook;
    private bool savedCursorVisible;
    private CursorLockMode savedLockMode;

    private void Start()
    {
        if (settingWordsButton != null)
            settingWordsButton.onClick.AddListener(TogglePanel);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(ClosePanel);

        if (backMainPanelButton != null)
            backMainPanelButton.onClick.AddListener(GoToMainMenu);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        if (settingWordsPanel != null)
            settingWordsPanel.SetActive(false);

        CachePlayerComponents();
    }

    private void CachePlayerComponents()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player != null)
        {
            playerController = player.GetComponentInChildren<ThirdPersonController>();
            playerInputs = player.GetComponentInChildren<StarterAssetsInputs>();
            playerInputComp = player.GetComponentInChildren<UnityEngine.InputSystem.PlayerInput>();
        }
    }

    private void Update()
    {
        // ESC toggles the panel
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePanel();
        }
    }

    private void TogglePanel()
    {
        if (settingWordsPanel == null) return;

        bool willOpen = !settingWordsPanel.activeSelf;
        settingWordsPanel.SetActive(willOpen);

        if (willOpen)
            OpenPanelState();
        else
            ClosePanelState();
    }

    private void ClosePanel()
    {
        if (settingWordsPanel != null)
            settingWordsPanel.SetActive(false);
        ClosePanelState();
    }

    private void OpenPanelState()
    {
        // Save cursor state before changing
        savedCursorVisible = Cursor.visible;
        savedLockMode = Cursor.lockState;

        // Freeze player
        if (playerController != null) playerController.enabled = false;
        if (playerInputs != null)
        {
            savedCursorLocked = playerInputs.cursorLocked;
            savedCursorForLook = playerInputs.cursorInputForLook;
            playerInputs.cursorLocked = false;
            playerInputs.cursorInputForLook = false;
        }
        if (playerInputComp != null) playerInputComp.enabled = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void ClosePanelState()
    {
        // Restore player
        if (playerController != null) playerController.enabled = true;
        if (playerInputs != null)
        {
            playerInputs.cursorLocked = savedCursorLocked;
            playerInputs.cursorInputForLook = savedCursorForLook;
        }
        if (playerInputComp != null) playerInputComp.enabled = true;

        // Restore previous cursor state (may be "unlocked for dialogue")
        Cursor.visible = savedCursorVisible;
        Cursor.lockState = savedLockMode;
    }

    private void GoToMainMenu()
    {
        ClosePanel();
        // Unlock cursor before leaving — StartScreen needs it
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartScreen");
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
