using UnityEngine;

public class AttackState : BasePlayerState
{
    // 콤보 입력 대기 시간 (애니메이션 끝난 후 다음 콤보를 기다리는 시간)
    private const float COMBO_BUFFER_TIME = 0.5f;
    // 공격 후 이동 감속 시간 (attackmoveforce를 부드럽게 감속)
    private const float ATTACK_MOVE_DURATION = 0.3f;
    
    private bool _isAnimationComplete = false;
    private bool _isInComboBuffer = false; // 콤보 버퍼 타임 중인지
    private float _comboBufferTimer = 0f;
    private float _attackDuration = 0f;
    private float _attackTimer = 0f;
    private float _attackMoveTimer = 0f;
    private float _initialAttackVelocity = 0f; // 공격 시작 시 velocity 저장

    public override void OnEnter(PlayerStateMachine stateMachine)
    {
        base.OnEnter(stateMachine);
        _isAnimationComplete = false;
        _isInComboBuffer = false;
        _comboBufferTimer = 0f;
        _attackMoveTimer = ATTACK_MOVE_DURATION;
        
        // 스테미나 소모 체크
        var playerHealth = _player.Health;
        if (playerHealth != null)
        {
            float staminaCost = _player.Combat.GetCurrentAttackStaminaCost();
            
            if (!playerHealth.UseStamina(staminaCost))
            {
                Debug.LogWarning($"[AttackState] 스테미나 부족! 필요: {staminaCost}, 현재: {playerHealth.CurrentStamina:F1}");
                _stateMachine.TransitionTo(PlayerState.Idle);
                return;
            }
        }
        else
        {
            Debug.LogWarning("[AttackState] PlayerHealth를 찾을 수 없습니다!");
        }
        
        // 현재 방향으로 이동 잠금 (반대 방향 입력 무시)
        _controller?.LockDirection();
        
        // 공격 실행 (attackmoveforce가 velocity에 적용됨)
        _player.Combat.PerformCurrentAttack();
        
        // 공격 직후의 velocity를 저장 (Lerp 시작점)
        if (_rigidbody != null)
        {
            _initialAttackVelocity = _rigidbody.linearVelocity.x;
        }
        
        // 무기별 애니메이션 길이 가져오기
        _attackDuration = _player.Combat.GetCurrentAttackAnimationLength();
        _attackTimer = _attackDuration;
        
    }
    
    public override void OnUpdate()
    {
        // attackmoveforce를 부드럽게 감속 (Lerp 사용)
        if (_attackMoveTimer > 0f)
        {
            _attackMoveTimer -= Time.deltaTime;
            
            // Lerp 진행도 계산 (0 -> 1)
            float t = 1f - (_attackMoveTimer / ATTACK_MOVE_DURATION);
            
            // 초기 velocity에서 0으로 부드럽게 감소
            if (_rigidbody != null)
            {
                Vector3 velocity = _rigidbody.linearVelocity;
                velocity.x = Mathf.Lerp(_initialAttackVelocity, 0f, t);
                _rigidbody.linearVelocity = velocity;
            }
        }
        else
        {
            // 감속이 완료되면 완전히 정지
            _controller?.Stop();
        }
        
        // 타이머 기반으로 애니메이션 완료 체크
        if (!_isAnimationComplete)
        {
            _attackTimer -= Time.deltaTime;
            
            if (_attackTimer <= 0f)
            {
                _isAnimationComplete = true;
                _isInComboBuffer = true;
                _comboBufferTimer = COMBO_BUFFER_TIME;
            }
        }
        // 애니메이션이 완료된 후 콤보 버퍼 타임
        else if (_isInComboBuffer)
        {
            _comboBufferTimer -= Time.deltaTime;
            
            // 콤보 버퍼 시간이 끝나면 콤보 리셋 후 Idle로 전환
            if (_comboBufferTimer <= 0f)
            {
                // 현재 무기의 콤보 상태 리셋
                var currentWeapon = _player.Combat.GetCurrentWeaponStrategy();
                currentWeapon?.ResetCombo();
                
                _stateMachine.TransitionTo(PlayerState.Idle);
            }
        }
    }
    
    
    public override void OnExit() 
    {
        // 이동 방향 잠금 해제
        _controller?.UnlockDirection();
    }

    public override bool CanTransitionTo(PlayerState newState)
    {
        // 공격 애니메이션이 완료되기 전에는 다른 공격, 회피, Dead만 허용
        if (!_isAnimationComplete)
        {
            return newState == PlayerState.Attack || 
                   newState == PlayerState.Dodge || 
                   newState == PlayerState.Dead;
        }
        
        // 애니메이션이 완료되면 모든 상태로 전이 가능
        return true;
    }
    
    // 콤보 버퍼 타임 중인지 확인 (외부에서 참조 가능)
    public bool IsInComboBuffer => _isInComboBuffer;
}