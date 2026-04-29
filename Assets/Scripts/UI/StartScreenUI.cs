using UnityEngine;
using UnityEngine.UI;

public class StartScreenUI : MonoBehaviour
{
    private Button startGameButton;
    private Button basicUISceneButton;

    private void Start()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);
        foreach (Button button in buttons)
        {
            if (button.name == "StartGameButton")
            {
                startGameButton = button;
            }
            else if (button.name == "BasicUISceneButton")
            {
                basicUISceneButton = button;
            }
        }

        if (startGameButton != null)
        {
            startGameButton.onClick.RemoveListener(OnStartGameButton);
            startGameButton.onClick.AddListener(OnStartGameButton);
        }

        if (basicUISceneButton != null)
        {
            basicUISceneButton.onClick.RemoveListener(OnBasicUISceneButton);
            basicUISceneButton.onClick.AddListener(OnBasicUISceneButton);
        }
    }

    private void OnStartGameButton()
    {
        if (GameFrameworkManager.Instance != null)
        {
            GameFrameworkManager.Instance.LoadLevel(0);
        }
    }

    private void OnBasicUISceneButton()
    {
        if (GameFrameworkManager.Instance != null)
        {
            GameFrameworkManager.Instance.GoToBasicUIScene();
        }
    }

    private void OnDestroy()
    {
        if (startGameButton != null)
        {
            startGameButton.onClick.RemoveListener(OnStartGameButton);
        }

        if (basicUISceneButton != null)
        {
            basicUISceneButton.onClick.RemoveListener(OnBasicUISceneButton);
        }
    }
}
