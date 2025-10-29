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

    public ComboState BufferedAttack { get; }
    public void CommitToBufferedAttack()
    {
        throw new System.NotImplementedException();
    }

    public void ClearBufferedAttack()
    {
        throw new System.NotImplementedException();
    }

    public WeaponType GetWeaponType() => weaponType;
    public string GetWeaponName() => weaponName;
    public GameObject GetWeaponPrefab() => weaponPrefab;
}