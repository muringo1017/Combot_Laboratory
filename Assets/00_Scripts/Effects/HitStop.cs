using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    public static HitStop Instance { get; private set; }
    
    [Header("HitStop Settings")]
    [SerializeField] private float defaultDuration = 0.1f;
    
    private bool _isStopped = false;
    
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
    /// 시간을 일시 정지합니다 (타격감 효과)
    /// </summary>
    /// <param name="duration">정지 시간 (초)</param>
    public void Stop(float duration = -1f)
    {
        if (duration < 0) duration = defaultDuration;
        
        if (!_isStopped)
        {
            StartCoroutine(StopCoroutine(duration));
        }
    }
    
    /// <summary>
    /// 약한 히트스탑 (가벼운 타격)
    /// </summary>
    public void StopLight()
    {
        Stop(0.05f);
    }
    
    /// <summary>
    /// 중간 히트스탑 (일반 타격)
    /// </summary>
    public void StopMedium()
    {
        Stop(0.08f);
    }
    
    /// <summary>
    /// 강한 히트스탑 (강타격)
    /// </summary>
    public void StopHeavy()
    {
        Stop(0.12f);
    }
    
    private IEnumerator StopCoroutine(float duration)
    {
        _isStopped = true;
        
        // 시간 정지
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        
        Debug.Log($"[HitStop] 시간 정지: {duration}초");
        
        // 실제 시간으로 대기 (unscaledTime 사용)
        yield return new WaitForSecondsRealtime(duration);
        
        // 시간 복원
        Time.timeScale = originalTimeScale;
        
        Debug.Log("[HitStop] 시간 복원");
        _isStopped = false;
    }
    
    /// <summary>
    /// 강제로 시간 복원
    /// </summary>
    public void Resume()
    {
        if (_isStopped)
        {
            StopAllCoroutines();
            Time.timeScale = 1f;
            _isStopped = false;
        }
    }
}



