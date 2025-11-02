using UnityEngine;

public class SideScrollCamera : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.3f; 
    public float fixedYPosition = 5f; 
    public float fixedZPosition = -10f; 

    private Vector3 velocity = Vector3.zero;
    private Vector3 _shakeOffset = Vector3.zero; // CameraShake offset
    
    // CameraShake에서 호출할 수 있도록 public으로
    public Vector3 ShakeOffset
    {
        get => _shakeOffset;
        set => _shakeOffset = value;
    }
    
    // 💡 변경: LateUpdate로 변경하여 CameraShake 이후에 실행
    void LateUpdate() 
    {
        if (target == null) return;

        // 1. 목표 위치 계산
        Vector3 desiredPosition = new Vector3(
            target.position.x, 
            fixedYPosition, 
            fixedZPosition
        );

        // 2. SmoothDamp를 사용하여 부드럽게 이동
        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position, 
            desiredPosition, 
            ref velocity,      
            smoothTime
        );
        
        // 3. 카메라 위치 업데이트 (shake offset 적용)
        transform.position = smoothedPosition + _shakeOffset;
    }
}