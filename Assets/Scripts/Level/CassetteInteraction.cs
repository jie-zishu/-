using System.Collections;
using UnityEngine;
using Cinemachine;
using StarterAssets;

public class CassetteInteraction : MonoBehaviour
{
    [Header("Float Animation")]
    [SerializeField] private float floatHeight = 0.5f;
    [SerializeField] private float floatDuration = 0.8f;

    [Header("Rotation")]
    [SerializeField] private float rotationSensitivity = 0.28f;

    [Header("Cinematic Camera")]
    [SerializeField] private float vcamPriority = 20;

    [Header("Tape Strip Visual (绞出的磁带条)")]
    [SerializeField] private Transform stripPoint1;
    [SerializeField] private Transform stripPoint2;
    [SerializeField] private float stripWidth = 0.015f;
    [SerializeField] private Color stripColor = new Color(0.35f, 0.2f, 0.1f);

    [Header("Winding (回卷)")]
    [SerializeField] private UnityEngine.UI.Slider windingSlider;
    [SerializeField] private GameObject windingSliderUI;
    public UnityEngine.Events.UnityEvent onWindingComplete;

    [Header("Interaction Prompt")]
    [SerializeField] private GameObject interactPromptUI;

    // State
    private bool isInteracting;
    private bool playerInRange;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 floatTargetPos;

    // Mouse drag
    private bool isDragging;
    private Vector2 lastMousePos;

    // Winding state
    private int totalTapePoints;
    private int activePointCount;

    // References
    private ThirdPersonController playerController;
    private StarterAssetsInputs playerInputs;
    private UnityEngine.InputSystem.PlayerInput playerInputComp;
    private CharacterController playerCC;
    private LineRenderer tapeLine;
    private CinemachineVirtualCamera cassetteVCam;

    private void Start()
    {
        CachePlayerComponents();
        CreateCassetteVCam();
        CreateTapeStripVisual();
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
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
            playerCC = player.GetComponentInChildren<CharacterController>();
        }
    }

    private void CreateCassetteVCam()
    {
        // Find pre-placed VCam in scene (created in Editor)
        var vcamGo = GameObject.Find("CassetteVCam");
        if (vcamGo != null)
        {
            cassetteVCam = vcamGo.GetComponent<CinemachineVirtualCamera>();
            if (cassetteVCam != null)
                cassetteVCam.Priority = 0;
        }
    }

    private void CreateTapeStripVisual()
    {
        if (stripPoint1 == null || stripPoint2 == null) return;

        tapeLine = gameObject.AddComponent<LineRenderer>();
        tapeLine.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        tapeLine.material.color = stripColor;
        tapeLine.useWorldSpace = false;
        tapeLine.numCornerVertices = 4;
        tapeLine.numCapVertices = 4;

        RebuildTapeStripAll();
    }

    /// <summary>
    /// Rebuild with ALL control points (Editor use).
    /// </summary>
    [ContextMenu("Rebuild Tape Strip")]
    public void RebuildTapeStripAll()
    {
        var allPoints = GetSortedTapePoints();
        totalTapePoints = allPoints.Count;
        activePointCount = totalTapePoints;
        UpdateTapeLine(allPoints.Count);
    }

    /// <summary>
    /// Reads child objects named "TapePoint_*" sorted by name.
    /// </summary>
    private System.Collections.Generic.List<Transform> GetSortedTapePoints()
    {
        var points = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.name.StartsWith("TapePoint_") && child != stripPoint1 && child != stripPoint2)
                points.Add(child);
        }
        points.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
        return points;
    }

    /// <summary>
    /// Rebuild LineRenderer with only `count` active control points (winding effect).
    /// </summary>
    private void UpdateTapeLine(int activeCount)
    {
        if (tapeLine == null || stripPoint1 == null || stripPoint2 == null) return;

        var allPoints = GetSortedTapePoints();
        int visible = Mathf.Clamp(activeCount, 0, allPoints.Count);

        tapeLine.positionCount = 2 + visible;
        tapeLine.startWidth = stripWidth;
        tapeLine.endWidth = stripWidth;

        tapeLine.SetPosition(0, stripPoint1.localPosition);
        for (int i = 0; i < visible; i++)
            tapeLine.SetPosition(i + 1, allPoints[i].localPosition);
        tapeLine.SetPosition(1 + visible, stripPoint2.localPosition);
    }

    private void Update()
    {
        if (!isInteracting)
        {
            if (playerInRange && Input.GetKeyDown(KeyCode.F))
            {
                StartCoroutine(InteractionSequence());
            }
            return;
        }

        HandleMouseDrag();

        // Slider-driven winding
        if (windingSlider != null)
        {
            int targetCount = Mathf.RoundToInt((1f - windingSlider.value) * totalTapePoints);
            if (targetCount != activePointCount)
            {
                activePointCount = targetCount;
                UpdateTapeLine(activePointCount);
                if (activePointCount <= 0) onWindingComplete?.Invoke();
            }
        }
    }

    /// <summary>
    /// Called by winding slider's OnValueChanged.
    /// </summary>
    public void OnWindingSliderChanged(float value)
    {
        // Handled in Update
    }

    private void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastMousePos;
            lastMousePos = Input.mousePosition;

            Camera cam = Camera.main;
            Transform pivot = transform.Find("Center");
            Vector3 pivotPos = pivot != null ? pivot.position : transform.position;

            // 通过鼠标移动构建每帧的旋转
            Quaternion rotH = Quaternion.AngleAxis(delta.x * rotationSensitivity, cam.transform.up);
            Quaternion rotV = Quaternion.AngleAxis(-delta.y * rotationSensitivity, cam.transform.right);
            Quaternion frameRot = rotH * rotV;

            // 绕中心
            Vector3 toObj = transform.position - pivotPos;
            toObj = frameRot * toObj;
            transform.position = pivotPos + toObj;
            transform.rotation = frameRot * transform.rotation;

            // No winding here — handled by slider in WindingSliderUpdate()
        }

        // Exit with ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(ExitInteraction());
        }
    }

    private IEnumerator InteractionSequence()
    {
        isInteracting = true;
        playerInRange = false;
        if (interactPromptUI != null) interactPromptUI.SetActive(false);

        // Disable player
        if (playerController != null) playerController.enabled = false;
        if (playerInputs != null) { playerInputs.cursorLocked = false; playerInputs.cursorInputForLook = false; }
        if (playerInputComp != null) playerInputComp.enabled = false;
        if (playerCC != null) playerCC.enabled = false;

        // Show cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Store original state
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        floatTargetPos = originalPosition + Vector3.up * floatHeight;

        // Switch to cassette camera — Cinemachine Brain blends automatically
        if (cassetteVCam != null) cassetteVCam.Priority = (int)vcamPriority;

        // Show winding slider
        if (windingSliderUI != null) windingSliderUI.SetActive(true);
        if (windingSlider != null) windingSlider.value = 0f;

        // Float cassette up
        float elapsed = 0f;
        while (elapsed < floatDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / floatDuration;
            float ease = 1f - (1f - t) * (1f - t);
            transform.position = Vector3.Lerp(originalPosition, floatTargetPos, ease);
            yield return null;
        }
        transform.position = floatTargetPos;
    }

    private IEnumerator ExitInteraction()
    {
        isInteracting = false;
        isDragging = false;
        if (windingSliderUI != null) windingSliderUI.SetActive(false);

        // Switch back to player camera
        if (cassetteVCam != null) cassetteVCam.Priority = 0;

        // Float back down and restore rotation
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        while (elapsed < floatDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / floatDuration;
            float ease = 1f - (1f - t) * (1f - t);
            transform.position = Vector3.Lerp(startPos, originalPosition, ease);
            transform.rotation = Quaternion.Slerp(startRot, originalRotation, ease);
            yield return null;
        }
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        // Restore player
        if (playerController != null) playerController.enabled = true;
        if (playerInputs != null) { playerInputs.cursorLocked = true; playerInputs.cursorInputForLook = true; }
        if (playerInputComp != null) playerInputComp.enabled = true;
        if (playerCC != null) playerCC.enabled = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactPromptUI != null) interactPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactPromptUI != null) interactPromptUI.SetActive(false);
        }
    }
}
