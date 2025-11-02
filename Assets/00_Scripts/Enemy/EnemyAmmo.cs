using UnityEngine;

public class EnemyAmmo : MonoBehaviour
{
    [Header("Ammo Settings")]
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float damage = 10f;
    
    private float _spawnTime;
    
    private void Awake()
    {
        _spawnTime = Time.time;
        Debug.Log($"[EnemyAmmo] Awake: {gameObject.name} 생성됨, 위치: {transform.position}");
        
        // Trail Renderer 자동 추가 (적 총알은 빨간색)
        EnsureTrailRenderer();
    }
    
    private void EnsureTrailRenderer()
    {
        // BulletTrail 컴포넌트가 없으면 자동으로 추가
        var bulletTrail = GetComponent<BulletTrail>();
        if (bulletTrail == null)
        {
            bulletTrail = gameObject.AddComponent<BulletTrail>();
            // 적 총알은 빨간색 Trail
            bulletTrail.SetTrailColor(Color.red, new Color(1f, 0f, 0f, 0f));
            Debug.Log("[EnemyAmmo] BulletTrail 컴포넌트를 자동으로 추가했습니다 (빨간색).");
        }
    }
    
    private void Start()
    {
        Debug.Log($"[EnemyAmmo] Start: {gameObject.name} 시작, 위치: {transform.position}, Rigidbody: {(GetComponent<Rigidbody>() != null ? "있음" : "없음")}");
        
        // 일정 시간 후 자동으로 파괴
        Destroy(gameObject, lifetime);
        Debug.Log($"[EnemyAmmo] {lifetime}초 후 파괴 예약됨");
    }
    
    private void OnDestroy()
    {
        float lifeTime = Time.time - _spawnTime;
        Debug.Log($"[EnemyAmmo] 파괴됨: {gameObject.name}, 생존시간: {lifeTime:F2}초");
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[EnemyAmmo] OnTriggerEnter: {other.gameObject.name}, Tag: {other.tag}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}");
        
        // Enemy와 충돌하면 무시 (BaseEnemy 타입으로 체크)
        if (other.GetComponent<BaseEnemy>() != null)
        {
            Debug.Log($"[EnemyAmmo] Enemy와 충돌 - 무시");
            return;
        }
        
        // Player와 충돌했는지 확인
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[EnemyAmmo] Player와 충돌!");
            
            // 무적 레이어 체크
            int invincibleLayer = LayerMask.NameToLayer("Invincible");
            if (invincibleLayer != -1 && other.gameObject.layer == invincibleLayer)
            {
                Debug.Log($"[EnemyAmmo] Player가 무적 상태! (Invincible Layer) - 데미지 무시");
                Destroy(gameObject); // 총알은 파괴
                return;
            }
            
            // Player에게 데미지 전달
            var playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"[EnemyAmmo] Player에게 {damage} 데미지를 입혔습니다!");
                
                // 타격 위치
                Vector3 hitPosition = other.ClosestPoint(transform.position);
                
                // 플레이어 피격 시 타격감 효과
                ApplyPlayerHitFeedback(hitPosition);
            }
            else
            {
                Debug.LogWarning($"[EnemyAmmo] Player에 PlayerHealth 컴포넌트가 없습니다!");
            }
            
            // 총알 파괴
            Debug.Log($"[EnemyAmmo] Player 충돌로 파괴");
            Destroy(gameObject);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[EnemyAmmo] OnCollisionEnter: {collision.gameObject.name}, Tag: {collision.gameObject.tag}, Layer: {LayerMask.LayerToName(collision.gameObject.layer)}");
        
        // Enemy와 충돌하면 무시 (BaseEnemy 타입으로 체크)
        if (collision.gameObject.GetComponent<BaseEnemy>() != null)
        {
            Debug.Log($"[EnemyAmmo] Enemy와 충돌 - 무시");
            return;
        }
        
        // Player와 충돌
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"[EnemyAmmo] Player와 충돌!");
            
            // 무적 레이어 체크
            int invincibleLayer = LayerMask.NameToLayer("Invincible");
            if (invincibleLayer != -1 && collision.gameObject.layer == invincibleLayer)
            {
                Debug.Log($"[EnemyAmmo] Player가 무적 상태! (Invincible Layer) - 데미지 무시");
                Destroy(gameObject); // 총알은 파괴
                return;
            }
            
            var playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"[EnemyAmmo] Player에게 {damage} 데미지를 입혔습니다!");
            }
            
            Destroy(gameObject);
        }
        else
        {
            // 벽이나 다른 오브젝트와 충돌 시 총알 파괴
            Debug.Log($"[EnemyAmmo] 벽/오브젝트 충돌로 파괴: {collision.gameObject.name}");
            Destroy(gameObject);
        }
    }
    
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
    
    // 플레이어 피격 시 타격감 피드백
    private void ApplyPlayerHitFeedback(Vector3 hitPosition)
    {
        // 카메라 쉐이크 (플레이어 피격은 중간 흔들림)
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeMedium();
        }
        
        // 히트스탑
        if (HitStop.Instance != null)
        {
            HitStop.Instance.StopMedium();
        }
        
        // 플레이어 피격 이펙트
        if (HitEffectManager.Instance != null)
        {
            HitEffectManager.Instance.PlayPlayerHitEffect(hitPosition);
        }
    }
}


