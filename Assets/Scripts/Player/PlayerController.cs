using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 玩家控制器 - 处理玩家输入和基础控制
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactionLayer;

    [Header("Events")]
    public UnityEvent OnInteract;
    public UnityEvent<Vector3> OnMove;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;
    private Vector3 lastMoveDirection;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        controller = GetComponent<CharacterController>();
        currentSpeed = moveSpeed;

        if (OnInteract == null) OnInteract = new UnityEvent();
        if (OnMove == null) OnMove = new UnityEvent<Vector3>();
    }

    private void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        HandleMovement();
        HandleInteraction();
    }

    private void HandleMovement()
    {
        // 地面检测
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // 获取输入
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 计算移动方向
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;

        // 检测冲刺
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;

        // 应用移动
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        // 记录移动方向
        if (moveDirection.magnitude > 0.1f)
        {
            lastMoveDirection = moveDirection.normalized;
            OnMove?.Invoke(moveDirection);
        }

        // 应用重力
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        // 射线检测可交互物体
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange, interactionLayer);

        if (hits.Length > 0)
        {
            IInteractable interactable = hits[0].GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
                OnInteract?.Invoke();
                Debug.Log("[Player] Interacted with " + hits[0].name);
            }
        }
    }

    /// <summary>
    /// 设置玩家位置
    /// </summary>
    public void SetPosition(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position;
        controller.enabled = true;
    }

    /// <summary>
    /// 设置玩家旋转
    /// </summary>
    public void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    /// <summary>
    /// 获取玩家位置
    /// </summary>
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    /// <summary>
    /// 获取移动方向
    /// </summary>
    public Vector3 GetMoveDirection()
    {
        return lastMoveDirection;
    }

    /// <summary>
    /// 获取当前速度
    /// </summary>
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

/// <summary>
/// 可交互物体接口
/// </summary>
public interface IInteractable
{
    void Interact();
}
