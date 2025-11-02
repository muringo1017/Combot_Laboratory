public interface IWeapon
{
    WeaponData Data { get; }
    void HandleInput(PlayerCombat combat, AttackType attackType, bool isInputReleased);
    void PerformAttack(CharacterAnimation characterAnim);
    void OnEquip(PlayerCombat combat);
    void OnUnequip(PlayerCombat combat);
    
    float GetCurrentAttackAnimationLength();
    float GetCurrentAttackDamage();
    float GetCurrentAttackStaminaCost(); // 현재 공격의 스테미나 비용
    
    // 콤보 상태 관리
    void ResetCombo();
    bool IsInCombo();
}