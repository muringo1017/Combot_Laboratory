using UnityEngine;

/// <summary>
/// 근접 공격 전용 Enemy
/// </summary>
public class MeleeEnemy : BaseEnemy
{
    [Header("Melee Attack Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackCooldown = 1.5f;
    
    private float _lastAttackTime = 0f;
    
    protected override void Awake()
    {
        base.Awake();
        canShoot = false; // 근접 Enemy는 원거리 공격 불가
        Debug.Log($"[MeleeEnemy] Awake - canShoot={canShoot}");
    }
    
    public override float GetAttackRange()
    {
        return attackRange;
    }
    
    public override bool CanAttack()
    {
        // 쿨다운 확인
        return Time.time - _lastAttackTime >= attackCooldown;
    }
    
    public override void PerformAttack(Transform target)
    {
        if (!CanAttack() || target == null)
        {
            Debug.LogWarning("[MeleeEnemy] 공격 불가!");
            return;
        }
        
        Debug.Log($"[MeleeEnemy] 근접 공격 실행! 데미지: {attackDamage}");
        
        // 애니메이션 재생 (MyBT에서 처리)
        // animator.SetTrigger("Attack");
        
        // 공격 범위 내의 플레이어에게 데미지
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= attackRange)
        {
            var playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"[MeleeEnemy] Player에게 {attackDamage} 데미지!");
            }
        }
        
        _lastAttackTime = Time.time;
    }
}

