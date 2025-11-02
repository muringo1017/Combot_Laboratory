using System.Collections;
using UnityEngine;

public class HitEffectManager : MonoBehaviour
{
    public static HitEffectManager Instance { get; private set; }
    
    [Header("Hit Effect Prefabs")]
    [SerializeField] private GameObject lightHitEffectPrefab;   // 약공격 이펙트
    [SerializeField] private GameObject mediumHitEffectPrefab;  // 중간 타격 이펙트
    [SerializeField] private GameObject heavyHitEffectPrefab;   // 강공격 이펙트
    [SerializeField] private GameObject bulletHitEffectPrefab;  // 총알 히트 이펙트
    
    [Header("Player Hit Effects")]
    [SerializeField] private GameObject playerHitEffectPrefab;  // 플레이어 피격 이펙트
    
    [Header("Effect Settings")]
    [SerializeField] private float effectLifetime = 2f; // 이펙트 자동 파괴 시간
    
    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    /// <summary>
    /// 약한 타격 이펙트 (데미지 < 10)
    /// </summary>
    public void PlayLightHitEffect(Vector3 position, Vector3 normal)
    {
        PlayEffect(lightHitEffectPrefab, position, normal, "Light Hit");
    }
    
    /// <summary>
    /// 중간 타격 이펙트 (10 <= 데미지 < 20)
    /// </summary>
    public void PlayMediumHitEffect(Vector3 position, Vector3 normal)
    {
        PlayEffect(mediumHitEffectPrefab, position, normal, "Medium Hit");
    }
    
    /// <summary>
    /// 강한 타격 이펙트 (데미지 >= 20)
    /// </summary>
    public void PlayHeavyHitEffect(Vector3 position, Vector3 normal)
    {
        PlayEffect(heavyHitEffectPrefab, position, normal, "Heavy Hit");
    }
    
    /// <summary>
    /// 총알 히트 이펙트
    /// </summary>
    public void PlayBulletHitEffect(Vector3 position, Vector3 normal)
    {
        PlayEffect(bulletHitEffectPrefab, position, normal, "Bullet Hit");
    }
    
    /// <summary>
    /// 플레이어 피격 이펙트
    /// </summary>
    public void PlayPlayerHitEffect(Vector3 position)
    {
        PlayEffect(playerHitEffectPrefab, position, Vector3.up, "Player Hit");
    }
    
    /// <summary>
    /// 데미지에 따라 자동으로 적절한 이펙트 재생
    /// </summary>
    public void PlayHitEffectByDamage(float damage, Vector3 position, Vector3 normal)
    {
        if (damage < 10f)
        {
            PlayLightHitEffect(position, normal);
        }
        else if (damage < 20f)
        {
            PlayMediumHitEffect(position, normal);
        }
        else
        {
            PlayHeavyHitEffect(position, normal);
        }
    }
    
    private void PlayEffect(GameObject effectPrefab, Vector3 position, Vector3 normal, string effectName)
    {
        if (effectPrefab == null)
        {
            Debug.LogWarning($"[HitEffectManager] {effectName} 프리팹이 null입니다! Inspector에서 할당하세요.");
            return;
        }
        
        // 이펙트 생성
        GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);
        effect.name = $"{effectName}_Effect";
        
        // 법선 방향으로 회전 (타격 방향 반대로 이펙트 재생)
        if (normal != Vector3.zero)
        {
            effect.transform.rotation = Quaternion.LookRotation(normal);
        }
        
        Debug.Log($"[HitEffectManager] {effectName} 이펙트 재생: 위치={position}");
        
        // 일정 시간 후 자동 파괴
        Destroy(effect, effectLifetime);
    }
    
    /// <summary>
    /// 간단한 플래시 이펙트 (이펙트 프리팹이 없을 때 사용)
    /// </summary>
    public void PlaySimpleFlash(Vector3 position, Color color, float size = 0.5f)
    {
        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.name = "SimpleFlash";
        flash.transform.position = position;
        flash.transform.localScale = Vector3.one * size;
        
        // Collider 제거
        Destroy(flash.GetComponent<Collider>());
        
        // Material 설정
        var renderer = flash.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
            // Emission 설정 (발광 효과)
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", color * 2f);
        }
        
        // 페이드 아웃 애니메이션
        StartCoroutine(FadeOutAndDestroy(flash, 0.3f));
    }
    
    private IEnumerator FadeOutAndDestroy(GameObject obj, float duration)
    {
        float elapsed = 0f;
        var renderer = obj.GetComponent<Renderer>();
        
        while (elapsed < duration && renderer != null)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / duration);
            
            Color color = renderer.material.color;
            color.a = alpha;
            renderer.material.color = color;
            
            // 크기도 함께 증가
            obj.transform.localScale *= 1.02f;
            
            yield return null;
        }
        
        Destroy(obj);
    }
}


