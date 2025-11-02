using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [Header("히트박스 콜라이더")]
    [Header("맨손 히트박스 (4개)")]
    [SerializeField] private BoxCollider unarmedLeftArmHitbox;    
    [SerializeField] private BoxCollider unarmedRightArmHitbox;   
    [SerializeField] private BoxCollider unarmedLeftLegHitbox;     
    [SerializeField] private BoxCollider unarmedRightLegHitbox;     
    
    [Header("자동 무기 히트박스")]
    private BoxCollider _currentWeaponHitbox;  // 현재 무기의 BoxCollider
    
    [Header("기본 설정")]
    [SerializeField] private LayerMask enemyLayer;
    
    private PlayerCombat _playerCombat;
    private WeaponManager _weaponManager;
    
    private void Awake()
    {
        _playerCombat = GetComponentInParent<PlayerCombat>();
        _weaponManager = GetComponentInParent<WeaponManager>();
    }
    
    private void OnEnable()
    {
        if (_weaponManager != null)
        {
            _weaponManager.OnWeaponChanged += OnWeaponChanged;
        }
    }
    
    private void OnDisable()
    {
        if (_weaponManager != null)
        {
            _weaponManager.OnWeaponChanged -= OnWeaponChanged;
        }
    }
    
    private void OnWeaponChanged(IWeapon newWeapon)
    {
        UpdateHitboxVisibility();
    }
    
    private void UpdateHitboxVisibility()
    {
        // 모든 맨손 히트박스 비활성화
        SetHitboxActive(unarmedLeftArmHitbox, false);
        SetHitboxActive(unarmedRightArmHitbox, false);
        SetHitboxActive(unarmedLeftLegHitbox, false);
        SetHitboxActive(unarmedRightLegHitbox, false);
        
        // 이전 무기 히트박스는 비활성화하지 않음 (무기 놓기 시 콜라이더 유지)
        if (_currentWeaponHitbox != null)
        {
            _currentWeaponHitbox = null;
        }
        
        if (_weaponManager == null || !_weaponManager.HasWeapon)
        {
            // 맨손일 때: 4개 히트박스 활성화
            SetHitboxActive(unarmedLeftArmHitbox, true);
            SetHitboxActive(unarmedRightArmHitbox, true);
            SetHitboxActive(unarmedLeftLegHitbox, true);
            SetHitboxActive(unarmedRightLegHitbox, true);
        }
        else
        {
            // 무기 착용 시: 맨손 히트박스는 비활성화, 무기 콜라이더만 활성화
            _currentWeaponHitbox = GetWeaponHitbox();
             if (_currentWeaponHitbox != null)
             {
                 SetHitboxActive(_currentWeaponHitbox, true);
             }
        }
    }
    
    private BoxCollider GetWeaponHitbox()
    {
        if (_weaponManager == null || _weaponManager.CurrentWeapon == null)
            return null;
            
        // WeaponManager에서 현재 장착된 무기 오브젝트 가져오기
        var weaponManager = _weaponManager.GetComponent<WeaponManager>();
        if (weaponManager == null)
            return null;
            
        // WeaponManager의 private 필드에 접근하기 위해 리플렉션 사용
        var weaponObjectField = typeof(WeaponManager).GetField("_equippedWeaponObject", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (weaponObjectField != null)
        {
            var weaponObject = weaponObjectField.GetValue(weaponManager) as GameObject;
            if (weaponObject != null)
            {
                // 무기 오브젝트에서 BoxCollider 찾기
                var boxCollider = weaponObject.GetComponent<BoxCollider>();
                if (boxCollider != null)
                {
                    return boxCollider;
                }
                
                // 자식 오브젝트에서 BoxCollider 찾기
                boxCollider = weaponObject.GetComponentInChildren<BoxCollider>();
                if (boxCollider != null)
                {
                    return boxCollider;
                }
            }
        }
        
        return null;
    }
    
    private void SetHitboxActive(BoxCollider hitbox, bool active)
    {
        if (hitbox != null)
        {
            hitbox.enabled = active;
        }
    }
    
    // 애니메이션 이벤트로 호출
    public void CheckForHit()
    {
        if (_weaponManager == null) return;
        
        // 무기를 착용 중이면 무기 히트박스만 사용
        if (_weaponManager.HasWeapon)
        {
             if (_currentWeaponHitbox != null && _currentWeaponHitbox.enabled)
             {
                 float damage = _playerCombat != null ? _playerCombat.GetCurrentAttackDamage() : 10f;
                 CheckColliderHit(_currentWeaponHitbox, damage);
             }
        }
        else
        {
            // 맨손일 때만 맨손 히트박스 사용
            float damage = _playerCombat != null ? _playerCombat.GetCurrentAttackDamage() : 10f;
            BoxCollider[] unarmedHitboxes = GetUnarmedHitboxes();
            
             foreach (var hitbox in unarmedHitboxes)
             {
                 if (hitbox != null && hitbox.enabled)
                 {
                     CheckColliderHit(hitbox, damage);
                 }
             }
        }
    }
    
    private BoxCollider[] GetUnarmedHitboxes()
    {
        // 맨손 히트박스들만 반환
        var unarmedHitboxes = new System.Collections.Generic.List<BoxCollider>();
        if (unarmedLeftArmHitbox != null && unarmedLeftArmHitbox.enabled) unarmedHitboxes.Add(unarmedLeftArmHitbox);
        if (unarmedRightArmHitbox != null && unarmedRightArmHitbox.enabled) unarmedHitboxes.Add(unarmedRightArmHitbox);
        if (unarmedLeftLegHitbox != null && unarmedLeftLegHitbox.enabled) unarmedHitboxes.Add(unarmedLeftLegHitbox);
        if (unarmedRightLegHitbox != null && unarmedRightLegHitbox.enabled) unarmedHitboxes.Add(unarmedRightLegHitbox);
        return unarmedHitboxes.ToArray();
    }
    
    private BoxCollider[] GetCurrentHitboxes()
    {
        if (_weaponManager == null || !_weaponManager.HasWeapon)
        {
            return GetUnarmedHitboxes();
        }
        else
        {
            // 무기일 때는 현재 무기의 BoxCollider 반환
            if (_currentWeaponHitbox != null && _currentWeaponHitbox.enabled)
            {
                return new BoxCollider[] { _currentWeaponHitbox };
            }
            return new BoxCollider[0];
        }
    }
    
    private void CheckColliderHit(BoxCollider hitbox, float damage)
    {
        // 콜라이더의 월드 위치와 크기 계산
        Vector3 center = hitbox.transform.TransformPoint(hitbox.center);
        Vector3 halfExtents = Vector3.Scale(hitbox.size * 0.5f, hitbox.transform.lossyScale);
        Quaternion rotation = hitbox.transform.rotation;
        
        // BoxOverlap으로 적 검출
        Collider[] hits = Physics.OverlapBox(center, halfExtents, rotation, enemyLayer);
        
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                BaseEnemy enemy = hit.GetComponent<BaseEnemy>();
                if (enemy != null)
                {
                    // 타격 위치 계산 (적의 중심)
                    Vector3 hitPosition = hit.bounds.center;
                    
                    // 타격 방향 (플레이어 → 적)
                    Vector3 hitDirection = (hit.transform.position - transform.position).normalized;
                    
                    enemy.TakeDamage(damage);
                    
                    Debug.Log($"[AttackHitbox] 적 타격! 데미지: {damage}, 위치: {hitPosition}");
                    
                    // 타격감 효과 (카메라, 히트스탑, 이펙트)
                    ApplyImpactFeedback(damage, hitPosition, hitDirection);
                }
            }
        }
    }
    
    // 타격감 피드백 적용
    private void ApplyImpactFeedback(float damage, Vector3 hitPosition, Vector3 hitDirection)
    {
        // 카메라 쉐이크
        if (CameraShake.Instance != null)
        {
            // 데미지에 따라 흔들림 강도 결정
            if (damage < 10f)
            {
                CameraShake.Instance.ShakeLight();
            }
            else if (damage < 20f)
            {
                CameraShake.Instance.ShakeMedium();
            }
            else
            {
                CameraShake.Instance.ShakeHeavy();
            }
        }
        
        // 히트스탑
        if (HitStop.Instance != null)
        {
            // 데미지에 따라 정지 시간 결정
            if (damage < 10f)
            {
                HitStop.Instance.StopLight();
            }
            else if (damage < 20f)
            {
                HitStop.Instance.StopMedium();
            }
            else
            {
                HitStop.Instance.StopHeavy();
            }
        }
        
        // 히트 이펙트
        if (HitEffectManager.Instance != null)
        {
            HitEffectManager.Instance.PlayHitEffectByDamage(damage, hitPosition, hitDirection);
        }
        else
        {
            // HitEffectManager가 없으면 간단한 플래시 이펙트 사용
            Debug.LogWarning("[AttackHitbox] HitEffectManager가 없습니다. 간단한 이펙트를 사용합니다.");
        }
    }
}