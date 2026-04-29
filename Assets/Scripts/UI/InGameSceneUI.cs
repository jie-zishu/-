using UnityEngine;
using UnityEngine.UI;

public class InGameSceneUI : MonoBehaviour
{
    private Button basicUISceneButton;
    private Button returnToStartScreenButton;

    private void Start()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);
        foreach (Button button in buttons)
        {
            if (button.name == "BasicUISceneButton")
            {
                basicUISceneButton = button;
            }
            else if (button.name == "ReturnToStartScreenButton")
            {
                returnToStartScreenButton = button;
            }
        }

        if (basicUISceneButton != null)
        {
            basicUISceneButton.onClick.RemoveListener(OnBasicUISceneButton);
            basicUISceneButton.onClick.AddListener(OnBasicUISceneButton);
        }

        if (returnToStartScreenButton != null)
        {
            returnToStartScreenButton.onClick.RemoveListener(OnReturnToStartScreenButton);
            returnToStartScreenButton.onClick.AddListener(OnReturnToStartScreenButton);
        }
    }

    private void OnBasicUISceneButton()
    {
        if (GameFrameworkManager.Instance != null)
        {
            GameFrameworkManager.Instance.GoToBasicUIScene();
        }
    }

    private void OnReturnToStartScreenButton()
    {
        if (GameFrameworkManager.Instance != null)
        {
            GameFrameworkManager.Instance.GoToStartScreen();
        }
    }

    private void OnDestroy()
    {
        if (basicUISceneButton != null)
        {
            basicUISceneButton.onClick.RemoveListener(OnBasicUISceneButton);
        }

        if (returnToStartScreenButton != null)
        {
            returnToStartScreenButton.onClick.RemoveListener(OnReturnToStartScreenButton);
        }
    }
}
