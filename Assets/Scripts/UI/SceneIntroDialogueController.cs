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

    [Header("UI References (drag from Canvas children)")]
    [SerializeField] private GameObject dialogueRoot;
    [SerializeField] private Button clickBlockerButton;
    [SerializeField] private Text dialogueText;

    [Header("Click Behavior")]
    [SerializeField] private bool consumeAllClicks = true;

    public bool IsDialogueActive() { return dialogueActive; }

    public void StartDialogueForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        List<string> lines = ResolveLines(sceneName);
        if (lines.Count > 0) StartDialogue(lines);
    }

    public bool HasConfiguredLines()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        var config = FindConfig(sceneName);
        if (config != null && config.lines.Count > 0) return true;
        if (defaultLines.Count > 0) return true;
        return false;
    }

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
        if (sceneDialogues.Count > 0) return;
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
        if (dialogueRoot != null) dialogueRoot.SetActive(false);
        TryAutoStartForCurrentScene();
    }

    private void Update()
    {
        if (!dialogueActive || consumeAllClicks) return;
        if (Input.GetMouseButtonDown(0)) AdvanceDialogue();
    }

    private void OnDisable()
    {
        if (dialogueActive) EndDialogue();
    }

    private void TryAutoStartForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        SceneDialogueConfig config = FindConfig(sceneName);
        if (config != null && !config.autoStart) return;
        List<string> lines = ResolveLines(sceneName);
        if (lines.Count == 0) return;
        StartDialogue(lines);
    }

    private SceneDialogueConfig FindConfig(string sceneName)
    {
        for (int i = 0; i < sceneDialogues.Count; i++)
        {
            if (string.Equals(sceneDialogues[i].sceneName, sceneName, StringComparison.OrdinalIgnoreCase))
                return sceneDialogues[i];
        }
        return null;
    }

    private List<string> ResolveLines(string sceneName)
    {
        List<string> result = new List<string>();
        SceneDialogueConfig config = FindConfig(sceneName);
        if (config != null) CopyNonEmptyLines(config.lines, result);
        if (result.Count == 0) CopyNonEmptyLines(defaultLines, result);
        return result;
    }

    private static void CopyNonEmptyLines(List<string> source, List<string> destination)
    {
        if (source == null) return;
        for (int i = 0; i < source.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(source[i]))
                destination.Add(source[i].Trim());
        }
    }

    public void StartDialogue(List<string> lines)
    {
        if (lines == null || lines.Count == 0) return;
        if (dialogueText == null || clickBlockerButton == null) return;

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
        if (!dialogueActive) return;
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
        if (dialogueText == null || activeLines.Count == 0) return;
        dialogueText.text = activeLines[Mathf.Clamp(currentLineIndex, 0, activeLines.Count - 1)];
    }

    private void ShowDialogueUI()
    {
        if (dialogueRoot == null) return;

        clickBlockerButton.onClick.RemoveAllListeners();
        if (consumeAllClicks)
            clickBlockerButton.onClick.AddListener(AdvanceDialogue);

        dialogueRoot.SetActive(true);
    }

    private void HideDialogueUI()
    {
        if (dialogueRoot != null) dialogueRoot.SetActive(false);
    }

    // ===== Player Freeze / Restore =====

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
            if (inputs[i] == null) continue;
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
            if (behaviours[i] == null) continue;
            frozenBehaviourStates.Add(new BehaviourState { behaviour = behaviours[i], wasEnabled = behaviours[i].enabled });
            if (behaviours[i].enabled) behaviours[i].enabled = false;
        }
    }

    private void RestorePlayerControl()
    {
        for (int i = 0; i < frozenBehaviourStates.Count; i++)
        {
            BehaviourState state = frozenBehaviourStates[i];
            if (state.behaviour != null) state.behaviour.enabled = state.wasEnabled;
        }
        for (int i = 0; i < starterInputsStates.Count; i++)
        {
            StarterInputsState state = starterInputsStates[i];
            if (state.input == null) continue;
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
