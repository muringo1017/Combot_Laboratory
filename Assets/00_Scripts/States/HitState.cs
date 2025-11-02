using UnityEngine;

/// <summary>
/// 플레이어 피격 상태 - 짧은 경직과 애니메이션 재생
/// </summary>
public class HitState : BasePlayerState
{
    [Header("Hit Settings")]
    private const float HIT_STUN_DURATION = 0.3f; // 피격 경직 시간
    
    private float _hitStunTimer;
    
    public override void OnEnter(PlayerStateMachine stateMachine)
    {
        base.OnEnter(stateMachine);
        
        Debug.Log("[HitState] 플레이어 피격!");
        
        // 이동 멈추기
        _controller?.Stop();
        _controller?.UnlockDirection();
        
        // 피격 애니메이션 재생
        _characterAnimation?.TriggerDamaged();
        
        // 경직 타이머 설정
        _hitStunTimer = HIT_STUN_DURATION;
        
        // 진행 중인 콤보 리셋
        var currentWeapon = _player.Combat.GetCurrentWeaponStrategy();
        currentWeapon?.ResetCombo();
    }
    
    public override void OnUpdate()
    {
        // 경직 중에는 이동 불가
        _controller?.Stop();
        
        // 경직 타이머 감소
        _hitStunTimer -= Time.deltaTime;
        
        // 경직이 끝나면 Idle 상태로 전환
        if (_hitStunTimer <= 0f)
        {
            _stateMachine.TransitionTo(PlayerState.Idle);
        }
    }
    
    public override void OnExit()
    {
        Debug.Log("[HitState] 피격 상태 종료");
    }
    
    public override bool CanTransitionTo(PlayerState newState)
    {
        // 피격 경직 중에는 Idle이나 Dead로만 전환 가능
        if (newState == PlayerState.Idle || newState == PlayerState.Dead)
        {
            return true;
        }
        
        // 회피로 캔슬 가능하도록 (선택사항)
        if (newState == PlayerState.Dodge)
        {
            return true;
        }
        
        return false;
    }
}


