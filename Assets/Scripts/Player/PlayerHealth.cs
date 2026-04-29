using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 玩家生命值系统
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    [Header("Invincibility")]
    [SerializeField] private float invincibilityTime = 1f;
    [SerializeField] private bool isInvincible = false;

    [Header("Events")]
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent OnPlayerDeath;
    public UnityEvent OnPlayerDamaged;
    public UnityEvent OnPlayerHealed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        currentHealth = maxHealth;

        if (OnHealthChanged == null) OnHealthChanged = new UnityEvent<int>();
        if (OnPlayerDeath == null) OnPlayerDeath = new UnityEvent();
        if (OnPlayerDamaged == null) OnPlayerDamaged = new UnityEvent();
        if (OnPlayerHealed == null) OnPlayerHealed = new UnityEvent();
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isInvincible || currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        OnHealthChanged?.Invoke(currentHealth);
        OnPlayerDamaged?.Invoke();

        Debug.Log($"[PlayerHealth] Took {damage} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    /// <summary>
    /// 恢复生命值
    /// </summary>
    public void Heal(int healAmount)
    {
        if (currentHealth <= 0) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        OnHealthChanged?.Invoke(currentHealth);
        OnPlayerHealed?.Invoke();

        Debug.Log($"[PlayerHealth] Healed {healAmount}. Health: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// 玩家死亡
    /// </summary>
    private void Die()
    {
        Debug.Log("[PlayerHealth] Player died!");
        OnPlayerDeath?.Invoke();
        GameManager.Instance?.GameOver();
    }

    /// <summary>
    /// 无敌时间协程
    /// </summary>
    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityTime);
        isInvincible = false;
    }

    /// <summary>
    /// 重置生命值
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }

    /// <summary>
    /// 获取生命值百分比
    /// </summary>
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    /// <summary>
    /// 设置无敌状态
    /// </summary>
    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }
}
