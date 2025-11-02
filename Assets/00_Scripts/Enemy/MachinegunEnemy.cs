using System.Collections;
using UnityEngine;

/// <summary>
/// 기관총 Enemy - 연사 공격
/// </summary>
public class MachinegunEnemy : BaseEnemy
{
    [Header("Machinegun Settings")]
    [SerializeField] private float attackRange = 12f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletDamage = 5f; // 연사라서 데미지 낮음
    [SerializeField] private float attackCooldown = 3f; // 연사 쿨다운
    [SerializeField] private int burstCount = 5; // 연사 발수
    [SerializeField] private float burstInterval = 0.15f; // 연사 간격
    [SerializeField] private float shootDelay = 0.3f; // 애니메이션 후 발사 딜레이
    
    private float _lastAttackTime = 0f;
    private bool _isShooting = false;
    
    protected override void Awake()
    {
        base.Awake();
        canShoot = true; // 기관총 Enemy는 원거리 공격 가능
        Debug.Log($"[MachinegunEnemy] Awake - canShoot={canShoot}");
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
        if (!CanAttack() || target == null)
        {
            Debug.LogWarning("[MachinegunEnemy] 공격 불가!");
            return;
        }
        
        Debug.Log($"[MachinegunEnemy] 기관총 연사 시작! {burstCount}발");
        
        // 코루틴으로 연사 처리
        StartCoroutine(BurstFireCoroutine(target));
    }
    
    private IEnumerator BurstFireCoroutine(Transform target)
    {
        _isShooting = true;
        
        // 애니메이션 딜레이
        yield return new WaitForSeconds(shootDelay);
        
        // 연사 시작
        for (int i = 0; i < burstCount; i++)
        {
            // 유효성 체크
            if (bulletPrefab == null || target == null || _isDead)
            {
                break;
            }
            
            // 발사 위치
            Transform shootPoint = firePoint != null ? firePoint : transform;
            
            // 방향 계산 (x축만)
            Vector3 directionToTarget = (target.position - shootPoint.position);
            float xDirection = Mathf.Sign(directionToTarget.x);
            
            // 총알 생성
            GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
            bullet.name = $"MachinegunBullet_{i+1}";
            
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
            
            Debug.Log($"💥 [MachinegunEnemy] 연사 {i+1}/{burstCount}");
            
            // 마지막 발이 아니면 대기
            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(burstInterval);
            }
        }
        
        Debug.Log($"✅ [MachinegunEnemy] 연사 완료!");
        
        _lastAttackTime = Time.time;
        _isShooting = false;
    }
}

