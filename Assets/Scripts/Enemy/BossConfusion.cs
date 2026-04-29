using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// "迷茫" Boss - 关卡3的Boss
/// 拥有独特的AI行为和多阶段战斗
/// </summary>
public class BossConfusion : Boss
{
    public enum BossPhase
    {
        Phase1,     // 第一阶段：普通攻击
        Phase2,     // 第二阶段：增强攻击
        Phase3      // 第三阶段：狂暴模式
    }

    [Header("Phase Settings")]
    [SerializeField] private BossPhase currentPhase = BossPhase.Phase1;

    [Header("Phase 1 Settings")]
    [SerializeField] private float phase1AttackCooldown = 3f;

    [Header("Phase 2 Settings")]
    [SerializeField] private int phase2HealthThreshold = 300; // 血量低于此值进入第二阶段
    [SerializeField] private float phase2AttackCooldown = 2f;
    [SerializeField] private int phase2Damage = 25;

    [Header("Phase 3 Settings")]
    [SerializeField] private int phase3HealthThreshold = 100; // 血量低于此值进入第三阶段
    [SerializeField] private float phase3AttackCooldown = 1.5f;
    [SerializeField] private int phase3Damage = 35;

    [Header("Special Abilities")]
    [SerializeField] private GameObject confusionProjectile;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileSpeed = 10f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float chaseRange = 15f;
    [SerializeField] private float stopDistance = 3f;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem phaseTransitionEffect;
    [SerializeField] private Renderer bossRenderer;
    [SerializeField] private Color phase1Color = Color.blue;
    [SerializeField] private Color phase2Color = Color.magenta;
    [SerializeField] private Color phase3Color = Color.red;

    private UnityEngine.AI.NavMeshAgent agent; // 如果使用NavMesh
    private bool isChasing = false;

    protected override void Awake()
    {
        base.Awake();
        bossName = "迷茫";
        maxHealth = 500;
        currentHealth = maxHealth;
        attackCooldown = phase1AttackCooldown;
    }

    protected override void Start()
    {
        base.Start();

        // 尝试获取NavMeshAgent（如果使用）
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        UpdateBossAppearance();
    }

    protected override void Update()
    {
        if (!isActive || isDefeated) return;

        base.Update();

        // 追逐玩家
        ChasePlayer();
    }

    /// <summary>
    /// 追逐玩家
    /// </summary>
    private void ChasePlayer()
    {
        if (player == null) return;

        float distanceToPlayer = GetDistanceToPlayer();

        // 在追逐范围内且不在攻击距离内
        if (distanceToPlayer <= chaseRange && distanceToPlayer > stopDistance)
        {
            isChasing = true;

            if (agent != null)
            {
                agent.SetDestination(player.position);
            }
            else
            {
                // 简单的朝向玩家移动
                Vector3 direction = (player.position - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;
                transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
            }
        }
        else
        {
            isChasing = false;
            if (agent != null)
            {
                agent.ResetPath();
            }
        }
    }

    /// <summary>
    /// 检查阶段转换
    /// </summary>
    protected override void CheckPhaseTransition()
    {
        BossPhase newPhase = currentPhase;

        if (currentHealth <= phase3HealthThreshold && currentPhase != BossPhase.Phase3)
        {
            newPhase = BossPhase.Phase3;
        }
        else if (currentHealth <= phase2HealthThreshold && currentPhase == BossPhase.Phase1)
        {
            newPhase = BossPhase.Phase2;
        }

        if (newPhase != currentPhase)
        {
            TransitionToPhase(newPhase);
        }
    }

    /// <summary>
    /// 转换到新阶段
    /// </summary>
    private void TransitionToPhase(BossPhase newPhase)
    {
        currentPhase = newPhase;

        // 更新属性
        switch (newPhase)
        {
            case BossPhase.Phase1:
                attackCooldown = phase1AttackCooldown;
                attackDamage = 20;
                break;
            case BossPhase.Phase2:
                attackCooldown = phase2AttackCooldown;
                attackDamage = phase2Damage;
                break;
            case BossPhase.Phase3:
                attackCooldown = phase3AttackCooldown;
                attackDamage = phase3Damage;
                moveSpeed *= 1.3f; // 狂暴模式移动更快
                break;
        }

        // 播放阶段转换特效
        if (phaseTransitionEffect != null)
        {
            phaseTransitionEffect.Play();
        }

        UpdateBossAppearance();

        Debug.Log($"[BossConfusion] Entered {newPhase}!");
    }

    /// <summary>
    /// 更新Boss外观
    /// </summary>
    private void UpdateBossAppearance()
    {
        if (bossRenderer == null) return;

        Color targetColor = currentPhase switch
        {
            BossPhase.Phase1 => phase1Color,
            BossPhase.Phase2 => phase2Color,
            BossPhase.Phase3 => phase3Color,
            _ => phase1Color
        };

        bossRenderer.material.color = targetColor;
    }

    /// <summary>
    /// 攻击（重写）
    /// </summary>
    protected override void Attack()
    {
        if (!CanAttack()) return;

        lastAttackTime = Time.time;

        // 根据阶段选择攻击方式
        switch (currentPhase)
        {
            case BossPhase.Phase1:
                NormalAttack();
                break;
            case BossPhase.Phase2:
                EnhancedAttack();
                break;
            case BossPhase.Phase3:
                BerserkAttack();
                break;
        }

        OnAttack?.Invoke(attackCooldown);
    }

    /// <summary>
    /// 普通攻击
    /// </summary>
    private void NormalAttack()
    {
        // 近战攻击
        if (PlayerHealth.Instance != null && GetDistanceToPlayer() <= attackRange)
        {
            PlayerHealth.Instance.TakeDamage(attackDamage);
        }

        Debug.Log("[BossConfusion] Normal Attack!");
    }

    /// <summary>
    /// 增强攻击
    /// </summary>
    private void EnhancedAttack()
    {
        // 近战攻击 + 投射物
        NormalAttack();

        if (confusionProjectile != null && projectileSpawnPoint != null)
        {
            LaunchProjectile();
        }

        Debug.Log("[BossConfusion] Enhanced Attack!");
    }

    /// <summary>
    /// 狂暴攻击
    /// </summary>
    private void BerserkAttack()
    {
        // 多重攻击
        EnhancedAttack();

        // 额外伤害
        if (PlayerHealth.Instance != null && GetDistanceToPlayer() <= attackRange)
        {
            PlayerHealth.Instance.TakeDamage(10); // 额外伤害
        }

        Debug.Log("[BossConfusion] Berserk Attack!");
    }

    /// <summary>
    /// 发射投射物
    /// </summary>
    private void LaunchProjectile()
    {
        if (player == null) return;

        GameObject projectile = Instantiate(confusionProjectile, projectileSpawnPoint.position, Quaternion.identity);

        Vector3 direction = (player.position - projectileSpawnPoint.position).normalized;

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = direction * projectileSpeed;
        }

        // 设置投射物伤害
        BossProjectile bossProjectile = projectile.GetComponent<BossProjectile>();
        if (bossProjectile != null)
        {
            bossProjectile.SetDamage(attackDamage);
        }

        Debug.Log("[BossConfusion] Launched projectile!");
    }

    /// <summary>
    /// 开始战斗（重写）
    /// </summary>
    public override void StartBattle()
    {
        base.StartBattle();
        currentPhase = BossPhase.Phase1;
        UpdateBossAppearance();
    }

    /// <summary>
    /// 重置Boss（重写）
    /// </summary>
    public override void ResetBoss()
    {
        base.ResetBoss();
        currentPhase = BossPhase.Phase1;
        UpdateBossAppearance();
    }

    /// <summary>
    /// 获取当前阶段
    /// </summary>
    public BossPhase GetCurrentPhase()
    {
        return currentPhase;
    }
}

/// <summary>
/// Boss投射物组件
/// </summary>
public class BossProjectile : MonoBehaviour
{
    [SerializeField] private int damage = 20;
    [SerializeField] private float lifetime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDamage(int damageAmount)
    {
        damage = damageAmount;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}
