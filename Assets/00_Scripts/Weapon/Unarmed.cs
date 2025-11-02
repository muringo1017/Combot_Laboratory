using System.Collections.Generic;
using UnityEngine;

public class Unarmed : IWeapon
{
    public WeaponData Data { get; private set; }

    // Unarmed 상태의 고유 콤보 로직
    private ComboState _currentComboState = ComboState.NONE;

    private Dictionary<ComboState, float> _animationLengths = new Dictionary<ComboState, float>()
    {
        { ComboState.LightAttack_1, 0.4f },
        { ComboState.LightAttack_2, 0.4f },
        { ComboState.LightAttack_3, 0.6f },
        { ComboState.HeavyAttack_1, 0.6f },
        { ComboState.HeavyAttack_2, 0.6f },
        { ComboState.HeavyAttack_3, 0.6f }
    };
    
    private Dictionary<ComboState, float> _attackDamage = new Dictionary<ComboState, float>()
    {
        { ComboState.LightAttack_1, 5f },
        { ComboState.LightAttack_2, 6f },
        { ComboState.LightAttack_3, 7f },
        { ComboState.HeavyAttack_1, 8f },
        { ComboState.HeavyAttack_2, 10f },
        { ComboState.HeavyAttack_3, 12f }
    };

    public float GetCurrentAttackDamage()
    {
        if (_attackDamage.ContainsKey(_currentComboState))
            return _attackDamage[_currentComboState];
        return 10f;
    }
    
    public float GetCurrentAttackStaminaCost()
    {
        // 약공격(LightAttack)은 2.5, 강공격(HeavyAttack)은 5
        if (_currentComboState == ComboState.LightAttack_1 || 
            _currentComboState == ComboState.LightAttack_2 || 
            _currentComboState == ComboState.LightAttack_3)
        {
            return 2.5f; // Z 공격
        }
        else if (_currentComboState == ComboState.HeavyAttack_1 || 
                 _currentComboState == ComboState.HeavyAttack_2 || 
                 _currentComboState == ComboState.HeavyAttack_3)
        {
            return 5f; // X 공격
        }
        return 0f;
    }
    
    public float GetCurrentAttackAnimationLength()
    {
        if (_animationLengths.ContainsKey(_currentComboState))
            return _animationLengths[_currentComboState];
        
        return 1.0f; // 기본값
    }

    public Unarmed(WeaponData data)
    {
        Data = data;
    }
    
    public void HandleInput(PlayerCombat combat, AttackType attackType, bool isInputReleased)
    {
        if (attackType == AttackType.None || isInputReleased) return;
        
        // 스테미나 체크 (공격 전에 미리 확인)
        var playerHealth = combat.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            float staminaCost = (attackType == AttackType.LightAttack) ? 10f : 20f;
            if (playerHealth.CurrentStamina < staminaCost)
            {
                Debug.LogWarning($"[Unarmed] 스테미나 부족! 공격 취소. 필요: {staminaCost}, 현재: {playerHealth.CurrentStamina:F1}");
                return; // 공격 취소
            }
        }
        
        var nextState = _currentComboState;
        switch (_currentComboState)
        {
            case ComboState.NONE: 
                if (attackType == AttackType.LightAttack) nextState = ComboState.LightAttack_1;  //z->
                else if (attackType == AttackType.HeavyAttack) nextState = ComboState.HeavyAttack_1; // x->
                break;
            
            case ComboState.LightAttack_1:
                if (attackType == AttackType.LightAttack) nextState = ComboState.LightAttack_2; //z->zz->
                else if (attackType == AttackType.HeavyAttack) nextState = ComboState.HeavyAttack_2; //z->zx->
                break;

            case ComboState.LightAttack_2:
                if (attackType == AttackType.LightAttack) nextState = ComboState.LightAttack_3; //z->zz->zzz
                else if (attackType == AttackType.HeavyAttack) nextState = ComboState.HeavyAttack_3; // z->zz->zzx
                break;

            case ComboState.HeavyAttack_1:
                if (attackType == AttackType.HeavyAttack) nextState = ComboState.HeavyAttack_2; //x->xx
                else if (attackType == AttackType.LightAttack) nextState = ComboState.LightAttack_2; // x->xz
                
                break;
               
            case ComboState.HeavyAttack_2:
                if (attackType == AttackType.HeavyAttack) nextState = ComboState.HeavyAttack_3; // x->xx->xxx
                else if (attackType == AttackType.LightAttack) nextState = ComboState.LightAttack_3; // x->xz->xzz
                break;
        }

        if (nextState != _currentComboState)
        {
            _currentComboState = nextState;
            combat.PlayerStateMachine.TransitionTo(PlayerState.Attack);
        }
    }
    
    public void PerformAttack(CharacterAnimation characterAnim)
    {
        switch (_currentComboState)
        {
            case ComboState.LightAttack_1: characterAnim.LightAttack_1(); break;
            case ComboState.LightAttack_2: characterAnim.LightAttack_2(); break;
            case ComboState.LightAttack_3: characterAnim.LightAttack_3(); break;
            case ComboState.HeavyAttack_1: characterAnim.HeavyAttack_1(); break;
            case ComboState.HeavyAttack_2: characterAnim.HeavyAttack_2(); break;
            case ComboState.HeavyAttack_3: characterAnim.HeavyAttack_3(); break;
        }
    }

    public void OnEquip(PlayerCombat combat) { /* 맨손은 특별한 로직 없음 */ }
    public void OnUnequip(PlayerCombat combat) { /* 맨손은 특별한 로직 없음 */ }
    
    // 콤보 상태 관리
    public void ResetCombo()
    {
        _currentComboState = ComboState.NONE;
    }
    
    public bool IsInCombo()
    {
        return _currentComboState != ComboState.NONE;
    }
}