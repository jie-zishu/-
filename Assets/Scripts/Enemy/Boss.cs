using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Boss基类 - 所有Boss的父类
/// </summary>
public class Boss : MonoBehaviour
{
    [Header("Boss Info")]
    [SerializeField] protected string bossName = "Boss";
    [SerializeField] protected int maxHealth = 500;
    [SerializeField] protected int currentHealth;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    [Header("Combat Settings")]
    [SerializeField] protected float attackRange = 5f;
    [SerializeField] protected float attackCooldown = 2f;
    [SerializeField] protected int attackDamage = 20;

    [Header("State")]
    [SerializeField] protected bool isActive = false;
    [SerializeField] protected bool isDefeated = false;
    public bool IsDefeated => isDefeated;

    [Header("Events")]
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent OnBossDefeated;
    public UnityEvent OnBossDamaged;
    public UnityEvent<float> OnAttack;

    protected Transform player;
    protected float lastAttackTime = 0f;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;

        if (OnHealthChanged == null) OnHealthChanged = new UnityEvent<int>();
        if (OnBossDefeated == null) OnBossDefeated = new UnityEvent();
        if (OnBossDamaged == null) OnBossDamaged = new UnityEvent();
        if (OnAttack == null) OnAttack = new UnityEvent<float>();
    }

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    protected virtual void Update()
    {
        if (!isActive || isDefeated) return;

        // 检查玩家距离
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange && CanAttack())
            {
                Attack();
            }
        }
    }

    /// <summary>
    /// 开始战斗
    /// </summary>
    public virtual void StartBattle()
    {
        isActive = true;
        isDefeated = false;
        currentHealth = maxHealth;

        Debug.Log($"[Boss] {bossName} battle started!");
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    public virtual void TakeDamage(int damage)
    {
        if (!isActive || isDefeated) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        OnHealthChanged?.Invoke(currentHealth);
        OnBossDamaged?.Invoke();

        Debug.Log($"[Boss] {bossName} took {damage} damage. Health: {currentHealth}/{maxHealth}");

        // 血量低于特定百分比时触发阶段变化（子类可重写）
        CheckPhaseTransition();

        if (currentHealth <= 0)
        {
            Defeat();
        }
    }

    /// <summary>
    /// 检查阶段转换
    /// </summary>
    protected virtual void CheckPhaseTransition()
    {
        // 子类实现具体的阶段转换逻辑
    }

    /// <summary>
    /// 攻击
    /// </summary>
    protected virtual void Attack()
    {
        if (!CanAttack()) return;

        lastAttackTime = Time.time;

        // 对玩家造成伤害
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.TakeDamage(attackDamage);
        }

        OnAttack?.Invoke(attackCooldown);
        Debug.Log($"[Boss] {bossName} attacked for {attackDamage} damage!");
    }

    /// <summary>
    /// 检查是否可以攻击
    /// </summary>
    protected virtual bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    /// <summary>
    /// Boss被击败
    /// </summary>
    protected virtual void Defeat()
    {
        isDefeated = true;
        isActive = false;

        OnBossDefeated?.Invoke();

        // 通知关卡管理器
        Level03Manager levelManager = FindObjectOfType<Level03Manager>();
        if (levelManager != null)
        {
            levelManager.OnBossDefeated();
        }

        Debug.Log($"[Boss] {bossName} defeated!");
    }

    /// <summary>
    /// 重置Boss
    /// </summary>
    public virtual void ResetBoss()
    {
        isActive = false;
        isDefeated = false;
        currentHealth = maxHealth;
        lastAttackTime = 0f;
    }

    /// <summary>
    /// 获取血量百分比
    /// </summary>
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    /// <summary>
    /// 获取与玩家的距离
    /// </summary>
    protected float GetDistanceToPlayer()
    {
        if (player == null) return float.MaxValue;
        return Vector3.Distance(transform.position, player.position);
    }
}
