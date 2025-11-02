using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private LayerMask enemyLayer = 1; // Default layer
    
    private float _spawnTime;
    
    private void Awake()
    {
        _spawnTime = Time.time;
        Debug.Log($"[Bullet] Awake: {gameObject.name} 생성됨, 위치: {transform.position}");
        
        // Trail Renderer 자동 추가 (없으면)
        EnsureTrailRenderer();
    }
    
    private void EnsureTrailRenderer()
    {
        // BulletTrail 컴포넌트가 없으면 자동으로 추가
        var bulletTrail = GetComponent<BulletTrail>();
        if (bulletTrail == null)
        {
            gameObject.AddComponent<BulletTrail>();
            Debug.Log("[Bullet] BulletTrail 컴포넌트를 자동으로 추가했습니다.");
        }
    }
    
    private void Start()
    {
        Debug.Log($"[Bullet] Start: {gameObject.name} 시작, 위치: {transform.position}, Rigidbody: {(GetComponent<Rigidbody>() != null ? "있음" : "없음")}");
        
        // 일정 시간 후 자동으로 파괴
        Destroy(gameObject, lifetime);
        Debug.Log($"[Bullet] {lifetime}초 후 파괴 예약됨");
    }
    
    private void OnDestroy()
    {
        float lifeTime = Time.time - _spawnTime;
        Debug.Log($"[Bullet] 파괴됨: {gameObject.name}, 생존시간: {lifeTime:F2}초");
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Bullet] OnTriggerEnter: {other.gameObject.name}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}");
        
        // Player나 무기와 충돌하면 무시
        if (other.gameObject.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log($"[Bullet] Player와 충돌 - 무시");
            return;
        }
        
        // Enemy 레이어와 충돌했는지 확인
        if (IsInLayerMask(other.gameObject.layer, enemyLayer))
        {
            // Enemy에게 데미지 전달 (BaseEnemy 타입 체크)
            var enemy = other.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"[Bullet] Enemy에게 {damage} 데미지를 입혔습니다!");
                
                // 타격 위치 및 방향
                Vector3 hitPosition = other.ClosestPoint(transform.position);
                Vector3 hitDirection = transform.forward;
                
                // 타격감 효과
                ApplyImpactFeedback(hitPosition, hitDirection);
            }
            
            // 총알 파괴
            Debug.Log($"[Bullet] Enemy 충돌로 파괴");
            Destroy(gameObject);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[Bullet] OnCollisionEnter: {collision.gameObject.name}, Layer: {LayerMask.LayerToName(collision.gameObject.layer)}");
        
        // Player나 무기와 충돌하면 무시
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log($"[Bullet] Player와 충돌 - 무시");
            return;
        }
        
        // 벽이나 다른 오브젝트와 충돌 시 총알 파괴
        if (!IsInLayerMask(collision.gameObject.layer, enemyLayer))
        {
            Debug.Log($"[Bullet] 벽/오브젝트 충돌로 파괴: {collision.gameObject.name}");
            Destroy(gameObject);
        }
    }
    
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
    
    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return layerMask == (layerMask | (1 << layer));
    }
    
    // 타격감 피드백 적용
    private void ApplyImpactFeedback(Vector3 hitPosition, Vector3 hitDirection)
    {
        // 카메라 쉐이크 (총알은 가벼운 흔들림)
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeLight();
        }
        
        // 히트스탑 (총알은 짧은 정지)
        if (HitStop.Instance != null)
        {
            HitStop.Instance.StopLight();
        }
        
        // 총알 히트 이펙트
        if (HitEffectManager.Instance != null)
        {
            HitEffectManager.Instance.PlayBulletHitEffect(hitPosition, -hitDirection);
        }
    }
}

