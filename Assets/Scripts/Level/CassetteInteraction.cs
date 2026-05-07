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

        RebuildTapeStrip();
    }

    /// <summary>
    /// Reads child objects named "TapePoint_*" under this GameObject as control points,
    /// plus the fixed endpoints stripPoint1 (start) and stripPoint2 (end).
    /// Call this after adding/removing/moving control points in the Editor.
    /// </summary>
    [ContextMenu("Rebuild Tape Strip")]
    public void RebuildTapeStrip()
    {
        if (tapeLine == null || stripPoint1 == null || stripPoint2 == null) return;

        // Collect child control points sorted by name
        var controlPoints = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.name.StartsWith("TapePoint_") && child != stripPoint1 && child != stripPoint2)
                controlPoints.Add(child);
        }
        controlPoints.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

        // Build final point list: stripPoint1 → control points → stripPoint2
        int totalPoints = 2 + controlPoints.Count;
        tapeLine.positionCount = totalPoints;
        tapeLine.startWidth = stripWidth;
        tapeLine.endWidth = stripWidth;

        tapeLine.SetPosition(0, stripPoint1.localPosition);
        for (int i = 0; i < controlPoints.Count; i++)
            tapeLine.SetPosition(i + 1, controlPoints[i].localPosition);
        tapeLine.SetPosition(totalPoints - 1, stripPoint2.localPosition);
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

            // Build per-frame rotation from mouse delta
            Quaternion rotH = Quaternion.AngleAxis(delta.x * rotationSensitivity, cam.transform.up);
            Quaternion rotV = Quaternion.AngleAxis(-delta.y * rotationSensitivity, cam.transform.right);
            Quaternion frameRot = rotH * rotV;

            // Rotate position around pivot, then rotate orientation
            Vector3 toObj = transform.position - pivotPos;
            toObj = frameRot * toObj;
            transform.position = pivotPos + toObj;
            transform.rotation = frameRot * transform.rotation;
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
