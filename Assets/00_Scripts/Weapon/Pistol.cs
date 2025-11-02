using System.Collections;
using UnityEngine;

public class Pistol : IWeapon
{
    public WeaponData Data { get; private set; }

    private AttackType _currentAttackType = AttackType.None;
    private PlayerCombat _playerCombat;
    private Transform _firePoint;
    
    // WeaponData에서 가져올 설정들
    private GameObject _bulletPrefab;
    private float _bulletSpeed;
    private float _bulletDamage;
    private int _burstCount;
    private float _burstDelay;

    public Pistol(WeaponData data)
    {
        Data = data;
        
        if (data == null)
        {
            Debug.LogError("Pistol: WeaponData가 null입니다!");
            return;
        }
        
        // WeaponData에서 설정 로드
        _bulletPrefab = data.bulletPrefab;
        _bulletSpeed = data.bulletSpeed;
        _bulletDamage = data.bulletDamage;
        _burstCount = data.burstCount;
        _burstDelay = data.burstDelay;
        
        // 디버그 로그
        Debug.Log($"Pistol 생성: bulletPrefab={(_bulletPrefab != null ? _bulletPrefab.name : "null")}, speed={_bulletSpeed}, damage={_bulletDamage}, burstCount={_burstCount}");
    }

    private const float PISTOL_SINGLE_SHOT_LENGTH = 0.4f; // 단발 애니메이션
    private const float PISTOL_BURST_SHOT_LENGTH = 0.8f;  // 3점사 애니메이션

 
    public void HandleInput(PlayerCombat combat, AttackType attackType, bool isInputReleased)
    {
        // 실제 공격 입력이 아니거나, 키에서 손을 뗀 입력이면 무시합니다.
        if (attackType == AttackType.None || isInputReleased)
        {
            return;
        }

        // 스테미나 체크 (공격 전에 미리 확인)
        var playerHealth = combat.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            float staminaCost = (attackType == AttackType.LightAttack) ? 2f : 5f;
            if (playerHealth.CurrentStamina < staminaCost)
            {
                Debug.LogWarning($"[Pistol] 스테미나 부족! 공격 취소. 필요: {staminaCost}, 현재: {playerHealth.CurrentStamina:F1}");
                return; // 공격 취소
            }
        }

        // 약공격(Z) - 단발 사격
        if (attackType == AttackType.LightAttack)
        {
            _currentAttackType = AttackType.LightAttack;
            combat.PlayerStateMachine.TransitionTo(PlayerState.Attack);
        }
        // 강공격(X) - 3점사
        else if (attackType == AttackType.HeavyAttack)
        {
            _currentAttackType = AttackType.HeavyAttack;
            combat.PlayerStateMachine.TransitionTo(PlayerState.Attack);
        }
    }

   
    public void PerformAttack(CharacterAnimation characterAnim)
    {
        Debug.Log($"Pistol.PerformAttack 호출됨! AttackType={_currentAttackType}");
        
        // 약공격(Z) - 단발
        if (_currentAttackType == AttackType.LightAttack)
        {
            characterAnim.Pistol_Attack_1();
            Debug.Log("Pistol: 단발 사격 실행 (Pistol_Attack_1 트리거)");
            FireBullet();
        }
        // 강공격(X) - 3점사
        else if (_currentAttackType == AttackType.HeavyAttack)
        {
            characterAnim.Pistol_Attack_2(); // X키는 Pistol_Attack_2 애니메이션 재생
            Debug.Log("Pistol: 3점사 실행 (Pistol_Attack_2 트리거)");
            
            // Coroutine 대신 직접 호출로 변경
            if (_playerCombat != null)
            {
                _playerCombat.StartCoroutine(FireBurstShot());
            }
            else
            {
                Debug.LogWarning("Pistol: _playerCombat이 null입니다!");
            }
        }
        else
        {
            Debug.LogWarning($"Pistol: 알 수 없는 AttackType={_currentAttackType}");
        }
    }
    
    private IEnumerator FireBurstShot()
    {
        // 3점사: 3발을 일정 간격으로 발사
        for (int i = 0; i < _burstCount; i++)
        {
            FireBullet();
            
            // 마지막 발이 아니면 대기
            if (i < _burstCount - 1)
            {
                yield return new WaitForSeconds(_burstDelay);
            }
        }
    }
    
    private void FireBullet()
    {
        Debug.Log($"FireBullet 호출됨! bulletPrefab={(null != _bulletPrefab ? _bulletPrefab.name : "NULL")}, firePoint={(_firePoint != null ? _firePoint.name : "NULL")}");
        
        if (_bulletPrefab == null)
        {
            Debug.LogError("Pistol: bulletPrefab가 설정되지 않았습니다! WeaponData에서 bulletPrefab을 할당하세요.");
            return;
        }
        
        if (_firePoint == null)
        {
            Debug.LogError("Pistol: firePoint가 설정되지 않았습니다! Pistol 프리팹에 FirePoint 자식 오브젝트를 추가하세요.");
            return;
        }
        
        // 플레이어의 방향 가져오기
        var playerController = _playerCombat.GetComponent<PlayerController>();
        Vector3 shootDirection = Vector3.right; // 기본 방향
        
        if (playerController != null)
        {
            shootDirection = playerController.FacingDirection;
            Debug.Log($"플레이어 방향: {(playerController.IsFacingRight ? "오른쪽" : "왼쪽")}");
        }
        else
        {
            // PlayerController가 없으면 Transform의 scale로 판단
            shootDirection = _playerCombat.transform.localScale.x > 0 ? Vector3.right : Vector3.left;
            Debug.LogWarning("PlayerController를 찾을 수 없어 Transform scale로 방향 판단");
        }
        
        Debug.Log($"총알 생성 중... 위치: {_firePoint.position}, 발사방향: {shootDirection}");
        
        // 총알 인스턴스 생성
        GameObject bullet = GameObject.Instantiate(_bulletPrefab, _firePoint.position, Quaternion.identity);
        bullet.name = "Bullet_Active"; // (Clone) 제거하고 명확한 이름으로
        
        // 총알 레이어 설정 (PlayerProjectile 레이어가 있으면 사용)
        int playerProjectileLayer = LayerMask.NameToLayer("PlayerProjectile");
        if (playerProjectileLayer != -1)
        {
            bullet.layer = playerProjectileLayer;
        }
        
        // 총알을 발사 방향으로 회전
        if (shootDirection.x > 0)
        {
            bullet.transform.rotation = Quaternion.Euler(0, 90, 0); // 오른쪽
        }
        else
        {
            bullet.transform.rotation = Quaternion.Euler(0, -90, 0); // 왼쪽
        }
        
        Debug.Log($"총알 생성 완료: {bullet.name}, 활성화 상태: {bullet.activeSelf}, 레이어: {LayerMask.LayerToName(bullet.layer)}, 위치: {bullet.transform.position}, 회전: {bullet.transform.rotation.eulerAngles}");
        
        // 총알에 속도와 데미지 설정
        var bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            // 플레이어가 바라보는 방향으로 총알 발사 (x축만 사용)
            Vector3 velocity = new Vector3(shootDirection.x * _bulletSpeed, 0f, 0f);
            bulletRb.linearVelocity = velocity;
            
            // 중력 비활성화
            bulletRb.useGravity = false;
            
            Debug.Log($"총알 속도 설정: velocity={bulletRb.linearVelocity}, speed={_bulletSpeed}, 방향={shootDirection}");
        }
        else
        {
            Debug.LogWarning($"Pistol: 총알({bullet.name})에 Rigidbody가 없습니다!");
        }
        
        // 총알에 데미지 설정 (총알에 Bullet 컴포넌트가 있다고 가정)
        var bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.SetDamage(_bulletDamage);
            Debug.Log($"총알 데미지 설정: {_bulletDamage}");
        }
        else
        {
            Debug.LogWarning($"Pistol: 총알({bullet.name})에 Bullet 컴포넌트가 없습니다!");
        }
        
        Debug.Log($"✅ 총알 발사 성공: 위치={_firePoint.position}, 방향={_firePoint.forward}");
    }
    
    // --- 아래는 IWeapon 인터페이스 규칙을 지키기 위한 필수 함수들입니다 ---

    public void OnEquip(PlayerCombat combat)
    {
        Debug.Log("Pistol.OnEquip 호출됨!");
        _playerCombat = combat;
        
        // WeaponManager에서 장착된 무기 오브젝트 가져오기
        var weaponManager = combat.GetComponent<WeaponManager>();
        if (weaponManager != null && weaponManager.EquippedWeaponObject != null)
        {
            Debug.Log($"Pistol: 무기 오브젝트 찾음: {weaponManager.EquippedWeaponObject.name}");
            
            // 무기 오브젝트의 자식에서 FirePoint 찾기
            _firePoint = weaponManager.EquippedWeaponObject.transform.Find("FirePoint");
            
            if (_firePoint != null)
            {
                Debug.Log($"Pistol: FirePoint를 무기 모델에서 찾았습니다! 위치: {_firePoint.position}, 회전: {_firePoint.rotation.eulerAngles}");
            }
            else
            {
                Debug.LogWarning("Pistol: FirePoint를 무기 모델에서 찾을 수 없습니다. 무기 오브젝트를 사용합니다.");
                _firePoint = weaponManager.EquippedWeaponObject.transform;
            }
        }
        else
        {
            // WeaponManager가 없거나 무기 오브젝트가 없으면 PlayerCombat 사용
            Debug.LogWarning("Pistol: WeaponManager 또는 장착된 무기 오브젝트를 찾을 수 없습니다. PlayerCombat transform을 사용합니다.");
            _firePoint = combat.transform;
        }
        
        // 최종 상태 확인
        Debug.Log($"Pistol OnEquip 완료: bulletPrefab={(_bulletPrefab != null ? "OK" : "NULL")}, firePoint={(_firePoint != null ? "OK" : "NULL")}, playerCombat={(_playerCombat != null ? "OK" : "NULL")}");
    }

    public void OnUnequip(PlayerCombat combat)
    {
        _playerCombat = null;
        _firePoint = null;
    }

    public float GetCurrentAttackAnimationLength()
    {
        // 약공격(단발)과 강공격(3점사)의 애니메이션 길이 다름
        if (_currentAttackType == AttackType.HeavyAttack)
        {
            return PISTOL_BURST_SHOT_LENGTH;
        }
        return PISTOL_SINGLE_SHOT_LENGTH;
    }

    public float GetCurrentAttackDamage()
    {
        // 예시 값: ScriptableObject로 옮길 수 있음
        return 15f;
    }
    
    public float GetCurrentAttackStaminaCost()
    {
        // 약공격(단발)은 2.5, 강공격(3점사)은 5
        if (_currentAttackType == AttackType.LightAttack)
        {
            return 2.5f; // Z - 단발
        }
        else if (_currentAttackType == AttackType.HeavyAttack)
        {
            return 5f; // X - 3점사
        }
        return 0f;
    }
    
    // 콤보 상태 관리 - 권총은 콤보가 없음
    public void ResetCombo()
    {
        // 권총은 콤보가 없으므로 아무 작업도 하지 않음
    }
    
    public bool IsInCombo()
    {
        return false; // 권총은 항상 콤보 상태가 아님
    }
}