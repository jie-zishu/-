using UnityEngine;
using UnityEngine.UI;

public class BasicUISceneUI : MonoBehaviour
{
    private Button returnToStartScreenButton;

    private void Start()
    {
        // 自动查找返回按钮
        Button[] buttons = FindObjectsOfType<Button>();
        foreach (Button button in buttons)
        {
            if (button.gameObject.name == "ReturnToStartScreenButton")
            {
                returnToStartScreenButton = button;
                break;
            }
        }

        if (returnToStartScreenButton != null)
        {
            returnToStartScreenButton.onClick.AddListener(OnReturnToStartScreen);
        }
    }

    private void OnReturnToStartScreen()
    {
        GameFrameworkManager.Instance.GoToStartScreen();
    }

    private void OnDestroy()
    {
        if (returnToStartScreenButton != null)
        {
            returnToStartScreenButton.onClick.RemoveListener(OnReturnToStartScreen);
        }
    }
}