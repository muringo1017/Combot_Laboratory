using UnityEngine;

/// <summary>
/// 기존 Enemy - 호환성을 위해 HandgunEnemy를 상속받도록 변경
/// (기존 프리팹들이 작동하도록 유지)
/// </summary>
public class Enemy : HandgunEnemy
{
    // 기존 Enemy를 HandgunEnemy로 변경하여 호환성 유지
    // 모든 기능은 HandgunEnemy와 BaseEnemy에서 제공됨
    
    protected override void Awake()
    {
        base.Awake();
        
        // MyBT와의 호환성을 위한 초기화
        Debug.Log($"[Enemy] 초기화 완료 (HandgunEnemy 타입)");
    }
    
    // MyBT 호환성을 위한 레거시 프로퍼티들
    // CanShoot는 BaseEnemy에서 제공됨 (canShoot 필드)
    public float ShootRange => GetAttackRange();
    public bool IsShooting => !CanAttack();
    
    // MyBT 호환성을 위한 레거시 메서드
    public void StartShootCoroutine(Transform target)
    {
        Debug.Log($"[Enemy] StartShootCoroutine 호출됨! target={target?.name}");
        PerformAttack(target);
    }
}