using UnityEngine;

/// <summary>
/// 카메라와 플레이어 사이에 장애물이 있으면 카메라를 줌인합니다.
/// </summary>
public class CameraCollisionZoom : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target; // 플레이어
    
    [Header("Zoom Settings")]
    [SerializeField] private float normalZDistance = -10f; // 기본 Z 거리
    [SerializeField] private float minZDistance = -3f; // 최소 Z 거리 (최대 줌인)
    [SerializeField] private float zoomSpeed = 10f; // 줌 속도
    
    [Header("Collision Settings")]
    [SerializeField] private LayerMask collisionLayers; // 충돌 체크할 레이어
    [SerializeField] private float sphereRadius = 0.3f; // SphereCast 반경
    
    private SideScrollCamera _sideScrollCamera;
    private float _targetZDistance;
    
    private void Awake()
    {
        _sideScrollCamera = GetComponent<SideScrollCamera>();
        _targetZDistance = normalZDistance;
    }
    
    private void Update()
    {
        if (target == null) return;
        
        CheckForCollision();
        UpdateZoom();
    }
    
    private void CheckForCollision()
    {
        Vector3 direction = target.position - transform.position;
        float distance = direction.magnitude;
        
        // SphereCast로 장애물 검출
        if (Physics.SphereCast(
            transform.position,
            sphereRadius,
            direction.normalized,
            out RaycastHit hit,
            distance,
            collisionLayers,
            QueryTriggerInteraction.Ignore))
        {
            // 장애물이 있으면 줌인
            if (hit.transform != target && !hit.transform.IsChildOf(target))
            {
                // 장애물까지의 거리에 따라 줌인 정도 조정
                float hitDistance = hit.distance;
                float zoomRatio = hitDistance / distance;
                _targetZDistance = Mathf.Lerp(minZDistance, normalZDistance, zoomRatio);
                
                Debug.Log($"[CameraCollision] 장애물 감지: {hit.transform.name}, 거리: {hitDistance:F2}, 줌: {_targetZDistance:F2}");
            }
        }
        else
        {
            // 장애물이 없으면 정상 거리로 복귀
            _targetZDistance = normalZDistance;
        }
    }
    
    private void UpdateZoom()
    {
        if (_sideScrollCamera == null) return;
        
        // 부드럽게 줌 적용
        _sideScrollCamera.fixedZPosition = Mathf.Lerp(
            _sideScrollCamera.fixedZPosition,
            _targetZDistance,
            Time.deltaTime * zoomSpeed
        );
    }
    
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;
        
        // SphereCast 경로 표시
        Gizmos.color = Color.cyan;
        Vector3 direction = (target.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, target.position);
        
        // 시작점
        Gizmos.DrawWireSphere(transform.position, sphereRadius);
        
        // 끝점
        Gizmos.DrawWireSphere(transform.position + direction * distance, sphereRadius);
        
        // 연결선
        Gizmos.DrawLine(transform.position, target.position);
    }
}


