using UnityEngine;
using UnityEngine.Playables;
using StarterAssets;

/// <summary>
/// Lightweight replacement for BornSceneSequenceManager.
/// Handles input/trigger detection and delegates all cinematic work to Timeline.
/// </summary>
public class BornSceneTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private SceneIntroDialogueController dialogueController;

    [Header("Effects")]
    [SerializeField] private GameObject areaStarYellow;

    [Header("UI")]
    [SerializeField] private GameObject movementHintUI;
    [SerializeField] private GameObject interactPromptUI;

    [Header("Player")]
    [SerializeField] private GameObject player;

    [Header("Trigger")]
    [SerializeField] private TriggerDetector areaTrigger;

    [Header("Timeline")]
    [SerializeField] private BornSceneDirector bornSceneDirector;

    private bool playerInTrigger;
    private bool dialogueDone;
    private bool waitingForF;

    private ThirdPersonController tpc;
    private StarterAssetsInputs inputs;
    private UnityEngine.InputSystem.PlayerInput playerInputComp;

    private void Start()
    {
        // Hide everything initially
        if (movementHintUI != null) movementHintUI.SetActive(false);
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (areaStarYellow != null) areaStarYellow.SetActive(false);
        // Timeline is controlled by BornSceneDirector

        CachePlayerComponents();

        // Listen for dialogue end
        if (dialogueController != null)
        {
            if (!dialogueController.HasConfiguredLines())
                OnDialogueEnd();
            else
                dialogueController.onDialogueEnd.AddListener(OnDialogueEnd);
        }
        else
        {
            OnDialogueEnd();
        }

        // Wire trigger
        if (areaTrigger != null)
        {
            areaTrigger.onTriggerEnter.AddListener(OnPlayerEnter);
            areaTrigger.onTriggerExit.AddListener(OnPlayerExit);
        }
    }

    private void OnDestroy()
    {
        if (dialogueController != null)
            dialogueController.onDialogueEnd.RemoveListener(OnDialogueEnd);
        if (areaTrigger != null)
        {
            areaTrigger.onTriggerEnter.RemoveListener(OnPlayerEnter);
            areaTrigger.onTriggerExit.RemoveListener(OnPlayerExit);
        }
    }

    private void CachePlayerComponents()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player != null)
        {
            tpc = player.GetComponentInChildren<ThirdPersonController>();
            inputs = player.GetComponentInChildren<StarterAssetsInputs>();
            playerInputComp = player.GetComponentInChildren<UnityEngine.InputSystem.PlayerInput>();
        }
    }

    private void Update()
    {
        if (waitingForF && playerInTrigger && Input.GetKeyDown(KeyCode.F))
        {
            PlayTimeline();
        }
    }

    // ---- Flow ----

    private void OnDialogueEnd()
    {
        dialogueDone = true;
        if (movementHintUI != null) movementHintUI.SetActive(true);
        if (areaStarYellow != null) areaStarYellow.SetActive(true);
    }

    private void OnPlayerEnter(GameObject obj)
    {
        if (!dialogueDone) return;
        playerInTrigger = true;
        waitingForF = true;
        if (interactPromptUI != null) interactPromptUI.SetActive(true);
    }

    private void OnPlayerExit(GameObject obj)
    {
        playerInTrigger = false;
        waitingForF = false;
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
    }

    private void PlayTimeline()
    {
        waitingForF = false;
        playerInTrigger = false;
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (movementHintUI != null) movementHintUI.SetActive(false);

        // Disable player before timeline plays
        if (tpc != null) tpc.enabled = false;
        if (inputs != null) { inputs.cursorLocked = false; inputs.cursorInputForLook = false; }
        if (playerInputComp != null) playerInputComp.enabled = false;

        if (bornSceneDirector != null)
            bornSceneDirector.PlayTimeline();
    }
}
