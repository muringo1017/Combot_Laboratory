using UnityEngine;

public abstract class BaseWeapon : IWeapon
{
    protected string weaponName;
    protected GameObject weaponPrefab;
    protected WeaponType weaponType;

    public abstract void PerformLightAttack(PlayerCombat combat);
    public abstract void PerformHeavyAttack(PlayerCombat combat);

    public WeaponData Data { get; }
    public void HandleInput(PlayerCombat combat, AttackType attackType, bool isInputReleased)
    {
        throw new System.NotImplementedException();
    }

    public void PerformAttack(CharacterAnimation characterAnim)
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnEquip(PlayerCombat combat)
    {
       // combat.CharacterAnimation.ResetCombo();
        // 무기 모델 장착 로직
    }

    public virtual void OnUnequip(PlayerCombat combat)
    {
        //combat.CharacterAnimation.ResetCombo();
        // 무기 모델 해제 로직
    }

    public float GetCurrentAttackAnimationLength()
    {
        throw new System.NotImplementedException();
    }

    public float GetCurrentAttackDamage()
    {
        throw new System.NotImplementedException();
    }
    
    public virtual float GetCurrentAttackStaminaCost()
    {
        // 기본 구현: 약공격 2.5
        return 2.5f;
    }

    // 콤보 상태 관리
    public virtual void ResetCombo()
    {
        // 기본 구현: 아무 작업도 하지 않음 (파생 클래스에서 오버라이드 가능)
    }
    
    public virtual bool IsInCombo()
    {
        // 기본 구현: 콤보 상태가 아님 (파생 클래스에서 오버라이드 가능)
        return false;
    }

    public WeaponType GetWeaponType() => weaponType;
    public string GetWeaponName() => weaponName;
    public GameObject GetWeaponPrefab() => weaponPrefab;
}