using UnityEngine;

public class MoveState : BasePlayerState
{
    public override void OnEnter(PlayerStateMachine stateMachine)
    {
        base.OnEnter(stateMachine);
        _characterAnimation?.SetMoving(true);
    }

    public override void OnUpdate()
    {
        if (_player == null) return;
        
        // 액션 애니메이션 중에는 이동 불가 (추가 보호 장치)
        var currentStateType = _stateMachine.CurrentStateType;
        if (currentStateType == PlayerState.Attack || 
            currentStateType == PlayerState.Equip || 
            currentStateType == PlayerState.Dodge)
        {
            _controller?.Stop();
            return;
        }
        
        float horizontal = Managers.InputManager.MoveInput.x;
        
        if (horizontal == 0)
        {
            _stateMachine.TransitionTo(PlayerState.Idle);
            return;
        }
        _controller?.Move(horizontal);
    }

    public override void OnExit()
    {
        _controller?.Stop();
        _characterAnimation?.SetMoving(false);
    }
}