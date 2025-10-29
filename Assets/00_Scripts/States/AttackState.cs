using UnityEngine;

public class AttackState : BasePlayerState
{
    private float _attackTimer;


    public override void OnEnter(PlayerStateMachine stateMachine)
    {
        base.OnEnter(stateMachine); // 공격 실행
        _player.Combat.PerformCurrentAttack();
        
        // 애니메이션 길이 설정 (방법 1: 무기에서 가져오기)
        _attackTimer = _player.Combat.GetCurrentAttackAnimationLength();
        
        // 또는 방법 2: CharacterAnimation에서 실제 클립 길이 가져오기
        // _attackTimer = _characterAnimation.GetCurrentAnimationLength();
        
        Debug.Log($"⚔️ AttackState - 예상 길이: {_attackTimer:F2}s");
    }
    
    public override void OnUpdate()
    {
        _attackTimer -= Time.deltaTime;
        if (_attackTimer <= 0f)
        {
            _stateMachine.TransitionTo(PlayerState.Idle);
        }
    }
    
    public override void OnExit() { }
}