using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using StarterAssets;

public class PianoGameManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pianoUI;
    [SerializeField] private GameObject closeButton;

    [Header("Test")]
    [SerializeField] private bool autoOpenOnStart = false;

    [Header("Events")]
    public UnityEvent onPianoClosed;

    [Header("Blur")]
    [SerializeField] private float maxBlurStrength = 2.5f;
    [SerializeField] private float blurTransitionDuration = 0.5f;

    private BlurRendererFeature blurFeature;
    private Coroutine blurCoroutine;
    private bool isPianoOpen;

    private ThirdPersonController playerController;
    private StarterAssetsInputs playerInputs;
    private UnityEngine.InputSystem.PlayerInput playerInputComp;
    private bool savedCursorLocked;
    private bool savedCursorForLook;

    private void Start()
    {
        // Auto-find references if not assigned
        if (pianoUI == null)
            pianoUI = GameObject.Find("InterationPianoGameUI");
        if (closeButton == null && pianoUI != null)
            closeButton = pianoUI.transform.Find("Delete")?.gameObject;

        // Wire close button
        if (closeButton != null)
        {
            var btn = closeButton.GetComponent<UnityEngine.UI.Button>();
            if (btn == null) btn = closeButton.AddComponent<UnityEngine.UI.Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(ClosePiano);
            Debug.Log("[PianoGameManager] Delete button wired. Listeners: " + btn.onClick.GetPersistentEventCount());
        }
        else
        {
            // Fallback: find by name directly
            var fallback = GameObject.Find("Delete");
            if (fallback != null)
            {
                closeButton = fallback;
                var btn = fallback.GetComponent<UnityEngine.UI.Button>();
                if (btn == null) btn = fallback.AddComponent<UnityEngine.UI.Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(ClosePiano);
                Debug.Log("[PianoGameManager] Delete button wired via fallback. Listeners: " + btn.onClick.GetPersistentEventCount());
            }
        }

        FindBlurFeature();
        CachePlayerComponents();

        if (autoOpenOnStart)
            OpenPiano();
        else if (pianoUI != null)
            pianoUI.SetActive(false);
    }

    private void FindBlurFeature()
    {
        blurFeature = BlurRendererFeature.Instance;
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

    public void OpenPiano()
    {
        if (isPianoOpen) return;
        isPianoOpen = true;

        DisablePlayerInput();
        ShowCursor();

        if (pianoUI != null) pianoUI.SetActive(true);
        SetBlur(maxBlurStrength);
    }

    public void FadeInPianoUI()
    {
        if (isPianoOpen) return;
        isPianoOpen = true;

        DisablePlayerInput();
        ShowCursor();

        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        // Fade blur from 0 → maxBlurStrength over 0.5s
        float elapsed = 0f;
        while (elapsed < blurTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / blurTransitionDuration;
            SetBlur(Mathf.Lerp(0f, maxBlurStrength, 1f - (1f - t) * (1f - t)));
            yield return null;
        }
        SetBlur(maxBlurStrength);

        // Show piano UI after blur
        if (pianoUI != null) pianoUI.SetActive(true);
    }

    private void DisablePlayerInput()
    {
        if (playerController != null) playerController.enabled = false;
        if (playerInputs != null)
        {
            savedCursorLocked = playerInputs.cursorLocked;
            savedCursorForLook = playerInputs.cursorInputForLook;
            playerInputs.cursorLocked = false;
            playerInputs.cursorInputForLook = false;
        }
        if (playerInputComp != null) playerInputComp.enabled = false;
    }

    private void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ClosePiano()
    {
        if (!isPianoOpen) return;
        StartCoroutine(ClosePianoRoutine());
    }

    private IEnumerator ClosePianoRoutine()
    {
        isPianoOpen = false;

        // Hide UI immediately
        if (pianoUI != null) pianoUI.SetActive(false);

        // Smoothly fade blur to 0
        yield return FadeBlurToZero();

        // Restore player
        if (playerController != null) playerController.enabled = true;
        if (playerInputs != null)
        {
            playerInputs.cursorLocked = savedCursorLocked;
            playerInputs.cursorInputForLook = savedCursorForLook;
        }
        if (playerInputComp != null) playerInputComp.enabled = true;

        // Hide cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        onPianoClosed?.Invoke();
    }

    private void SetBlur(float value)
    {
        if (blurFeature != null)
            blurFeature.SetBlurStrength(value);
    }

    private IEnumerator FadeBlurToZero()
    {
        if (blurFeature == null) yield break;

        float start = blurFeature.GetBlurStrength();
        float elapsed = 0f;
        while (elapsed < blurTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / blurTransitionDuration;
            float ease = 1f - (1f - t) * (1f - t); // ease-out
            blurFeature.SetBlurStrength(Mathf.Lerp(start, 0f, ease));
            yield return null;
        }
        blurFeature.SetBlurStrength(0f);
    }

    private void Update()
    {
        // ESC also closes
        if (isPianoOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePiano();
        }
    }
}
