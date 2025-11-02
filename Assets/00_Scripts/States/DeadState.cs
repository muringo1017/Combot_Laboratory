using UnityEngine;

public class DeadState : BasePlayerState
{
    public override void OnEnter(PlayerStateMachine stateMachine)
    {
        base.OnEnter(stateMachine);
        
        Debug.Log("[DeadState] 플레이어 사망!");
        
        // 이동 멈추기
        _controller?.Stop();
        _controller?.UnlockDirection();
        
        // 사망 애니메이션 재생 (있다면)
        // _characterAnimation?.TriggerDeath();
        
        // Rigidbody 비활성화 (물리 충돌 무시)
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = true;
        }
    }
    
    public override void OnUpdate()
    {
        // 사망 상태에서는 아무것도 하지 않음
    }
    
    public override void OnExit()
    {
        // 사망 상태에서는 빠져나올 수 없음
    }
    
    public override bool CanTransitionTo(PlayerState newState)
    {
        // 사망 상태에서는 다른 상태로 전환 불가
        return false;
    }
}
