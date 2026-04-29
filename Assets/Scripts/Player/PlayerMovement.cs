using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家移动组件 - 附加的移动功能
/// 可用于实现冲刺、跳跃等
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private bool canJump = true;

    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private bool canDash = true;

    private CharacterController controller;
    private PlayerController playerController;
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (canJump && Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        if (canDash && Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && dashCooldownTimer <= 0)
        {
            Dash();
        }
    }

    private void Jump()
    {
        // 需要配合PlayerController使用
        // 这里只是示例，实际跳跃逻辑应该在PlayerController中
    }

    private void Dash()
    {
        if (isDashing) return;

        StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;

        Vector3 startPos = transform.position;
        Vector3 dashDirection = playerController != null ? playerController.GetMoveDirection() : transform.forward;

        if (dashDirection == Vector3.zero)
        {
            dashDirection = transform.forward;
        }

        Vector3 endPos = startPos + dashDirection * dashDistance;

        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dashDuration;

            if (controller != null)
            {
                controller.Move(dashDirection * (dashDistance / dashDuration) * Time.deltaTime);
            }

            yield return null;
        }

        isDashing = false;
    }

    public bool IsDashing()
    {
        return isDashing;
    }
}
