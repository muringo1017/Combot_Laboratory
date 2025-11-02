using System.Collections.Generic;
using UnityEngine;

public class LongSword : IWeapon
{
    public WeaponData Data { get; private set; }

    // LongSword만의 고유 콤보 상태
    private ComboState _currentComboState = ComboState.NONE;

    public LongSword(WeaponData data)
    {
        Data = data;
    }
    private Dictionary<ComboState, float> _animationLengths = new Dictionary<ComboState, float>()
    {
        { ComboState.LightAttack_1, 0.8f },
        { ComboState.LightAttack_2, 0.6f },
        { ComboState.LightAttack_3, 1.0f },
        { ComboState.HeavyAttack_1, 0.8f },
        { ComboState.HeavyAttack_2, 0.8f },
        { ComboState.HeavyAttack_3, 1.0f }
    };

    public float GetCurrentAttackAnimationLength()
    {
        if (_animationLengths.ContainsKey(_currentComboState))
            return _animationLengths[_currentComboState];
        
        return 1.0f; // 기본값
    }

    private Dictionary<ComboState, float> _attackDamage = new Dictionary<ComboState, float>()
    {
        { ComboState.LightAttack_1, 10f },
        { ComboState.LightAttack_2, 15f },
        { ComboState.LightAttack_3, 20f },
        { ComboState.HeavyAttack_1, 25f },
        { ComboState.HeavyAttack_2, 30f },
        { ComboState.HeavyAttack_3, 40f }
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
                Debug.LogWarning($"[LongSword] 스테미나 부족! 공격 취소. 필요: {staminaCost}, 현재: {playerHealth.CurrentStamina:F1}");
                return; // 공격 취소
            }
        }

        var nextState = _currentComboState;
        // --- LongSword만의 다른 콤보 트리 ---
        switch (_currentComboState)
        {
            case ComboState.NONE:
                if (attackType == AttackType.LightAttack) nextState = ComboState.LightAttack_1; //z->
                else if (attackType == AttackType.HeavyAttack) nextState = ComboState.HeavyAttack_1;//x->
                break;
            case ComboState.LightAttack_1:
                if (attackType == AttackType.LightAttack) nextState = ComboState.LightAttack_2; //z->zz
                else if (attackType == AttackType.HeavyAttack) nextState = ComboState.HeavyAttack_2;//z->zx
                break;
            case ComboState.LightAttack_2:
                if (attackType == AttackType.LightAttack) nextState = ComboState.LightAttack_3; // z->zz->zzz
                else if (attackType == AttackType.HeavyAttack) nextState = ComboState.HeavyAttack_3;//z->zx->zxx
                break;
            case ComboState.HeavyAttack_1:
                // X -> X 콤보
                if (attackType == AttackType.HeavyAttack) nextState = ComboState.HeavyAttack_2; //x->xx
                else if (attackType == AttackType.LightAttack) nextState = ComboState.LightAttack_2; //x->xz
                break;
            case ComboState.HeavyAttack_2:
                // X -> X 콤보
                if (attackType == AttackType.HeavyAttack) nextState = ComboState.HeavyAttack_3;//x->xx->xxx
                else if (attackType == AttackType.LightAttack) nextState = ComboState.LightAttack_3; //x->xz->xzz
                break;
        }

        if (nextState != _currentComboState)
        {
            _currentComboState = nextState;
            // PlayerCombat을 통해 상태 머신에 상태 전환을 요청합니다.
            combat.PlayerStateMachine.TransitionTo(PlayerState.Attack);
        }
    }

    /// <summary>
    /// AttackState에 진입했을 때 호출되어 실제 LongSword 애니메이션을 재생합니다.
    /// </summary>
    public void PerformAttack(CharacterAnimation characterAnim)
    {
        // 각 콤보 상태에 맞는 'LongSword' 전용 애니메이션 함수를 호출합니다.
        switch (_currentComboState)
        {
            case ComboState.LightAttack_1: characterAnim.LongSword_Light_1(); break;
            case ComboState.LightAttack_2: characterAnim.LongSword_Light_2(); break;
            case ComboState.LightAttack_3: characterAnim.LongSword_Light_3(); break;
            case ComboState.HeavyAttack_1: characterAnim.LongSword_Heavy_1(); break;
            case ComboState.HeavyAttack_2: characterAnim.LongSword_Heavy_2(); break; 
            case ComboState.HeavyAttack_3: characterAnim.LongSword_Heavy_3(); break;
        }
        
        // LongSword 공격 시 히트박스 체크
        CheckForHit(characterAnim);
    }
    
    private void CheckForHit(CharacterAnimation characterAnim)
    {
        // CharacterAnimation을 통해 AttackHitbox에 접근
        var attackHitbox = characterAnim.GetComponentInChildren<AttackHitbox>();
        if (attackHitbox != null)
        {
            attackHitbox.CheckForHit();
        }
    }

    public void OnEquip(PlayerCombat combat)
    {
        // 여기에 장검을 장착했을 때 모델의 손에 칼을 보이게 하는 로직 등을 추가할 수 있습니다.
    }

    public void OnUnequip(PlayerCombat combat)
    {
        // 장착 해제 시 칼을 보이지 않게 하는 로직 등을 추가할 수 있습니다.
    }
    
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