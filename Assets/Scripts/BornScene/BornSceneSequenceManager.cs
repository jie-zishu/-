using System.Collections;
using UnityEngine;
using Cinemachine;

public class BornSceneSequenceManager : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private SceneIntroDialogueController dialogueController;

    [Header("Area & Effects")]
    [SerializeField] private GameObject areaStarYellow;
    [SerializeField] private TriggerDetector areaTrigger;

    [Header("UI")]
    [SerializeField] private GameObject movementHintUI;
    [SerializeField] private GameObject interactPromptUI;

    [Header("Player")]
    [SerializeField] private GameObject player;
    [SerializeField] private Transform sitPosition;

    [Header("Radio & Camera")]
    [SerializeField] private FloatOscillator radioFloater;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform radioLookTarget;

    [Header("==== Phase 1: Hold looking at player ====")]
    [SerializeField] private float phase1_Duration = 1.5f;

    [Header("==== Phase 2: Approach player (0→1 ease-out) ====")]
    [SerializeField] private float phase2_Duration = 1.5f;
    [SerializeField] private float phase2_EndDistance = 2.5f;

    [Header("==== Phase 3: Radio rise timing ====")]
    [SerializeField] private float radioRiseAt = 3f;

    [Header("==== Phase 4: Pan camera to radio (position + rotation, ease-out) ====")]
    [SerializeField] private float phase4_StartAt = 3.5f;
    [SerializeField] private float phase4_Duration = 1.5f;
    [SerializeField] private float phase4_DistanceFromRadio = 3.5f;

    [Header("==== Phase 5: Dolly closer to radio (ease-out) ====")]
    [SerializeField] private float phase5_StartAt = 5f;
    [SerializeField] private float phase5_Duration = 2f;
    [SerializeField] private float phase5_DistanceFromRadio = 1.5f;

    [Header("==== Scene Transition ====")]
    [SerializeField] private float totalSequenceTime = 8f;
    [SerializeField] private bool enableSceneTransition = false;

    private enum SequenceState
    {
        WaitingForDialogue,
        MovingToChair,
        WaitingForInteract,
        Sitting,
        Complete
    }

    private SequenceState state = SequenceState.WaitingForDialogue;
    private bool playerInTrigger;

    private void Start()
    {
        if (movementHintUI != null) movementHintUI.SetActive(false);
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (areaStarYellow != null) areaStarYellow.SetActive(false);

        if (dialogueController != null)
            dialogueController.onDialogueEnd.AddListener(OnDialogueEnd);

        if (areaTrigger != null)
        {
            areaTrigger.onTriggerEnter.AddListener(OnPlayerEnterArea);
            areaTrigger.onTriggerExit.AddListener(OnPlayerExitArea);
        }
    }

    private void OnDestroy()
    {
        if (dialogueController != null)
            dialogueController.onDialogueEnd.RemoveListener(OnDialogueEnd);
        if (areaTrigger != null)
        {
            areaTrigger.onTriggerEnter.RemoveListener(OnPlayerEnterArea);
            areaTrigger.onTriggerExit.RemoveListener(OnPlayerExitArea);
        }
    }

    private void Update()
    {
        if (state == SequenceState.WaitingForInteract && playerInTrigger)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                StartCoroutine(SitSequence());
            }
        }
    }

    private void OnDialogueEnd()
    {
        state = SequenceState.MovingToChair;
        if (movementHintUI != null) movementHintUI.SetActive(true);
        if (areaStarYellow != null) areaStarYellow.SetActive(true);
    }

    private void OnPlayerEnterArea(GameObject obj)
    {
        if (state != SequenceState.MovingToChair) return;
        playerInTrigger = true;
        state = SequenceState.WaitingForInteract;
        if (interactPromptUI != null) interactPromptUI.SetActive(true);
    }

    private void OnPlayerExitArea(GameObject obj)
    {
        if (state != SequenceState.WaitingForInteract) return;
        playerInTrigger = false;
        state = SequenceState.MovingToChair;
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
    }

    private static float EaseOut(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }

    private IEnumerator SitSequence()
    {
        state = SequenceState.Sitting;
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (movementHintUI != null) movementHintUI.SetActive(false);
        if (areaStarYellow != null) areaStarYellow.SetActive(false);

        // --- Create independent cinematic camera ---
        Camera mainCam = Camera.main;
        Transform camStart = new GameObject("CamStartMarker").transform;
        camStart.position = mainCam.transform.position;
        camStart.rotation = mainCam.transform.rotation;

        GameObject cineGo = new GameObject("CinematicCamera");
        cineGo.transform.position = camStart.position;
        cineGo.transform.rotation = camStart.rotation;
        Camera cineCam = cineGo.AddComponent<Camera>();
        cineCam.fieldOfView = mainCam.fieldOfView;
        cineCam.nearClipPlane = mainCam.nearClipPlane;
        cineCam.farClipPlane = mainCam.farClipPlane;
        cineCam.depth = mainCam.depth + 1;
        cineCam.tag = "MainCamera";

        mainCam.enabled = false;
        mainCam.tag = "Untagged";
        var brain = mainCam.GetComponent<CinemachineBrain>();
        if (brain != null) brain.enabled = false;

        // --- Disable player & teleport ---
        var tpc = player.GetComponentInChildren<StarterAssets.ThirdPersonController>();
        var inputs = player.GetComponentInChildren<StarterAssets.StarterAssetsInputs>();
        var playerInput = player.GetComponentInChildren<UnityEngine.InputSystem.PlayerInput>();
        if (tpc != null) tpc.enabled = false;
        if (inputs != null) { inputs.cursorLocked = false; inputs.cursorInputForLook = false; }
        if (playerInput != null) playerInput.enabled = false;

        var cc = player.GetComponentInChildren<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = sitPosition.position;
        player.transform.rotation = sitPosition.rotation;

        var armature = player.transform.Find("PlayerArmature");
        if (armature != null)
        {
            armature.localPosition = Vector3.zero;
            armature.localRotation = Quaternion.identity;
        }

        var animator = player.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.SetTrigger("SitDown");
        }

        // --- Pre-compute target positions/rotations for each phase ---
        Vector3 playerTargetPos = sitPosition.position + Vector3.up * 1.2f;
        Vector3 radioTarget = radioLookTarget != null
            ? radioLookTarget.position
            : (radioFloater != null ? radioFloater.transform.position + Vector3.up * 0.3f : Vector3.up * 1.2f);

        // Phase 1 end = same as camera start (hold still)
        // Phase 2 end: closer to player
        Vector3 dirToPlayer = (playerTargetPos - camStart.position).normalized;
        Vector3 p2_EndPos = playerTargetPos - dirToPlayer * phase2_EndDistance;
        Quaternion p2_EndRot = Quaternion.LookRotation(dirToPlayer, Vector3.up);

        // Phase 4 end: position looking at radio from a medium distance
        Vector3 dirToRadio_fromP2 = (radioTarget - p2_EndPos).normalized;
        Vector3 p4_EndPos = radioTarget - dirToRadio_fromP2 * phase4_DistanceFromRadio;
        Quaternion p4_EndRot = Quaternion.LookRotation(dirToRadio_fromP2, Vector3.up);

        // Phase 5 end: even closer to radio
        Vector3 dirToRadio_fromP4 = (radioTarget - p4_EndPos).normalized;
        Vector3 p5_EndPos = radioTarget - dirToRadio_fromP4 * phase5_DistanceFromRadio;

        // --- State tracking ---
        bool radioRisen = false;
        Vector3 p4_StartPos = Vector3.zero, p5_StartPos = Vector3.zero;
        Quaternion p4_StartRot = Quaternion.identity;

        float t = 0f;
        float endTime = totalSequenceTime;

        while (t < endTime)
        {
            t += Time.deltaTime;

            // Phase 1: hold (0 → phase1_Duration)
            // camera stays at snapshot

            // Phase 2: approach player
            float p2Start = phase1_Duration;
            float p2End = p2Start + phase2_Duration;
            if (t > p2Start && t <= p2End)
            {
                float p = EaseOut(Mathf.Clamp01((t - p2Start) / phase2_Duration));
                cineGo.transform.position = Vector3.Lerp(camStart.position, p2_EndPos, p);
                cineGo.transform.rotation = Quaternion.Slerp(camStart.rotation, p2_EndRot, p);
            }
            else if (t > p2End && t < phase4_StartAt)
            {
                // Hold at phase 2 end position
                cineGo.transform.position = p2_EndPos;
                cineGo.transform.rotation = p2_EndRot;
            }

            // Phase 3: radio rises
            if (!radioRisen && t >= radioRiseAt)
            {
                radioRisen = true;
                if (radioFloater != null) radioFloater.StartFloating();
            }

            // Phase 4: pan to radio (position + rotation)
            if (t >= phase4_StartAt && t <= phase4_StartAt + phase4_Duration)
            {
                if (p4_StartPos == Vector3.zero) // first frame snapshot
                {
                    p4_StartPos = cineGo.transform.position;
                    p4_StartRot = cineGo.transform.rotation;
                }
                float p = EaseOut(Mathf.Clamp01((t - phase4_StartAt) / phase4_Duration));
                cineGo.transform.position = Vector3.Lerp(p4_StartPos, p4_EndPos, p);
                cineGo.transform.rotation = Quaternion.Slerp(p4_StartRot, p4_EndRot, p);
            }

            // Phase 5: dolly closer to radio
            if (t >= phase5_StartAt && t <= phase5_StartAt + phase5_Duration)
            {
                if (p5_StartPos == Vector3.zero) // first frame snapshot
                {
                    p5_StartPos = cineGo.transform.position;
                }
                float p = EaseOut(Mathf.Clamp01((t - phase5_StartAt) / phase5_Duration));
                cineGo.transform.position = Vector3.Lerp(p5_StartPos, p5_EndPos, p);
            }

            yield return null;
        }

        // Clean up
        state = SequenceState.Complete;
        Destroy(cineGo);
        Destroy(camStart.gameObject);
        if (enableSceneTransition)
            GameFrameworkManager.Instance.CompleteLevel(0);
    }
}
