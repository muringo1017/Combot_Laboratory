using UnityEngine;

public class DodgeState : BasePlayerState
{
    [Header("Dodge Settings")]
    public float dodgeDistance = 2.5f;
    public float dodgeDuration = 0.3f;
    public float invincibilityDuration = 0.3f;
    
    private float _dodgeTimer;
    private float _invincibilityTimer;
    private bool _isInvincible = false;
    private Vector3 _dodgeDirection;
    private float _originalSpeed;
    private bool _wasKinematic = false;
    private Rigidbody _rigidbody;
    
    public override void OnEnter(PlayerStateMachine stateMachine)
    {
        base.OnEnter(stateMachine);
        
        // 회피 게이지 소모 (PlayerHealth에서 관리)
        var playerHealth = _player.Health;
        if (playerHealth != null)
        {
            bool canDodge = playerHealth.UseDodgeGauge();
            if (!canDodge)
            {
                Debug.LogWarning("[DodgeState] 회피 게이지 부족! 회피 취소");
                _stateMachine.TransitionTo(PlayerState.Idle);
                return;
            }
        }
        else
        {
            Debug.LogWarning("[DodgeState] PlayerHealth를 찾을 수 없습니다!");
        }
        
        // 공격 중 회피 시 콤보 리셋 (공격이 중단됨)
        var currentWeapon = _player.Combat.GetCurrentWeaponStrategy();
        currentWeapon?.ResetCombo();
        
        // 현재 방향으로 이동 잠금 (반대 방향 입력 무시)
        _controller?.LockDirection();
        
        // Rigidbody 가져오기
        _rigidbody = _player.GetComponent<Rigidbody>();
        if (_rigidbody != null)
        {
            // 회피 시작 전 기존 velocity 초기화
            Vector3 velocity = _rigidbody.linearVelocity;
            velocity.x = 0;
            _rigidbody.linearVelocity = velocity;
            
            // 현재 kinematic 상태 저장
            _wasKinematic = _rigidbody.isKinematic;
            // Dodge 중에는 kinematic 활성화 (물리 충돌 무시)
            _rigidbody.isKinematic = true;
        }
        
        // 현재 바라보는 방향으로 회피 방향 설정
        _dodgeDirection = _player.transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        
        // 회피 타이머 초기화
        _dodgeTimer = dodgeDuration;
        _invincibilityTimer = invincibilityDuration;
        _isInvincible = true;
        
        // 회피 애니메이션 재생
        _characterAnimation?.TriggerDodge();
        
        // 무적 상태 설정 (레이어를 통해 구현)
        SetInvincibility(true);
    }

    public override void OnUpdate()
    {
        if (_player == null) return;
        
        // 회피 이동 처리
        if (_dodgeTimer > 0)
        {
            _dodgeTimer -= Time.deltaTime;
            
            // 회피 이동 (kinematic 상태에서는 Transform 사용)
            float dodgeSpeed = dodgeDistance / dodgeDuration;
            Vector3 movement = _dodgeDirection * dodgeSpeed * Time.deltaTime;
            _player.transform.Translate(movement);
        }
        else
        {
            // 회피 이동 완료 후 정지 (kinematic 상태에서는 별도 정지 불필요)
        }
        
        // 무적 상태 타이머 처리
        if (_invincibilityTimer > 0)
        {
            _invincibilityTimer -= Time.deltaTime;
        }
        else if (_isInvincible)
        {
            // 무적 상태 해제
            _isInvincible = false;
            SetInvincibility(false);
        }
        
        // 회피와 무적이 모두 끝나면 Idle 상태로 전환
        if (_dodgeTimer <= 0 && _invincibilityTimer <= 0)
        {
            _stateMachine.TransitionTo(PlayerState.Idle);
        }
    }

    public override void OnExit()
    {
        // Rigidbody 원래 상태로 복원
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = _wasKinematic;
        }
        
        SetInvincibility(false);
        
        // 이동 방향 잠금 해제
        _controller?.UnlockDirection();
    }
    
    public override bool CanTransitionTo(PlayerState newState)
    {
        // 회피 중에는 다른 상태로 전환 불가 (공격과 Idle 제외)
        if (newState == PlayerState.Attack || newState == PlayerState.Idle)
            return true;
            
        return false;
    }
    
    private void SetInvincibility(bool isInvincible)
    {
        // 레이어를 통해 무적 상태 구현
        if (_player != null)
        {
            int invincibleLayer = LayerMask.NameToLayer("Invincible");
            int defaultLayer = LayerMask.NameToLayer("Default");
            
            if (isInvincible && invincibleLayer != -1)
            {
                _player.gameObject.layer = invincibleLayer;
            }
            else if (!isInvincible && defaultLayer != -1)
            {
                _player.gameObject.layer = defaultLayer;
            }
        }
    }
    
    public bool IsInvincible => _isInvincible;
}
