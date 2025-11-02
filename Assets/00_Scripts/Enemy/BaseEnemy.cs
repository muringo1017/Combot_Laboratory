using UnityEngine;

/// <summary>
/// 모든 Enemy의 기본 클래스 - 공통 기능 제공
/// </summary>
public abstract class BaseEnemy : MonoBehaviour
{
    [Header("Base Enemy Settings")]
    [SerializeField] protected float maxHealth = 100f;
    protected float currentHealth;
    
    [Header("Movement Settings")]
    [SerializeField] protected float moveSpeed = 2.0f;
    [SerializeField] protected float chaseRange = 5.0f;
    
    [Header("Attack Type")]
    [SerializeField] protected bool canShoot = false; // 원거리 공격 가능 여부
    
    [Header("Animation/Death Settings")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected string deathTrigger = "Die";
    [SerializeField] protected float deathDestroyDelay = 10.0f;
    
    [Header("Stun Settings")]
    [SerializeField] protected float stunDurationOnHit = 0.3f;
    
    protected Rigidbody _rigidbody;
    protected Collider _collider;
    protected bool _isDead = false;
    protected bool _isStunned = false;
    protected float _stunTime = 0f;
    
    public float MoveSpeed => moveSpeed;
    public float ChaseRange => chaseRange;
    public bool IsDead => _isDead;
    public bool IsStunned => _isStunned;
    public Animator Animator => animator;
    public bool CanShoot => canShoot; // 원거리 공격 가능 여부
    
    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        
        // Enemy가 Player에 의해 밀리지 않도록 Rigidbody를 Kinematic으로 설정
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = true;
        }
    }
    
    protected virtual void Update()
    {
        // 스턴 타이머 업데이트
        if (_isStunned)
        {
            _stunTime -= Time.deltaTime;
            if (_stunTime <= 0f)
            {
                _isStunned = false;
                OnStunEnd();
            }
        }
    }
    
    /// <summary>
    /// 데미지를 받습니다
    /// </summary>
    public virtual void TakeDamage(float damage)
    {
        if (_isDead) return;
        
        currentHealth -= damage;
        
        Debug.Log($"[{GetType().Name}] 데미지 받음: {damage}, 남은 체력: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            OnDamaged(stunDurationOnHit);
        }
    }
    
    /// <summary>
    /// 피격 시 호출 (스턴 처리)
    /// </summary>
    protected virtual void OnDamaged(float stunDuration)
    {
        _isStunned = true;
        _stunTime = stunDuration > 0 ? stunDuration : 0.3f;
        
        if (animator != null)
        {
            animator.SetTrigger("Damaged");
        }
        
        Debug.Log($"[{GetType().Name}] 피격! 스턴 시간: {_stunTime}초");
    }
    
    /// <summary>
    /// 스턴이 끝났을 때 호출
    /// </summary>
    protected virtual void OnStunEnd()
    {
        Debug.Log($"[{GetType().Name}] 스턴 종료");
    }
    
    /// <summary>
    /// 사망 처리
    /// </summary>
    protected virtual void Die()
    {
        if (_isDead) return;
        
        _isDead = true;
        
        Debug.Log($"[{GetType().Name}] 사망!");
        
        // Rigidbody kinematic 활성화
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = true;
        }
        
        // Collider 비활성화
        if (_collider != null)
        {
            _collider.enabled = false;
        }
        
        // 무적 레이어로 변경
        int invincibleLayer = LayerMask.NameToLayer("Invincible");
        if (invincibleLayer != -1)
        {
            gameObject.layer = invincibleLayer;
        }
        
        // 사망 애니메이션
        if (animator != null && !string.IsNullOrEmpty(deathTrigger))
        {
            animator.SetTrigger(deathTrigger);
        }
        
        // 일정 시간 후 파괴
        Destroy(gameObject, deathDestroyDelay);
    }
    
    /// <summary>
    /// 공격 실행 - 각 Enemy 타입에서 구현
    /// </summary>
    public abstract void PerformAttack(Transform target);
    
    /// <summary>
    /// 공격 범위 반환 - 각 Enemy 타입에서 구현
    /// </summary>
    public abstract float GetAttackRange();
    
    /// <summary>
    /// 공격 가능 여부 - 각 Enemy 타입에서 구현
    /// </summary>
    public abstract bool CanAttack();
}

