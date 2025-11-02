using UnityEngine;

public class EquipState : BasePlayerState
{
    private float _equipTimer;
    private const float EQUIP_ANIMATION_DURATION = 0.7f; // 장착/해제 애니메이션 길이

    public override void OnEnter(PlayerStateMachine stateMachine)
    {
        base.OnEnter(stateMachine);

        // 장착/해제 애니메이션 시작 전 이동 멈추기
        _controller?.Stop();
        
        // 현재 방향으로 이동 잠금 (반대 방향 입력 무시)
        _controller?.LockDirection();

        // PlayerCombat의 무기 상호작용 로직 호출
        _player.Combat.HandleWeaponInteraction();
        
        // 타이머 설정
        _equipTimer = EQUIP_ANIMATION_DURATION;
    }

    public override void OnUpdate()
    {
        // 장착/해제 중에는 계속 이동 멈추기 (velocity를 0으로 유지)
        _controller?.Stop();
        
        // 타이머를 감소시키고, 0이 되면 Idle 상태로 전환
        _equipTimer -= Time.deltaTime;
        if (_equipTimer <= 0f)
        {
            _stateMachine.TransitionTo(PlayerState.Idle);
        }
    }
    
    public override void OnExit() 
    {
        // 이동 방향 잠금 해제
        _controller?.UnlockDirection();
    }
}