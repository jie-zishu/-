using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 玩家交互处理器
/// 附加到StarterAssets的玩家角色上，处理交互输入
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Instance { get; private set; }

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private KeyCode interactionKey = KeyCode.F;

    [Header("UI Prompt")]
    [SerializeField] private GameObject interactionPromptUI;

    [Header("Events")]
    public UnityEvent OnInteract;

    private InteractableObject currentInteractable;
    private bool canInteract = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (OnInteract == null) OnInteract = new UnityEvent();
    }

    private void Update()
    {
        if (!canInteract) return;

        // 检查游戏状态
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        // 检测附近可交互物体
        DetectInteractable();

        // 检测交互输入
        if (Input.GetKeyDown(interactionKey))
        {
            TryInteract();
        }
    }

    private void DetectInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange, interactableLayer);

        if (hits.Length > 0)
        {
            // 找到最近的可交互物体
            float closestDistance = float.MaxValue;
            InteractableObject closestInteractable = null;

            foreach (var hit in hits)
            {
                InteractableObject interactable = hit.GetComponent<InteractableObject>();
                if (interactable != null)
                {
                    float distance = Vector3.Distance(transform.position, hit.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInteractable = interactable;
                    }
                }
            }

            // 更新当前可交互物体
            if (closestInteractable != currentInteractable)
            {
                if (currentInteractable != null)
                {
                    currentInteractable.SetHighlight(false);
                }

                currentInteractable = closestInteractable;

                if (currentInteractable != null)
                {
                    currentInteractable.SetHighlight(true);
                }
            }

            // 显示提示UI
            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(currentInteractable != null);
            }
        }
        else
        {
            // 没有可交互物体
            if (currentInteractable != null)
            {
                currentInteractable.SetHighlight(false);
                currentInteractable = null;
            }

            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(false);
            }
        }
    }

    private void TryInteract()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
            OnInteract?.Invoke();
            Debug.Log($"[PlayerInteraction] Interacted with {currentInteractable.name}");
        }
    }

    /// <summary>
    /// 设置是否可以交互
    /// </summary>
    public void SetCanInteract(bool value)
    {
        canInteract = value;
    }

    /// <summary>
    /// 获取当前可交互物体
    /// </summary>
    public InteractableObject GetCurrentInteractable()
    {
        return currentInteractable;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
