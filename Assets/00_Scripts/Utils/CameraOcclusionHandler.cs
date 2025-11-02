using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 카메라와 플레이어 사이의 오브젝트를 투명하게 만들어 플레이어가 항상 보이도록 합니다.
/// </summary>
public class CameraOcclusionHandler : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target; // 플레이어
    
    [Header("Occlusion Settings")]
    [SerializeField] private LayerMask occlusionLayers; // 투명화할 레이어
    [SerializeField] private float transparentAlpha = 0.3f; // 투명도 (0~1)
    [SerializeField] private float fadeSpeed = 5f; // 페이드 속도
    
    [Header("Raycast Settings")]
    [SerializeField] private float raycastRadius = 0.5f; // SphereCast 반경
    
    // 현재 투명화된 오브젝트들
    private Dictionary<Renderer, Material[]> _fadedObjects = new Dictionary<Renderer, Material[]>();
    private Dictionary<Material, float> _originalAlpha = new Dictionary<Material, float>();
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        // 이전 프레임에 투명화된 오브젝트 복원
        RestoreFadedObjects();
        
        // 카메라와 플레이어 사이의 오브젝트 찾기
        CheckForOcclusion();
    }
    
    private void CheckForOcclusion()
    {
        Vector3 direction = target.position - transform.position;
        float distance = direction.magnitude;
        
        // SphereCast로 카메라와 플레이어 사이의 모든 오브젝트 검출
        RaycastHit[] hits = Physics.SphereCastAll(
            transform.position,
            raycastRadius,
            direction.normalized,
            distance,
            occlusionLayers,
            QueryTriggerInteraction.Ignore
        );
        
        // 플레이어 자신은 제외
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == target) continue;
            if (hit.transform.IsChildOf(target)) continue; // 플레이어 자식도 제외
            
            // 오브젝트를 투명하게
            FadeObject(hit.transform);
        }
    }
    
    private void FadeObject(Transform obj)
    {
        // 모든 Renderer 찾기
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;
            
            // 이미 처리 중인지 확인
            if (_fadedObjects.ContainsKey(renderer)) continue;
            
            // Material 복사 (원본 보존)
            Material[] originalMaterials = renderer.materials;
            Material[] fadedMaterials = new Material[originalMaterials.Length];
            
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                // Material 인스턴스 생성 (원본 보호)
                fadedMaterials[i] = new Material(originalMaterials[i]);
                
                // 원래 알파값 저장
                Color color = fadedMaterials[i].color;
                if (!_originalAlpha.ContainsKey(originalMaterials[i]))
                {
                    _originalAlpha[originalMaterials[i]] = color.a;
                }
                
                // 투명 모드로 전환
                SetMaterialTransparent(fadedMaterials[i]);
                
                // 알파값 설정
                color.a = transparentAlpha;
                fadedMaterials[i].color = color;
            }
            
            // 투명한 Material 적용
            renderer.materials = fadedMaterials;
            _fadedObjects[renderer] = originalMaterials;
            
            Debug.Log($"[CameraOcclusion] 오브젝트 투명화: {obj.name}");
        }
    }
    
    private void RestoreFadedObjects()
    {
        // 이전에 투명화된 모든 오브젝트 복원
        foreach (var kvp in _fadedObjects)
        {
            Renderer renderer = kvp.Key;
            Material[] originalMaterials = kvp.Value;
            
            if (renderer != null)
            {
                renderer.materials = originalMaterials;
                Debug.Log($"[CameraOcclusion] 오브젝트 복원: {renderer.name}");
            }
        }
        
        _fadedObjects.Clear();
    }
    
    private void SetMaterialTransparent(Material material)
    {
        // Standard Shader의 Rendering Mode를 Transparent로 변경
        material.SetFloat("_Mode", 3); // Transparent mode
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;
        
        // 디버그용: 카메라와 플레이어 사이의 선 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, target.position);
        
        // SphereCast 범위 표시
        Gizmos.color = Color.red;
        Vector3 direction = (target.position - transform.position).normalized;
        Gizmos.DrawWireSphere(transform.position + direction * 2f, raycastRadius);
    }
}


