using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[Serializable]
public class SceneDialogueConfig
{
    public string sceneName;
    [TextArea(2, 6)] public List<string> lines = new List<string>();
    public bool autoStart = true;
}

[DisallowMultipleComponent]
public class SceneIntroDialogueController : MonoBehaviour
{
    public UnityEvent onDialogueEnd;

    [Header("Scene Dialogue Mapping")]
    [SerializeField] private List<SceneDialogueConfig> sceneDialogues = new List<SceneDialogueConfig>();
    [TextArea(2, 6)] [SerializeField] private List<string> defaultLines = new List<string>();

    [Header("Click Behavior")]
    [SerializeField] private bool consumeAllClicks = true;

    public bool IsDialogueActive() { return dialogueActive; }

    [Header("Dialogue Panel Style")]
    [SerializeField] private float panelHeight = 180f;
    [SerializeField] private float panelBottomMargin = 24f;
    [SerializeField] private Vector2 panelPadding = new Vector2(28f, 20f);
    [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.72f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private int fontSize = 28;

    private const string DialogueRootName = "SceneIntroDialogueUI";
    private const string ClickBlockerName = "ClickBlocker";
    private const string PanelName = "DialoguePanel";
    private const string TextName = "DialogueText";

    private GameObject dialogueRoot;
    private Image clickBlockerImage;
    private Button clickBlockerButton;
    private Image panelImage;
    private Text dialogueText;

    private bool dialogueActive;
    private int currentLineIndex;
    private List<string> activeLines = new List<string>();

    private bool previousCursorVisible;
    private CursorLockMode previousCursorLockMode;
    private readonly List<BehaviourState> frozenBehaviourStates = new List<BehaviourState>();
    private readonly List<StarterInputsState> starterInputsStates = new List<StarterInputsState>();

    private struct BehaviourState
    {
        public Behaviour behaviour;
        public bool wasEnabled;
    }

    private struct StarterInputsState
    {
        public StarterAssets.StarterAssetsInputs input;
        public bool cursorLocked;
        public bool cursorInputForLook;
    }

    private void Reset()
    {
        if (sceneDialogues.Count > 0)
        {
            return;
        }

        sceneDialogues = new List<SceneDialogueConfig>
        {
            new SceneDialogueConfig { sceneName = "BornScene", autoStart = true },
            new SceneDialogueConfig { sceneName = "Level1", autoStart = true },
            new SceneDialogueConfig { sceneName = "Level2", autoStart = true },
            new SceneDialogueConfig { sceneName = "Level3", autoStart = true }
        };
    }

    private void Start()
    {
        EnsureDialogueUI();
        HideDialogueUI();
        TryAutoStartForCurrentScene();
    }

    private void Update()
    {
        if (!dialogueActive || consumeAllClicks)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            AdvanceDialogue();
        }
    }

    private void OnDisable()
    {
        if (dialogueActive)
        {
            EndDialogue();
        }
    }

    private void TryAutoStartForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        SceneDialogueConfig config = FindConfig(sceneName);
        if (config != null && !config.autoStart)
        {
            return;
        }

        List<string> lines = ResolveLines(sceneName);
        if (lines.Count == 0)
        {
            return;
        }

        StartDialogue(lines);
    }

    private SceneDialogueConfig FindConfig(string sceneName)
    {
        for (int i = 0; i < sceneDialogues.Count; i++)
        {
            SceneDialogueConfig config = sceneDialogues[i];
            if (string.Equals(config.sceneName, sceneName, StringComparison.OrdinalIgnoreCase))
            {
                return config;
            }
        }

        return null;
    }

    private List<string> ResolveLines(string sceneName)
    {
        List<string> result = new List<string>();
        SceneDialogueConfig config = FindConfig(sceneName);

        if (config != null)
        {
            CopyNonEmptyLines(config.lines, result);
        }

        if (result.Count == 0)
        {
            CopyNonEmptyLines(defaultLines, result);
        }

        return result;
    }

    private static void CopyNonEmptyLines(List<string> source, List<string> destination)
    {
        if (source == null)
        {
            return;
        }

        for (int i = 0; i < source.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(source[i]))
            {
                continue;
            }

            destination.Add(source[i].Trim());
        }
    }

    private void StartDialogue(List<string> lines)
    {
        if (lines == null || lines.Count == 0)
        {
            return;
        }

        EnsureDialogueUI();
        if (dialogueText == null || clickBlockerButton == null)
        {
            return;
        }

        activeLines = lines;
        currentLineIndex = 0;
        dialogueActive = true;

        FreezePlayerControl();
        ApplyDialogueCursorState();
        ShowDialogueUI();
        ApplyCurrentLine();
    }

    private void AdvanceDialogue()
    {
        if (!dialogueActive)
        {
            return;
        }

        if (currentLineIndex < activeLines.Count - 1)
        {
            currentLineIndex++;
            ApplyCurrentLine();
            return;
        }

        EndDialogue();
    }

    private void EndDialogue()
    {
        dialogueActive = false;
        onDialogueEnd?.Invoke();
        currentLineIndex = 0;
        activeLines.Clear();

        HideDialogueUI();
        RestorePlayerControl();
        RestoreCursorState();
    }

    private void ApplyCurrentLine()
    {
        if (dialogueText == null || activeLines.Count == 0)
        {
            return;
        }

        dialogueText.text = activeLines[Mathf.Clamp(currentLineIndex, 0, activeLines.Count - 1)];
    }

    private void EnsureDialogueUI()
    {
        if (dialogueRoot != null)
        {
            return;
        }

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>(true);
        }

        if (canvas == null)
        {
            Debug.LogWarning("[SceneIntroDialogueController] No Canvas found, dialogue UI cannot be created.");
            return;
        }

        Transform existingRoot = canvas.transform.Find(DialogueRootName);
        if (existingRoot != null)
        {
            dialogueRoot = existingRoot.gameObject;
            clickBlockerImage = dialogueRoot.transform.Find(ClickBlockerName)?.GetComponent<Image>();
            clickBlockerButton = dialogueRoot.transform.Find(ClickBlockerName)?.GetComponent<Button>();
            panelImage = dialogueRoot.transform.Find(PanelName)?.GetComponent<Image>();
            dialogueText = dialogueRoot.transform.Find(PanelName + "/" + TextName)?.GetComponent<Text>();
            ApplyVisualSettings();
            ConfigureClickAdvance();
            return;
        }

        dialogueRoot = new GameObject(DialogueRootName, typeof(RectTransform));
        dialogueRoot.transform.SetParent(canvas.transform, false);
        RectTransform rootRect = dialogueRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        GameObject blocker = new GameObject(ClickBlockerName, typeof(RectTransform), typeof(Image), typeof(Button));
        blocker.transform.SetParent(dialogueRoot.transform, false);
        RectTransform blockerRect = blocker.GetComponent<RectTransform>();
        blockerRect.anchorMin = Vector2.zero;
        blockerRect.anchorMax = Vector2.one;
        blockerRect.offsetMin = Vector2.zero;
        blockerRect.offsetMax = Vector2.zero;

        clickBlockerImage = blocker.GetComponent<Image>();
        clickBlockerButton = blocker.GetComponent<Button>();
        clickBlockerButton.transition = Selectable.Transition.None;
        clickBlockerButton.targetGraphic = clickBlockerImage;

        GameObject panel = new GameObject(PanelName, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(dialogueRoot.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(1f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(0f, panelHeight);
        panelRect.anchoredPosition = new Vector2(0f, panelBottomMargin);
        panelImage = panel.GetComponent<Image>();

        GameObject textObj = new GameObject(TextName, typeof(RectTransform), typeof(Text));
        textObj.transform.SetParent(panel.transform, false);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = panelPadding;
        textRect.offsetMax = new Vector2(-panelPadding.x, -panelPadding.y);
        dialogueText = textObj.GetComponent<Text>();
        dialogueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        dialogueText.horizontalOverflow = HorizontalWrapMode.Wrap;
        dialogueText.verticalOverflow = VerticalWrapMode.Truncate;
        dialogueText.alignment = TextAnchor.MiddleLeft;

        ApplyVisualSettings();
        ConfigureClickAdvance();
    }

    private void ApplyVisualSettings()
    {
        if (panelImage != null)
        {
            panelImage.color = panelColor;
            RectTransform panelRect = panelImage.rectTransform;
            panelRect.sizeDelta = new Vector2(0f, panelHeight);
            panelRect.anchoredPosition = new Vector2(0f, panelBottomMargin);
        }

        if (dialogueText != null)
        {
            dialogueText.color = textColor;
            dialogueText.fontSize = fontSize;
            RectTransform textRect = dialogueText.rectTransform;
            textRect.offsetMin = panelPadding;
            textRect.offsetMax = new Vector2(-panelPadding.x, -panelPadding.y);
        }
    }

    private void ConfigureClickAdvance()
    {
        if (clickBlockerButton == null || clickBlockerImage == null)
        {
            return;
        }

        clickBlockerButton.onClick.RemoveListener(AdvanceDialogue);

        if (consumeAllClicks)
        {
            clickBlockerImage.color = new Color(0f, 0f, 0f, 0.001f);
            clickBlockerImage.raycastTarget = true;
            clickBlockerButton.interactable = true;
            clickBlockerButton.onClick.AddListener(AdvanceDialogue);
        }
        else
        {
            clickBlockerImage.color = new Color(0f, 0f, 0f, 0f);
            clickBlockerImage.raycastTarget = false;
            clickBlockerButton.interactable = false;
        }
    }

    private void ShowDialogueUI()
    {
        if (dialogueRoot == null)
        {
            return;
        }

        ConfigureClickAdvance();
        dialogueRoot.SetActive(true);
    }

    private void HideDialogueUI()
    {
        if (dialogueRoot != null)
        {
            dialogueRoot.SetActive(false);
        }
    }

    private void FreezePlayerControl()
    {
        frozenBehaviourStates.Clear();
        starterInputsStates.Clear();

        CacheStarterInputStates();

        FreezeBehavioursOfType<StarterAssets.ThirdPersonController>();
        FreezeBehavioursOfType<StarterAssets.StarterAssetsInputs>();
        FreezeBehavioursOfType<InteractionSystem>();
#if ENABLE_INPUT_SYSTEM
        FreezeBehavioursOfType<PlayerInput>();
#endif
    }

    private void CacheStarterInputStates()
    {
        StarterAssets.StarterAssetsInputs[] inputs = FindObjectsOfType<StarterAssets.StarterAssetsInputs>(true);
        for (int i = 0; i < inputs.Length; i++)
        {
            if (inputs[i] == null)
            {
                continue;
            }

            starterInputsStates.Add(new StarterInputsState
            {
                input = inputs[i],
                cursorLocked = inputs[i].cursorLocked,
                cursorInputForLook = inputs[i].cursorInputForLook
            });

            inputs[i].cursorLocked = false;
            inputs[i].cursorInputForLook = false;
        }
    }

    private void FreezeBehavioursOfType<T>() where T : Behaviour
    {
        T[] behaviours = FindObjectsOfType<T>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            T behaviour = behaviours[i];
            if (behaviour == null)
            {
                continue;
            }

            frozenBehaviourStates.Add(new BehaviourState
            {
                behaviour = behaviour,
                wasEnabled = behaviour.enabled
            });

            if (behaviour.enabled)
            {
                behaviour.enabled = false;
            }
        }
    }

    private void RestorePlayerControl()
    {
        for (int i = 0; i < frozenBehaviourStates.Count; i++)
        {
            BehaviourState state = frozenBehaviourStates[i];
            if (state.behaviour != null)
            {
                state.behaviour.enabled = state.wasEnabled;
            }
        }

        for (int i = 0; i < starterInputsStates.Count; i++)
        {
            StarterInputsState state = starterInputsStates[i];
            if (state.input == null)
            {
                continue;
            }

            state.input.cursorLocked = state.cursorLocked;
            state.input.cursorInputForLook = state.cursorInputForLook;
        }

        frozenBehaviourStates.Clear();
        starterInputsStates.Clear();
    }

    private void ApplyDialogueCursorState()
    {
        previousCursorVisible = Cursor.visible;
        previousCursorLockMode = Cursor.lockState;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void RestoreCursorState()
    {
        Cursor.visible = previousCursorVisible;
        Cursor.lockState = previousCursorLockMode;
    }
}
