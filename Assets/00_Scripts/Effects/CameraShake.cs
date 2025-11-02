using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }
    
    [Header("Shake Settings")]
    [SerializeField] private float defaultDuration = 0.2f;
    [SerializeField] private float defaultMagnitude = 0.15f; // 0.3 → 0.15
    
    private bool _isShaking = false;
    private SideScrollCamera _sideScrollCamera; // SideScrollCamera 참조
    
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
        
        // SideScrollCamera 찾기
        _sideScrollCamera = GetComponent<SideScrollCamera>();
        if (_sideScrollCamera == null)
        {
            Debug.LogWarning("[CameraShake] SideScrollCamera를 찾을 수 없습니다. 일반 카메라 쉐이크를 사용합니다.");
        }
    }
    
    /// <summary>
    /// 카메라를 흔듭니다.
    /// </summary>
    /// <param name="duration">흔들림 지속 시간</param>
    /// <param name="magnitude">흔들림 강도</param>
    public void Shake(float duration = -1f, float magnitude = -1f)
    {
        if (duration < 0) duration = defaultDuration;
        if (magnitude < 0) magnitude = defaultMagnitude;
        
        if (!_isShaking)
        {
            StartCoroutine(ShakeCoroutine(duration, magnitude));
        }
    }
    
    /// <summary>
    /// 약한 흔들림 (가벼운 공격)
    /// </summary>
    public void ShakeLight()
    {
        Shake(0.1f, 0.075f); // 0.15 → 0.075
    }
    
    /// <summary>
    /// 중간 흔들림 (일반 공격)
    /// </summary>
    public void ShakeMedium()
    {
        Shake(0.15f, 0.1f); // 0.25 → 0.125
    }
    
    /// <summary>
    /// 강한 흔들림 (강공격)
    /// </summary>
    public void ShakeHeavy()
    {
        Shake(0.25f, 0.1f); // 0.4 → 0.2
    }
    
    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        _isShaking = true;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            // 랜덤한 방향으로 카메라 흔들기
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            Vector3 shakeOffset = new Vector3(x, y, 0);
            
            // SideScrollCamera가 있으면 offset을 전달, 없으면 직접 위치 변경
            if (_sideScrollCamera != null)
            {
                _sideScrollCamera.ShakeOffset = shakeOffset;
            }
            else
            {
                transform.position += shakeOffset;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 원래 위치로 복귀
        if (_sideScrollCamera != null)
        {
            _sideScrollCamera.ShakeOffset = Vector3.zero;
        }
        
        _isShaking = false;
    }
    
    /// <summary>
    /// 강제로 흔들림 중지
    /// </summary>
    public void StopShake()
    {
        StopAllCoroutines();
        
        if (_sideScrollCamera != null)
        {
            _sideScrollCamera.ShakeOffset = Vector3.zero;
        }
        
        _isShaking = false;
    }
}


