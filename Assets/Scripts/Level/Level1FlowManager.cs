using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using StarterAssets;

public class Level1FlowManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject player;
    [SerializeField] private Transform benchSitPosition;

    [Header("Effects")]
    [SerializeField] private GameObject pianoShineEffect;
    [SerializeField] private GameObject areaStarEffect;
    [SerializeField] private GameObject[] cubeEffects;

    [Header("Piano Interaction")]
    [SerializeField] private GameObject pianoBillboard;
    [SerializeField] private TriggerDetector pianoTrigger;

    [Header("Cube Interactions")]
    [SerializeField] private TriggerDetector[] cubeTriggers;
    [SerializeField] private GameObject[] cubeBillboards;

    [Header("MP3 Easter Egg")]
    [SerializeField] private GameObject mp3Effect;
    [SerializeField] private GameObject mp3Billboard;
    [SerializeField] private TriggerDetector mp3Trigger;
    [SerializeField] private string[] mp3Dialogues = new string[] { "一台老旧的MP3，还在播放着很久以前的歌..." };

    [Header("Performance Area")]
    [SerializeField] private GameObject performanceBillboard;
    [SerializeField] private TriggerDetector performanceTrigger;

    [Header("UI - Exploration Counter")]
    [SerializeField] private GameObject explorationCounterUI;
    [SerializeField] private Text counterText;

    [Header("UI - Go To Piano Hint")]
    [SerializeField] private GameObject goToPianoUI;

    [Header("Dialogue")]
    [SerializeField] private SceneIntroDialogueController dialogueController;
    [SerializeField] private string[] pianoInteractionDialogues = new string[] {
        "一架落满灰尘的旧钢琴，琴键却依然光亮如新。",
        "仿佛在等待谁来弹奏它，但它似乎少了些什么..."
    };
    [SerializeField] private string[] cubeFoundDialogues = new string[] { "测试一", "测试二", "测试三", "测试四" };
    [SerializeField] private string[] allCubesFoundDialogues = new string[] {
        "找到了所有碎片，附近似乎有什么东西在发光。"
    };

    [Header("Interaction Ranges")]
    [SerializeField] private float pianoTriggerRadius = 1.5f;
    [SerializeField] private float cubeTriggerRadius = 1.2f;
    [SerializeField] private float performanceTriggerRadius = 2f;

    [Header("Bench Reading")]
    [SerializeField] private GameObject benchSparkleEffect;
    [SerializeField] private GameObject readTextUI;
    [SerializeField] private GameObject exitReadUI;
    [SerializeField] private GameObject readBillboard;
    [SerializeField] private TriggerDetector benchReadTrigger;
    [SerializeField] private Transform allTextBench;
    [SerializeField] private Transform allTextPiano;
    [SerializeField] private CinemachineVirtualCamera readingVCam;
    [SerializeField] private string[] benchReadingEndDialogues = new string[] {
        "原来是这份乐谱...",
        "把它带到钢琴那边去吧。"
    };

    [Header("Ending")]
    [SerializeField] private Level1EndingDirector endingDirector;

    [Header("Piano UI")]
    [SerializeField] private PianoGameManager pianoGameManager;
    [SerializeField] private PianoGamePlay pianoGamePlay;

    private enum Phase
    {
        InitialDialogue,
        ApproachPiano,
        PianoDialogue,
        Exploration,
        AllCubesFoundDialogue,
        ApproachBenchReading,
        Reading,
        ApproachPerformance,
        Performance
    }

    private Phase currentPhase = Phase.InitialDialogue;
    private int cubesFound;
    private bool waitingForF;
    private TriggerDetector activeTrigger;

    private ThirdPersonController tpc;
    private StarterAssetsInputs inputs;
    private UnityEngine.InputSystem.PlayerInput playerInput;
    private CharacterController cc;
    private Animator animator;
    private Vector3 returnPosition;
    private Vector2 lastMousePos;
    private bool isDragging;

    private void Awake()
    {
        // Auto-create missing UI objects at runtime
        AutoCreateBillboards();
        AutoCreateCounter();
        AutoCreateGoToPianoUI();
        AutoCreateTriggers();
        AutoWireReferences();
        SetupExistingBillboards();

        // Listen for piano close to restore player
        if (pianoGameManager != null)
            pianoGameManager.onPianoClosed.AddListener(OnPianoClosed);
    }

    private void Start()
    {
        CachePlayerComponents();
        HideAllEffects();
        DisableAllBillboards();
        DisableAllTriggers();
        if (explorationCounterUI != null) explorationCounterUI.SetActive(false);

        // Start initial dialogue — or skip if no lines configured
        if (dialogueController != null && dialogueController.HasConfiguredLines())
        {
            dialogueController.onDialogueEnd.AddListener(OnInitialDialogueEnd);
            StartCoroutine(WaitThenStartDialogue());
        }
        else
        {
            // No dialogue configured, skip to Phase 1
            OnInitialDialogueEnd();
        }
    }

    private IEnumerator WaitThenStartDialogue()
    {
        yield return null;
        // Explicitly start dialogue (don't rely on autoStart)
        if (dialogueController != null && dialogueController.HasConfiguredLines())
            dialogueController.StartDialogueForCurrentScene();
    }

    private void CachePlayerComponents()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
        }
        if (player != null)
        {
            tpc = player.GetComponentInChildren<ThirdPersonController>();
            inputs = player.GetComponentInChildren<StarterAssetsInputs>();
            playerInput = player.GetComponentInChildren<UnityEngine.InputSystem.PlayerInput>();
            cc = player.GetComponentInChildren<CharacterController>();
            animator = player.GetComponentInChildren<Animator>();
        }
        // Lock camera during dialogue to prevent spin artifact
        if (tpc != null) tpc.LockCameraPosition = true;
    }

    // ===== PHASE TRANSITIONS =====

    private void OnInitialDialogueEnd()
    {
        dialogueController.onDialogueEnd.RemoveListener(OnInitialDialogueEnd);
        currentPhase = Phase.ApproachPiano;

        // Unlock camera now that dialogue is done
        if (tpc != null) tpc.LockCameraPosition = false;

        ShowPianoGuide();
    }

    private System.Collections.IEnumerator ResetCameraNextFrame()
    {
        yield return null;
        // Reset TPC internal camera angles only — do NOT touch vcam.transform
        // (Cinemachine Follow mode drives the transform; manual changes cause conflicts)
        if (tpc != null)
        {
            var tpcType = tpc.GetType();
            var yawField = tpcType.GetField("_cinemachineTargetYaw", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pitchField = tpcType.GetField("_cinemachineTargetPitch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (yawField != null) yawField.SetValue(tpc, player.transform.eulerAngles.y);
            if (pitchField != null) pitchField.SetValue(tpc, 10f);
        }
    }

    private void ShowPianoGuide()
    {
        if (pianoShineEffect != null) pianoShineEffect.SetActive(true);
        EnableTrigger(pianoTrigger, OnPianoTriggerEnter, OnPianoTriggerExit);
    }

    private void OnPianoTriggerEnter(GameObject obj)
    {
        if (currentPhase != Phase.ApproachPiano) return;
        pianoBillboard.SetActive(true);
        activeTrigger = pianoTrigger;
        waitingForF = true;
    }

    private void OnPianoTriggerExit(GameObject obj)
    {
        pianoBillboard.SetActive(false);
        waitingForF = false;
        activeTrigger = null;
    }

    private void PianoInteracted()
    {
        waitingForF = false;
        activeTrigger = null;
        currentPhase = Phase.PianoDialogue;
        pianoBillboard.SetActive(false);
        DisableTrigger(pianoTrigger);

        // Start second dialogue about the piano
        if (dialogueController != null)
        {
            dialogueController.onDialogueEnd.AddListener(OnPianoDialogueEnd);
            dialogueController.StartDialogue(new System.Collections.Generic.List<string>(pianoInteractionDialogues));
        }
        else
        {
            OnPianoDialogueEnd();
        }
    }

    private void OnPianoDialogueEnd()
    {
        dialogueController.onDialogueEnd.RemoveListener(OnPianoDialogueEnd);
        StartExplorationPhase();
    }

    // ===== EXPLORATION PHASE =====

    private void StartExplorationPhase()
    {
        currentPhase = Phase.Exploration;
        cubesFound = 0;

        // Hide piano shine — no longer needed after piano interaction
        if (pianoShineEffect != null) pianoShineEffect.SetActive(false);

        // Show exploration UI
        if (explorationCounterUI != null) explorationCounterUI.SetActive(true);
        UpdateCounterText();

        // Show all cube effects + MP3 easter egg
        if (cubeEffects != null)
            foreach (var fx in cubeEffects) if (fx != null) fx.SetActive(true);
        if (mp3Effect != null) mp3Effect.SetActive(true);

        // Enable all cube triggers
        for (int i = 0; i < cubeTriggers.Length; i++)
        {
            int index = i;
            EnableTrigger(cubeTriggers[i],
                (go) => OnCubeTriggerEnter(index, go),
                (go) => OnCubeTriggerExit(index, go));
        }

        // Enable MP3 easter egg trigger
        EnableTrigger(mp3Trigger, OnMP3TriggerEnter, OnMP3TriggerExit);
    }

    private void OnCubeTriggerEnter(int index, GameObject obj)
    {
        if (currentPhase != Phase.Exploration) return;
        if (cubeBillboards != null && index < cubeBillboards.Length && cubeBillboards[index] != null)
            cubeBillboards[index].SetActive(true);
        activeTrigger = cubeTriggers[index];
        waitingForF = true;
    }

    private void OnMP3TriggerEnter(GameObject obj)
    {
        if (currentPhase != Phase.Exploration) return;
        if (mp3Billboard != null) mp3Billboard.SetActive(true);
        activeTrigger = mp3Trigger;
        waitingForF = true;
    }

    private void OnMP3TriggerExit(GameObject obj)
    {
        if (mp3Billboard != null) mp3Billboard.SetActive(false);
        waitingForF = false;
        activeTrigger = null;
    }

    private void OnCubeTriggerExit(int index, GameObject obj)
    {
        if (cubeBillboards != null && index < cubeBillboards.Length && cubeBillboards[index] != null)
            cubeBillboards[index].SetActive(false);
        waitingForF = false;
        activeTrigger = null;
    }

    private void CubeInteracted(int index)
    {
        waitingForF = false;
        activeTrigger = null;

        // Hide the cube itself, its effect, and billboard
        if (cubeTriggers != null && index < cubeTriggers.Length && cubeTriggers[index] != null)
            cubeTriggers[index].gameObject.SetActive(false);
        if (cubeEffects != null && index < cubeEffects.Length && cubeEffects[index] != null)
            cubeEffects[index].SetActive(false);
        if (cubeBillboards != null && index < cubeBillboards.Length && cubeBillboards[index] != null)
            cubeBillboards[index].SetActive(false);
        DisableTrigger(cubeTriggers[index]);

        cubesFound++;
        UpdateCounterText();

        // Show cube dialogue
        string msg = (cubeFoundDialogues != null && index < cubeFoundDialogues.Length)
            ? cubeFoundDialogues[index] : "[未配置对话]";
        if (dialogueController != null)
        {
            dialogueController.onDialogueEnd.AddListener(OnCubeDialogueEnd);
            dialogueController.StartDialogue(new System.Collections.Generic.List<string> { msg });
        }
        else
        {
            OnCubeDialogueEnd();
        }
    }

    private void OnCubeDialogueEnd()
    {
        dialogueController.onDialogueEnd.RemoveListener(OnCubeDialogueEnd);
        waitingForF = false;
        activeTrigger = null;

        if (cubesFound >= 4)
        {
            AllCubesFound();
        }
    }

    private void UpdateCounterText()
    {
        if (counterText != null)
            counterText.text = "找到过去的碎片 " + cubesFound + "/4";
    }

    private void AllCubesFound()
    {
        if (explorationCounterUI != null) explorationCounterUI.SetActive(false);

        if (dialogueController != null)
        {
            dialogueController.onDialogueEnd.AddListener(OnAllCubesDialogueEnd);
            StartCoroutine(DelayedAllCubesDialogue());
        }
        else
        {
            OnAllCubesDialogueEnd();
        }
    }

    private System.Collections.IEnumerator DelayedAllCubesDialogue()
    {
        yield return null; // wait for current EndDialogue to fully complete
        dialogueController.StartDialogue(new System.Collections.Generic.List<string>(allCubesFoundDialogues));
    }

    private void OnAllCubesDialogueEnd()
    {
        dialogueController.onDialogueEnd.RemoveListener(OnAllCubesDialogueEnd);

        currentPhase = Phase.ApproachBenchReading;

        // Show bench reading guide
        if (benchSparkleEffect != null) benchSparkleEffect.SetActive(true);
        if (readTextUI != null) readTextUI.SetActive(true);

        // Enable bench read trigger
        EnableTrigger(benchReadTrigger, OnBenchReadTriggerEnter, OnBenchReadTriggerExit);
    }

    // ===== PERFORMANCE PHASE =====

    // ===== BENCH READING PHASE =====

    private void OnBenchReadTriggerEnter(GameObject obj)
    {
        if (currentPhase != Phase.ApproachBenchReading) return;
        if (readBillboard != null) readBillboard.SetActive(true);
        activeTrigger = benchReadTrigger;
        waitingForF = true;
    }

    private void OnBenchReadTriggerExit(GameObject obj)
    {
        if (readBillboard != null) readBillboard.SetActive(false);
        waitingForF = false;
        activeTrigger = null;
    }

    private void EnterReadingMode()
    {
        waitingForF = false;
        currentPhase = Phase.Reading;

        if (readBillboard != null) readBillboard.SetActive(false);
        if (readTextUI != null) readTextUI.SetActive(false);
        if (exitReadUI != null) exitReadUI.SetActive(true);
        if (benchSparkleEffect != null) benchSparkleEffect.SetActive(false);

        // Raise All_Text_bench
        if (allTextBench != null)
            allTextBench.position += Vector3.up * 0.2f;

        // Switch to reading camera
        if (readingVCam != null)
            readingVCam.Priority = 20;
        var playerVCam = GameObject.Find("Virtual Camera")?.GetComponent<CinemachineVirtualCamera>();
        if (playerVCam != null) playerVCam.Priority = 0;

        // Disable player control
        if (tpc != null) tpc.enabled = false;
        if (inputs != null) { inputs.cursorLocked = false; inputs.cursorInputForLook = false; }
        if (playerInput != null) playerInput.enabled = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void ExitReadingMode()
    {
        // Switch camera back to player
        if (readingVCam != null) readingVCam.Priority = 0;
        var playerVCam = GameObject.Find("Virtual Camera")?.GetComponent<CinemachineVirtualCamera>();
        if (playerVCam != null) playerVCam.Priority = 20;

        if (exitReadUI != null) exitReadUI.SetActive(false);

        // Swap objects: hide bench text, show piano version
        if (allTextBench != null) allTextBench.gameObject.SetActive(false);
        if (allTextPiano != null) allTextPiano.gameObject.SetActive(true);

        // Restore player
        if (tpc != null) tpc.enabled = true;
        if (inputs != null) { inputs.cursorLocked = true; inputs.cursorInputForLook = true; }
        if (playerInput != null) playerInput.enabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Dialogue before continuing to performance
        if (dialogueController != null && benchReadingEndDialogues != null && benchReadingEndDialogues.Length > 0)
        {
            dialogueController.onDialogueEnd.AddListener(OnBenchReadingEndDialogueDone);
            dialogueController.StartDialogue(new System.Collections.Generic.List<string>(benchReadingEndDialogues));
        }
        else
        {
            OnBenchReadingEndDialogueDone();
        }
    }

    private void OnBenchReadingEndDialogueDone()
    {
        dialogueController.onDialogueEnd.RemoveListener(OnBenchReadingEndDialogueDone);

        currentPhase = Phase.ApproachPerformance;
        if (areaStarEffect != null) areaStarEffect.SetActive(true);
        if (goToPianoUI != null) goToPianoUI.SetActive(true);
        EnableTrigger(performanceTrigger, OnPerformanceTriggerEnter, OnPerformanceTriggerExit);
    }

    private void HandleReadingUpdate()
    {
        // F to exit reading mode
        if (Input.GetKeyDown(KeyCode.F))
        {
            ExitReadingMode();
            return;
        }

        // Mouse drag to rotate All_Text_bench
        if (allTextBench == null) return;
        var cam = Camera.main;
        if (cam == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0)) isDragging = false;

        if (isDragging)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastMousePos;
            lastMousePos = Input.mousePosition;
            allTextBench.Rotate(cam.transform.up, delta.x * 0.3f, Space.World);
            allTextBench.Rotate(cam.transform.right, -delta.y * 0.3f, Space.World);
        }
    }

    private void OnPerformanceTriggerEnter(GameObject obj)
    {
        if (currentPhase != Phase.ApproachPerformance) return;
        performanceBillboard.SetActive(true);
        activeTrigger = performanceTrigger;
        waitingForF = true;
    }

    private void OnPerformanceTriggerExit(GameObject obj)
    {
        performanceBillboard.SetActive(false);
        waitingForF = false;
        activeTrigger = null;
    }

    private void StartPerformance()
    {
        currentPhase = Phase.Performance;
        performanceBillboard.SetActive(false);
        if (goToPianoUI != null) goToPianoUI.SetActive(false);
        if (areaStarEffect != null) areaStarEffect.SetActive(false);
        waitingForF = false;

        // Save return position (Area_star_ellow's position)
        returnPosition = performanceTrigger != null ? performanceTrigger.transform.position : player.transform.position;

        // Disable player controls
        if (tpc != null) tpc.enabled = false;
        if (inputs != null) { inputs.cursorLocked = false; inputs.cursorInputForLook = false; }
        if (playerInput != null) playerInput.enabled = false;

        // Use Bench_Piano's sit position
        Transform sitTarget = benchSitPosition;
        var benchPiano = GameObject.Find("SceneObject/Bench_Piano");
        if (benchPiano != null) sitTarget = benchPiano.transform.Find("SitPosition") ?? sitTarget;

        // Teleport to bench (keep CC disabled during sit)
        if (cc != null) cc.enabled = false;
        player.transform.position = sitTarget.position;
        player.transform.rotation = sitTarget.rotation;
        var armature = player.transform.Find("PlayerArmature");
        if (armature != null)
        {
            armature.localPosition = Vector3.zero;
            armature.localRotation = Quaternion.identity;
        }

        // Trigger sit animation
        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.SetTrigger("SitDown");
        }

        StartCoroutine(PerformanceSequence());
    }

    private System.Collections.IEnumerator PerformanceSequence()
    {
        // Phase 1: wait 0.5s with character sitting
        yield return new WaitForSeconds(0.5f);

        // Phase 2: fade in blur + piano UI over 0.5s
        if (pianoGameManager != null)
            pianoGameManager.FadeInPianoUI();

        // Wait for fade+UI to fully appear before starting the game
        yield return new WaitForSeconds(0.6f);

        // Phase 3: start the piano game
        if (pianoGamePlay != null)
            pianoGamePlay.StartGame();
    }

    private void OnPerformanceComplete()
    {
        Debug.Log("[Level1] Performance complete!");
    }

    private void OnFinishClicked()
    {
        // Close piano UI and restore player first
        if (pianoGameManager != null)
            pianoGameManager.ClosePiano();
        StartCoroutine(EndingSequence());
    }

    private System.Collections.IEnumerator EndingSequence()
    {
        yield return new WaitForSeconds(0.5f); // Wait for piano close to complete
        yield return new WaitForSeconds(3f);
        if (endingDirector != null)
            endingDirector.PlayEnding();
    }

    private void OnPianoClosed()
    {
        // Stop the piano game
        if (pianoGamePlay != null) pianoGamePlay.StopGame();

        // Restore standing animation
        if (animator != null)
            animator.CrossFade("Idle Walk Run Blend", 0.3f);

        // Restore collision and position
        if (cc != null) cc.enabled = false;
        player.transform.position = returnPosition;
        player.transform.rotation = Quaternion.identity;
        var armature = player.transform.Find("PlayerArmature");
        if (armature != null)
        {
            armature.localPosition = Vector3.zero;
            armature.localRotation = Quaternion.identity;
        }
        if (cc != null) cc.enabled = true;

        // Restore player controls
        if (tpc != null) tpc.enabled = true;
        if (inputs != null) { inputs.cursorLocked = true; inputs.cursorInputForLook = true; }
        if (playerInput != null) playerInput.enabled = true;

        // Hide cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // ===== UPDATE =====

    private void Update()
    {
        if (currentPhase == Phase.Reading)
        {
            HandleReadingUpdate();
            return;
        }

        if (waitingForF && Input.GetKeyDown(KeyCode.F))
        {
            HandleFInteraction();
        }
    }

    private void HandleFInteraction()
    {
        if (currentPhase == Phase.ApproachPiano)
        {
            PianoInteracted();
        }
        else if (currentPhase == Phase.ApproachBenchReading)
        {
            EnterReadingMode();
        }
        else if (currentPhase == Phase.Exploration)
        {
            // MP3 easter egg (not counted in cubes)
            if (activeTrigger == mp3Trigger)
            {
                MP3Interacted();
                return;
            }
            // Find which cube trigger is active
            for (int i = 0; i < cubeTriggers.Length; i++)
            {
                if (activeTrigger == cubeTriggers[i])
                {
                    CubeInteracted(i);
                    return;
                }
            }
        }
        else if (currentPhase == Phase.ApproachPerformance)
        {
            StartPerformance();
        }
    }

    private void MP3Interacted()
    {
        waitingForF = false;
        activeTrigger = null;
        if (mp3Effect != null) mp3Effect.SetActive(false);
        if (mp3Billboard != null) mp3Billboard.SetActive(false);
        DisableTrigger(mp3Trigger);

        if (dialogueController != null && mp3Dialogues != null && mp3Dialogues.Length > 0)
            dialogueController.StartDialogue(new System.Collections.Generic.List<string>(mp3Dialogues));
    }

    // ===== HELPERS =====

    private void HideAllEffects()
    {
        if (pianoShineEffect != null) pianoShineEffect.SetActive(false);
        if (areaStarEffect != null) areaStarEffect.SetActive(false);
        if (mp3Effect != null) mp3Effect.SetActive(false);
        if (benchSparkleEffect != null) benchSparkleEffect.SetActive(false);
        if (cubeEffects != null)
            foreach (var fx in cubeEffects) if (fx != null) fx.SetActive(false);
    }

    private void DisableAllBillboards()
    {
        if (pianoBillboard != null) pianoBillboard.SetActive(false);
        if (performanceBillboard != null) performanceBillboard.SetActive(false);
        if (mp3Billboard != null) mp3Billboard.SetActive(false);
        if (readBillboard != null) readBillboard.SetActive(false);
        if (cubeBillboards != null)
            foreach (var b in cubeBillboards) if (b != null) b.SetActive(false);
    }

    private void EnableTrigger(TriggerDetector td, System.Action<GameObject> onEnter, System.Action<GameObject> onExit)
    {
        if (td == null) return;
        td.onTriggerEnter.RemoveAllListeners();
        td.onTriggerExit.RemoveAllListeners();
        td.onTriggerEnter.AddListener((go) => onEnter?.Invoke(go));
        td.onTriggerExit.AddListener((go) => onExit?.Invoke(go));
        td.gameObject.SetActive(true);
    }

    private void DisableTrigger(TriggerDetector td)
    {
        if (td == null) return;
        td.onTriggerEnter.RemoveAllListeners();
        td.onTriggerExit.RemoveAllListeners();
    }

    private void DisableAllTriggers()
    {
        DisableTrigger(pianoTrigger);
        DisableTrigger(mp3Trigger);
        if (cubeTriggers != null)
            foreach (var t in cubeTriggers) DisableTrigger(t);
        DisableTrigger(performanceTrigger);
    }

    // ===== AUTO-CREATION (runtime setup) =====

    private void AutoCreateBillboards()
    {
        // Bench reading billboard
        if (readBillboard == null)
            readBillboard = GameObject.Find("ReadBillboard_0") ?? CreateRuntimeBillboard("ReadBillboard_0", "按下f阅读", new Vector3(0, 1.5f, 0));

        // Find existing scene objects first; only create if missing
        if (pianoBillboard == null)
        {
            pianoBillboard = GameObject.Find("PianoBillboard");
            if (pianoBillboard == null)
                pianoBillboard = CreateRuntimeBillboard("PianoBillboard", "奇怪的钢琴", new Vector3(-1f, 1.1f, -0.67f));
        }
        if (performanceBillboard == null)
        {
            performanceBillboard = GameObject.Find("PerformanceBillboard");
            if (performanceBillboard == null)
                performanceBillboard = CreateRuntimeBillboard("PerformanceBillboard", "按下f演奏", new Vector3(-1f, 1.8f, -0.7f));
        }

        if (cubeBillboards == null || cubeBillboards.Length < 4 || cubeBillboards[0] == null)
        {
            cubeBillboards = new GameObject[4];
            cubeBillboards[0] = GameObject.Find("CubeBillboard_0") ?? CreateRuntimeBillboard("CubeBillboard_0", "按下f收集碎片", new Vector3(0.084f, 0.3f, -1.023f));
            cubeBillboards[1] = GameObject.Find("CubeBillboard_1") ?? CreateRuntimeBillboard("CubeBillboard_1", "按下f收集碎片", new Vector3(-1.547f, 0.3f, 2.053f));
            cubeBillboards[2] = GameObject.Find("CubeBillboard_2") ?? CreateRuntimeBillboard("CubeBillboard_2", "按下f收集碎片", new Vector3(2.2f, 1.2f, 2.864f));
            cubeBillboards[3] = GameObject.Find("CubeBillboard_3") ?? CreateRuntimeBillboard("CubeBillboard_3", "按下f收集碎片", new Vector3(4.496f, 0.3f, 1.946f));
        }
    }

    private GameObject CreateRuntimeBillboard(string name, string text, Vector3 pos)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 80);
        go.transform.localScale = new Vector3(0.008f, 0.008f, 0.008f);
        go.AddComponent<Billboard>();

        var tgo = new GameObject("Txt");
        tgo.transform.SetParent(go.transform, false);
        var trt = tgo.AddComponent<RectTransform>();
        trt.sizeDelta = new Vector2(380, 60);
        var txt = tgo.AddComponent<Text>();
        txt.text = text; txt.fontSize = 32; txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var ol = tgo.AddComponent<Outline>();
        ol.effectColor = new Color(0, 0, 0, 0.8f);
        ol.effectDistance = new Vector2(2, -2);

        go.SetActive(false);
        return go;
    }

    private void AutoCreateCounter()
    {
        if (explorationCounterUI != null) return;

        var canvasGo = GameObject.Find("Canvas");
        if (canvasGo == null) return;

        explorationCounterUI = new GameObject("ExplorationCounter");
        explorationCounterUI.transform.SetParent(canvasGo.transform, false);
        var rt = explorationCounterUI.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f); rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f); rt.anchoredPosition = new Vector2(30, 0);
        rt.sizeDelta = new Vector2(420, 50);

        counterText = explorationCounterUI.AddComponent<Text>();
        counterText.text = "根据指引探索，找到过去的碎片 0/4";
        counterText.fontSize = 24; counterText.color = Color.white;
        counterText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        counterText.alignment = TextAnchor.MiddleLeft;

        var ol = explorationCounterUI.AddComponent<Outline>();
        ol.effectColor = new Color(0, 0, 0, 0.8f);
        ol.effectDistance = new Vector2(2, -2);

        explorationCounterUI.SetActive(false);
    }

    private void AutoCreateGoToPianoUI()
    {
        if (goToPianoUI != null) return;

        var canvasGo = GameObject.Find("Canvas");
        if (canvasGo == null) return;

        goToPianoUI = new GameObject("GoToPianoHint");
        goToPianoUI.transform.SetParent(canvasGo.transform, false);
        var rt = goToPianoUI.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f); rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f); rt.anchoredPosition = new Vector2(30, 0);
        rt.sizeDelta = new Vector2(300, 50);

        var txt = goToPianoUI.AddComponent<Text>();
        txt.text = "前往钢琴处";
        txt.fontSize = 26; txt.color = Color.white;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = TextAnchor.MiddleLeft;

        var ol = goToPianoUI.AddComponent<Outline>();
        ol.effectColor = new Color(0, 0, 0, 0.8f);
        ol.effectDistance = new Vector2(2, -2);

        goToPianoUI.SetActive(false);
    }

    private void AutoCreateTriggers()
    {
        CreateTriggerIfNeeded("SceneObject/Piano", pianoTriggerRadius, ref pianoTrigger);
        CreateTriggerIfNeeded("Area_star_ellow", performanceTriggerRadius, ref performanceTrigger);
        CreateTriggerIfNeeded("MP3", cubeTriggerRadius, ref mp3Trigger);
        CreateTriggerIfNeeded("Bench_Piano", cubeTriggerRadius, ref benchReadTrigger);

        if (cubeTriggers == null || cubeTriggers.Length < 4 || cubeTriggers[0] == null)
        {
            cubeTriggers = new TriggerDetector[4];
            CreateTriggerIfNeeded("ObjectToFind/Cube1", cubeTriggerRadius, ref cubeTriggers[0]);
            CreateTriggerIfNeeded("ObjectToFind/Cube2", cubeTriggerRadius, ref cubeTriggers[1]);
            CreateTriggerIfNeeded("ObjectToFind/Cube3", cubeTriggerRadius, ref cubeTriggers[2]);
            CreateTriggerIfNeeded("ObjectToFind/Cube4", cubeTriggerRadius, ref cubeTriggers[3]);
        }
    }

    private void CreateTriggerIfNeeded(string path, float radius, ref TriggerDetector td)
    {
        var go = GameObject.Find(path);
        if (go == null) return;

        var col = go.GetComponent<SphereCollider>();
        if (col == null)
        {
            col = go.AddComponent<SphereCollider>();
            col.radius = radius;
        }
        col.isTrigger = true;
        // Respect Editor-set radius — never override

        if (td == null) td = go.GetComponent<TriggerDetector>();
        if (td == null) td = go.AddComponent<TriggerDetector>();
    }

    private void AutoWireReferences()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (benchSitPosition == null)
        {
            var bench = GameObject.Find("SceneObject/Bench");
            if (bench != null) benchSitPosition = bench.transform.Find("SitPosition");
        }

        if (pianoShineEffect == null)
            pianoShineEffect = GameObject.Find("SceneObject/Piano/Shine_ellow");

        if (areaStarEffect == null)
            areaStarEffect = GameObject.Find("Area_star_ellow");

        if (dialogueController == null)
        {
            var c = GameObject.Find("Canvas");
            if (c != null) dialogueController = c.GetComponent<SceneIntroDialogueController>();
        }

        if (cubeEffects == null || cubeEffects.Length < 4 || cubeEffects[0] == null)
        {
            cubeEffects = new GameObject[4];
            cubeEffects[0] = SafeFind("ObjectToFind/Cube1/Sparkle_ellow");
            cubeEffects[1] = SafeFind("ObjectToFind/Cube2/Sparkle_ellow");
            cubeEffects[2] = SafeFind("ObjectToFind/Cube3/Sparkle_ellow");
            cubeEffects[3] = SafeFind("ObjectToFind/Cube4/Sparkle_ellow");
        }

        if (mp3Effect == null)
            mp3Effect = GameObject.Find("ForMP3/MP3/Sparkle_ellow");
        if (mp3Billboard == null)
            mp3Billboard = GameObject.Find("MP3Billboard") ?? CreateRuntimeBillboard("MP3Billboard", "按下f查看", new Vector3(-0.46f, 2f, 0.78f));

        if (pianoGameManager == null)
        {
            var pm = GameObject.Find("PianoGameManager");
            if (pm != null) pianoGameManager = pm.GetComponent<PianoGameManager>();
        }
        if (pianoGamePlay == null)
        {
            var pg = GameObject.Find("PianoGameManager");
            if (pg != null) pianoGamePlay = pg.GetComponent<PianoGamePlay>();
        }
        if (pianoGamePlay != null)
        {
            pianoGamePlay.onSequenceComplete.AddListener(OnPerformanceComplete);
            pianoGamePlay.onFinishClicked.AddListener(OnFinishClicked);
        }
        if (endingDirector == null)
        {
            var edGo = GameObject.Find("Level1EndingDirector");
            if (edGo != null) endingDirector = edGo.GetComponent<Level1EndingDirector>();
        }

        // Bench reading UI
        if (readTextUI == null) readTextUI = CreateSideTextUI("ReadText", "阅读", -20);
        if (exitReadUI == null) exitReadUI = CreateSideTextUI("ExitRead", "按F退出阅读", -20);

        // Bench reading
        if (benchSparkleEffect == null)
            benchSparkleEffect = GameObject.Find("Bench_Piano/Sparkle_ellow");
        if (allTextBench == null)
        {
            var atb = GameObject.Find("Bench_Piano/All_Text_bench");
            if (atb != null) allTextBench = atb.transform;
        }
        if (allTextPiano == null)
        {
            var atp = GameObject.Find("Bench_Piano/All_Text_Piano");
            if (atp != null) allTextPiano = atp.transform;
            if (allTextPiano != null) allTextPiano.gameObject.SetActive(false);
        }
        if (readingVCam == null)
        {
            var rv = GameObject.Find("All_Text_bench Virtual Camera");
            if (rv != null) readingVCam = rv.GetComponent<CinemachineVirtualCamera>();
        }
    }

    private void SetupExistingBillboards()
    {
        var cam = Camera.main;
        void Config(GameObject go) {
            if (go == null) return;
            var c = go.GetComponent<Canvas>();
            if (c != null) { c.renderMode = RenderMode.WorldSpace; c.worldCamera = cam; }
        }
        Config(pianoBillboard);
        Config(performanceBillboard);
        if (cubeBillboards != null)
            foreach (var b in cubeBillboards) Config(b);
    }

    private GameObject CreateSideTextUI(string name, string text, float yOffset)
    {
        var canvasGo = GameObject.Find("Canvas");
        if (canvasGo == null) return null;
        var go = new GameObject(name);
        go.transform.SetParent(canvasGo.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f); rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f); rt.anchoredPosition = new Vector2(30, yOffset);
        rt.sizeDelta = new Vector2(300, 40);
        var txt = go.AddComponent<Text>();
        txt.text = text; txt.fontSize = 22; txt.color = Color.white;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.alignment = TextAnchor.MiddleLeft;
        go.SetActive(false);
        return go;
    }

    private GameObject SafeFind(string path)
    {
        var go = GameObject.Find(path);
        return go != null ? go : null;
    }
}
