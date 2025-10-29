public interface IWeapon
{
    WeaponData Data { get; }
    void HandleInput(PlayerCombat combat, AttackType attackType, bool isInputReleased);
    void PerformAttack(CharacterAnimation characterAnim); // ⬅️ 이 줄을 추가하세요.
    void OnEquip(PlayerCombat combat);
    void OnUnequip(PlayerCombat combat);
    
    float GetCurrentAttackAnimationLength();
    float GetCurrentAttackDamage();
    
}