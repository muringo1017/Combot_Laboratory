using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
public class MyBT : MonoBehaviour
{
    private MyNode _root;
    private Animator _animator;
    
    public Transform target;
    public float speed = 2.0f;
    public float chaseRange =  5.0f;
    public float attackRange = 1.5f;
    
    private BaseEnemy _enemy; // BaseEnemy 컴포넌트 참조 (Enemy, HandgunEnemy, MachinegunEnemy 등 모두 호환)
    
    // 공격 커밋 관련 변수 추가
    private bool _isAttackCommitted = false;
    private float _attackCommitTime = 0f;
    private float _attackAnimationLength = 1.0f;
    
    // 원거리 공격 커밋 관련
    private bool _isShootCommitted = false;
    private float _shootCommitTime = 0f;
    private float _shootAnimationLength = 1.0f; // 발사 애니메이션 길이
    private bool _hasFiredBullet = false; // 이번 공격에서 총알을 발사했는지

    // 피격(스턴) 처리
    [Header("Damage/Stun Settings")]
    [SerializeField] private string damagedTrigger = "Damaged";
    [SerializeField] private float defaultStunDuration = 0.3f;
    [SerializeField] private bool interruptAttacksOnDamage = true;
    private bool _isStunned = false;
    private float _stunTime = 0f;
    
    // 사망 처리
    [Header("Death Settings")]
    [SerializeField] private string deathTrigger = "Die";
    [SerializeField] private string invincibleLayerName = "Invincible";
    private bool _isDead = false;
    private bool _wasKinematic = false;
    private Rigidbody _rigidbody;
    private int _originalLayer;
    private Collider _collider;

    

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _originalLayer = gameObject.layer; // 원래 레이어 저장
        _enemy = GetComponent<BaseEnemy>(); // BaseEnemy 컴포넌트 가져오기 (Enemy, HandgunEnemy, MachinegunEnemy 등 모두 호환)
        
        if (_enemy == null)
        {
            Debug.LogError($"[MyBT] {gameObject.name}에 BaseEnemy 컴포넌트가 없습니다! Enemy, HandgunEnemy, MachinegunEnemy, MeleeEnemy 중 하나를 추가해주세요.");
        }
        else
        {
            Debug.Log($"[MyBT] Enemy 컴포넌트 찾음: {_enemy.GetType().Name}, CanShoot={_enemy.CanShoot}");
        }
        
        _root = new MySelector(new List<MyNode>
            {
                // 사망이 최우선. 사망 중에는 다른 노드 평가를 막음
                new MyLeaf(IsDead),
                
                // 스턴이 두 번째 우선. 스턴 중에는 다른 노드 평가를 막음
                new MyLeaf(IsStunned),

                // 원거리 공격 진행 중 (커밋된 공격 실행)
                new MySequence(new List<MyNode>
                {
                    new MyLeaf(ShouldContinueShoot),
                    new MyLeaf(ShootAtPlayer)
                }),

                // 원거리 공격 시작 (범위 체크 후 커밋)
                new MySequence(new List<MyNode>
                {
                    new MyLeaf(CheckShootRange),
                    new MyLeaf(CommitToShoot)
                }),

                // 근접 공격 진행 중
                new MySequence(new List<MyNode>
                {
                    new MyLeaf(ShouldContinueAttack),
                    new MyLeaf(AttackPlayer)
                }),
                
                // 근접 공격 시작
                new MySequence(new List<MyNode>
                {
                    new MyLeaf(CheckPlayerInRange), 
                    new MyLeaf(CommitToAttack)
                    
                }),
                new MySequence(new List<MyNode>
                {
                    new MyLeaf(CheckChaseRange),
                    new MyLeaf(ChasePlayer)
                }),
                new MyLeaf(IDLE)
            });
        
            AnimationClip[] clips = _animator.runtimeAnimatorController.animationClips;
            foreach (var clip in clips)
            {
                if (clip.name == "ATTACK")
                {
                    _attackAnimationLength = clip.length;
                    break;
                }
            }
    }

    private void Update()
    {
        // 스턴 타이머 업데이트
        if (_isStunned)
        {
            _stunTime -= Time.deltaTime;
            if (_stunTime <= 0f)
            {
                _isStunned = false;
            }
        }

        _root.Evaluate();

        // 근접 공격 커밋 타이머
        if (_isAttackCommitted)
        {
            _attackCommitTime -= Time.deltaTime;
            if (_attackCommitTime <= 0f)
            {
                _isAttackCommitted = false;
            }
        }
        
        // 원거리 공격 커밋 타이머
        if (_isShootCommitted)
        {
            _shootCommitTime -= Time.deltaTime;
            if (_shootCommitTime <= 0f)
            {
                _isShootCommitted = false;
                _hasFiredBullet = false; // 다음 공격을 위해 리셋
            }
        }
    }

    // 사망 상태 노드: 사망 중엔 Running 반환하여 상위 Selector가 다른 노드를 실행하지 않게 함
    MyNodeStatus IsDead()
    {
        return _isDead ? MyNodeStatus.Running : MyNodeStatus.Failure;
    }
    
    // 스턴 상태 노드: 스턴 중엔 Running 반환하여 상위 Selector가 다른 노드를 실행하지 않게 함
    MyNodeStatus IsStunned()
    {
        return _isStunned ? MyNodeStatus.Running : MyNodeStatus.Failure;
    }

    MyNodeStatus ShouldContinueAttack()
    {
        return _isAttackCommitted ? MyNodeStatus.Success : MyNodeStatus.Failure;
    }
    
    MyNodeStatus ShouldContinueShoot()
    {
        return _isShootCommitted ? MyNodeStatus.Success : MyNodeStatus.Failure;
    }

    private MyNodeStatus RangeCheck(float range)
    {
        float distance = Vector3.Distance(transform.position, target.position);
        return distance < range ? MyNodeStatus.Success : MyNodeStatus.Failure;
    }
    MyNodeStatus CheckChaseRange()
    {
        
        if (_isAttackCommitted) return MyNodeStatus.Failure;
        return RangeCheck(chaseRange);
    }
    MyNodeStatus CheckPlayerInRange()
    {
        if (_isAttackCommitted) return MyNodeStatus.Success;
        return RangeCheck(attackRange);
    }

    MyNodeStatus CommitToAttack()
    {
        if (!_isAttackCommitted)
        {
            // 공격 커밋 시 플레이어를 바라보도록 회전
            Rotate();
            _isAttackCommitted = true;
            _attackCommitTime = _attackAnimationLength;
        }
        return MyNodeStatus.Success;
    }

    MyNodeStatus AttackPlayer()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.IsName("ATTACK"))
        {
            // 애니메이션 진행 중
            if (stateInfo.normalizedTime < 1.0f)
            {
                return MyNodeStatus.Running;
            }
            else
            {
                // 애니메이션 완료
                return MyNodeStatus.Success;
            }
        }
        else
        {
            // 공격 시작
            Rotate();
            AnimatorChange("ATTACK");
            return MyNodeStatus.Running;
        }
    }


    MyNodeStatus IDLE()
    {
        AnimatorChange("IDLE");
        return MyNodeStatus.Success;
    }

    MyNodeStatus ChasePlayer()
    {
        // 플레이어를 바라보도록 회전 (InRange 상태에서도 지속적으로 회전)
        Rotate();
        
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        AnimatorChange("MOVE");
        
        return MyNodeStatus.Running;
    }

    void Rotate()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0.0f;
        transform.forward = direction;
    }
    
    // 원거리 공격 범위 체크
    MyNodeStatus CheckShootRange()
    {
        if (_enemy == null)
        {
            Debug.LogWarning("[MyBT] Enemy 컴포넌트가 NULL!");
            return MyNodeStatus.Failure;
        }
        
        if (!_enemy.CanShoot)
        {
            Debug.Log($"[MyBT] CanShoot=false (원거리 공격 불가)");
            return MyNodeStatus.Failure;
        }
        
        // 이미 공격 커밋 중이면 실패
        if (_isShootCommitted || _isAttackCommitted)
        {
            return MyNodeStatus.Failure;
        }
        
        float distance = Vector3.Distance(transform.position, target.position);
        float shootRange = _enemy.GetAttackRange();
        bool inRange = distance < shootRange && distance > attackRange; // 근접 범위 밖, 원거리 범위 안
        
        if (inRange)
        {
            Debug.Log($"[MyBT] 원거리 범위 안! distance={distance:F2}, ShootRange={shootRange}, attackRange={attackRange}");
        }
        
        return inRange ? MyNodeStatus.Success : MyNodeStatus.Failure;
    }
    
    // 원거리 공격 커밋
    MyNodeStatus CommitToShoot()
    {
        if (!_isShootCommitted)
        {
            // 발사 커밋 시 플레이어를 바라보도록 회전
            Rotate();
            _isShootCommitted = true;
            _shootCommitTime = _shootAnimationLength;
            _hasFiredBullet = false;
            
            Debug.Log($"[MyBT] 원거리 공격 커밋! 애니메이션 시간: {_shootAnimationLength}초");
        }
        return MyNodeStatus.Success;
    }
    
    // 원거리 공격 실행
    MyNodeStatus ShootAtPlayer()
    {
        if (_enemy == null || target == null)
        {
            return MyNodeStatus.Failure;
        }
        
        // 플레이어를 계속 향하도록 회전
        Rotate();
        
        // 발사 애니메이션 재생
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        
        // ATTACK 애니메이션 재생 중인지 확인
        if (stateInfo.IsName("ATTACK"))
        {
            // 코루틴이 아직 시작 안 됐고 발사도 안 했으면 공격 실행
            if (!_hasFiredBullet)
            {
                Debug.Log($"[MyBT] 원거리 공격 실행!");
                _enemy.PerformAttack(target);
                _hasFiredBullet = true;
            }
            
            // 애니메이션이 완료되었는지 확인
            if (stateInfo.normalizedTime < 1.0f)
            {
                return MyNodeStatus.Running; // 애니메이션 진행 중
            }
            else
            {
                Debug.Log("[MyBT] 원거리 공격 애니메이션 완료");
                return MyNodeStatus.Success; // 애니메이션 완료
            }
        }
        else
        {
            // 발사 애니메이션 시작
            AnimatorChange("ATTACK"); // 또는 "SHOOT" 애니메이션이 따로 있으면 사용
            Debug.Log("[MyBT] 원거리 공격 애니메이션 시작");
            return MyNodeStatus.Running;
        }
    }
    
    private void AnimatorChange(string temp)
    {
        if (_isStunned) return; // 스턴 중엔 다른 상태 강제 전환 금지
        _animator.SetBool("IDLE", false);
        _animator.SetBool("MOVE", false);
        _animator.SetBool("ATTACK", false);
        
        _animator.SetBool(temp, true);
    }

    // 외부(Enemy.TakeDamage 등)에서 호출: 피격 처리 및 스턴 시작
    public void OnDamaged(float stunDuration)
    {
        if (interruptAttacksOnDamage)
        {
            _isAttackCommitted = false;
            _isShootCommitted = false; // 원거리 공격도 중단
            _hasFiredBullet = false;
        }
        _isStunned = true;
        _stunTime = stunDuration > 0 ? stunDuration : defaultStunDuration;
        if (_animator != null && !string.IsNullOrEmpty(damagedTrigger))
        {
            _animator.SetTrigger(damagedTrigger);
        }
    }
    
    // 외부(Enemy.Die 등)에서 호출: 사망 처리
    public void OnDeath()
    {
        _isDead = true;
        _isAttackCommitted = false; // 공격 커밋 해제
        _isShootCommitted = false; // 원거리 공격 커밋 해제
        _hasFiredBullet = false;
        _isStunned = false; // 스턴 상태 해제
        
        // Rigidbody kinematic 활성화 (사망 시 물리 충돌 무시)
        if (_rigidbody != null)
        {
            _wasKinematic = _rigidbody.isKinematic;
            _rigidbody.isKinematic = true;
        }
        
        // Collider 비활성화 (통과 가능하도록)
        if (_collider != null)
        {
            _collider.enabled = false;
        }
        
        // 무적 레이어로 변경 (플레이어와의 상호작용 차단)
        int invincibleLayer = LayerMask.NameToLayer(invincibleLayerName);
        if (invincibleLayer != -1)
        {
            gameObject.layer = invincibleLayer;
        }
        
        if (_animator != null && !string.IsNullOrEmpty(deathTrigger))
        {
            _animator.SetTrigger(deathTrigger);
        }
    }
}
