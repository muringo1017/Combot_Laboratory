using UnityEngine;

public class SideScrollCamera : MonoBehaviour
{
    // ... (이전과 동일한 public 변수들)
    public Transform target;
    public float smoothTime = 0.3f; 
    public float fixedYPosition = 5f; 
    public float fixedZPosition = -10f; 

    private Vector3 velocity = Vector3.zero;
    
    // 💡 변경: LateUpdate 대신 FixedUpdate를 사용하여 물리 주기와 동기화
    void FixedUpdate() 
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
        
        // 3. 카메라 위치 업데이트
        transform.position = smoothedPosition;
    }
}