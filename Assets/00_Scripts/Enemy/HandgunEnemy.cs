using System.Collections;
using UnityEngine;

/// <summary>
/// 권총 공격 Enemy - 단발 사격
/// </summary>
public class HandgunEnemy : BaseEnemy
{
    [Header("Handgun Settings")]
    [SerializeField] private float attackRange = 10f; // 원거리 공격 범위
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private float bulletDamage = 10f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float shootDelay = 0.5f; // 애니메이션 후 발사 딜레이
    
    private float _lastAttackTime = 0f;
    private bool _isShooting = false;
    
    protected override void Awake()
    {
        base.Awake();
        canShoot = true; // 권총 Enemy는 원거리 공격 가능
        Debug.Log($"[HandgunEnemy] Awake - canShoot={canShoot}");
    }
    
    public override float GetAttackRange()
    {
        return attackRange;
    }
    
    public override bool CanAttack()
    {
        return !_isShooting && Time.time - _lastAttackTime >= attackCooldown;
    }
    
    public override void PerformAttack(Transform target)
    {
        Debug.Log($"[HandgunEnemy] PerformAttack 호출됨! CanAttack={CanAttack()}, target={target?.name}");
        Debug.Log($"[HandgunEnemy] bulletPrefab={(bulletPrefab != null ? "설정됨" : "NULL")}, firePoint={(firePoint != null ? firePoint.name : "NULL")}");
        
        if (!CanAttack())
        {
            Debug.LogWarning($"[HandgunEnemy] 공격 불가! _isShooting={_isShooting}, 쿨다운={(Time.time - _lastAttackTime)}/{attackCooldown}");
            return;
        }
        
        if (target == null)
        {
            Debug.LogWarning("[HandgunEnemy] Target이 NULL!");
            return;
        }
        
        Debug.Log($"[HandgunEnemy] 단발 사격 시작!");
        
        // 코루틴으로 딜레이 후 발사
        StartCoroutine(ShootCoroutine(target));
    }
    
    private IEnumerator ShootCoroutine(Transform target)
    {
        _isShooting = true;
        
        Debug.Log($"[HandgunEnemy] ShootCoroutine 시작! {shootDelay}초 대기...");
        
        // 애니메이션 딜레이
        yield return new WaitForSeconds(shootDelay);
        
        Debug.Log($"[HandgunEnemy] 딜레이 완료! bulletPrefab={(bulletPrefab != null ? "OK" : "NULL")}, target={(target != null ? "OK" : "NULL")}");
        
        // 유효성 체크
        if (bulletPrefab == null)
        {
            Debug.LogError("[HandgunEnemy] bulletPrefab이 NULL입니다! 인스펙터에서 설정해주세요.");
            _isShooting = false;
            yield break;
        }
        
        if (target == null)
        {
            Debug.LogWarning("[HandgunEnemy] target이 NULL입니다!");
            _isShooting = false;
            yield break;
        }
        
        // 발사 위치
        Transform shootPoint = firePoint != null ? firePoint : transform;
        
        // 방향 계산 (x축만)
        Vector3 directionToTarget = (target.position - shootPoint.position);
        float xDirection = Mathf.Sign(directionToTarget.x);
        
        // 총알 생성
        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
        bullet.name = "HandgunBullet";
        
        // 총알 레이어 설정
        int enemyProjectileLayer = LayerMask.NameToLayer("EnemyProjectile");
        if (enemyProjectileLayer != -1)
        {
            bullet.layer = enemyProjectileLayer;
        }
        
        // 회전
        bullet.transform.rotation = Quaternion.Euler(0, xDirection > 0 ? 90 : -90, 0);
        
        // 속도 설정 (x축만)
        var bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = new Vector3(xDirection * bulletSpeed, 0f, 0f);
            bulletRb.useGravity = false;
        }
        
        // 데미지 설정
        var ammoComponent = bullet.GetComponent<EnemyAmmo>();
        if (ammoComponent != null)
        {
            ammoComponent.SetDamage(bulletDamage);
        }
        
        Debug.Log($"✅ [HandgunEnemy] 총알 발사!");
        
        _lastAttackTime = Time.time;
        _isShooting = false;
    }
}

