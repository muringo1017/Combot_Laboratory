using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [Header("히트박스 콜라이더")]
    [SerializeField] private BoxCollider unarmedHitbox;    // 맨손 히트박스
    [SerializeField] private BoxCollider swordHitbox;      // 검 히트박스
    
    [Header("기본 설정")]
    [SerializeField] private LayerMask enemyLayer;
    
    private PlayerCombat _playerCombat;
    private WeaponManager _weaponManager;
    
    private void Awake()
    {
        _playerCombat = GetComponentInParent<PlayerCombat>();
        _weaponManager = GetComponentInParent<WeaponManager>();
    }
    
    // 애니메이션 이벤트로 호출
    public void CheckForHit()
    {
        if (_weaponManager == null) return;
        
        float damage = _playerCombat != null ? _playerCombat.GetCurrentAttackDamage() : 10f;
        
        // 현재 무기에 따라 다른 콜라이더 사용
        BoxCollider currentHitbox = GetCurrentHitbox();
        
        if (currentHitbox != null)
        {
            CheckColliderHit(currentHitbox, damage);
        }
    }
    
    private BoxCollider GetCurrentHitbox()
    {
        if (!_weaponManager.HasWeapon)
            return unarmedHitbox;
        else if (_weaponManager.CurrentWeapon is LongSword)
            return swordHitbox;
        else
            return unarmedHitbox; // 기본값
    }
    
    private void CheckColliderHit(BoxCollider hitbox, float damage)
    {
        // 콜라이더의 월드 위치와 크기 계산
        Vector3 center = hitbox.transform.TransformPoint(hitbox.center);
        Vector3 halfExtents = hitbox.size * 0.5f;
        Quaternion rotation = hitbox.transform.rotation;
        
        // BoxOverlap으로 적 검출
        Collider[] hits = Physics.OverlapBox(center, halfExtents, rotation, enemyLayer);
        
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log($"🎯 히트! {hit.name}에게 {damage} 데미지");
                }
            }
        }
        
        if (hits.Length == 0)
        {
            Debug.Log("❌ 적이 히트박스 안에 없음");
        }
    }
}