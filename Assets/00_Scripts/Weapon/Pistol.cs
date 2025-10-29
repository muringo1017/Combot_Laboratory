using UnityEngine;

public class Pistol : IWeapon
{
    public WeaponData Data { get; private set; }

    // 권총은 콤보가 없으므로, 콤보 상태를 추적할 필요가 없습니다.

    public Pistol(WeaponData data)
    {
        Data = data;
    }

    private const float PISTOL_ANIMATION_LENGTH = 0.6f;

 
    public void HandleInput(PlayerCombat combat, AttackType attackType, bool isInputReleased)
    {
        // 실제 공격 입력이 아니거나, 키에서 손을 뗀 입력이면 무시합니다.
        if (attackType == AttackType.None || isInputReleased)
        {
            return;
        }

        // 약공격(Z) 입력이 들어오면, Attack 상태로 전환을 요청합니다.
        if (attackType == AttackType.LightAttack)
        {
            combat.PlayerStateMachine.TransitionTo(PlayerState.Attack);
        }
        
        // 여기에 나중에 강공격(X) 로직을 추가할 수 있습니다.
        // else if (attackType == AttackType.HeavyAttack) { ... }
    }

   
    public void PerformAttack(CharacterAnimation characterAnim)
    {
        // 권총은 콤보가 없으므로 항상 하나의 공격 애니메이션만 호출합니다.
        characterAnim.Pistol_Attack_1();
    }
    
    // --- 아래는 IWeapon 인터페이스 규칙을 지키기 위한 필수 함수들입니다 ---

    public void OnEquip(PlayerCombat combat)
    {
        Debug.Log("Pistol Equipped");
        
    }

    public void OnUnequip(PlayerCombat combat)
    {
        Debug.Log("Pistol Unequipped");
    }

    public float GetCurrentAttackAnimationLength()
    {
        throw new System.NotImplementedException();
    }

    public float GetCurrentAttackDamage()
    {
        throw new System.NotImplementedException();
    }

    // 권총은 콤보가 없으므로 버퍼링 관련 기능은 필요 없습니다.
    // 하지만 IWeapon 인터페이스의 규칙이므로 함수 자체는 존재해야 합니다.
    public ComboState BufferedAttack => ComboState.NONE;
    public void CommitToBufferedAttack() { /* Do nothing */ }
    public void ClearBufferedAttack() { /* Do nothing */ }
}