using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    
    private Rigidbody _rigidbody;
    private bool _facingRight = true;
    
    // 액션 중 방향 잠금 (0 = 잠금 없음, 1 = 오른쪽 잠금, -1 = 왼쪽 잠금)
    private float _lockedDirection = 0f;
    
    // 외부에서 방향 확인 가능하도록
    public bool IsFacingRight => _facingRight;
    public Vector3 FacingDirection => _facingRight ? Vector3.right : Vector3.left;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Move(float horizontalInput)
    {
        // 방향이 잠겨있고 반대 방향 입력이면 무시
        if (_lockedDirection != 0f)
        {
            // 잠긴 방향과 입력 방향의 부호가 반대면 입력 무시
            if (Mathf.Sign(horizontalInput) != 0 && Mathf.Sign(horizontalInput) != Mathf.Sign(_lockedDirection))
            {
                return;
            }
        }
        
        Vector3 velocity = _rigidbody.linearVelocity;
        velocity.x = horizontalInput * moveSpeed;
        _rigidbody.linearVelocity = velocity;

        if (horizontalInput > 0 && !_facingRight) Flip();
        else if (horizontalInput < 0 && _facingRight) Flip();
    }
    
    public void Stop()
    {
        Vector3 velocity = _rigidbody.linearVelocity;
        velocity.x = 0;
        _rigidbody.linearVelocity = velocity;
    }
    
    // 현재 바라보는 방향으로 이동 방향 잠금
    public void LockDirection()
    {
        _lockedDirection = _facingRight ? 1f : -1f;
    }
    
    // 이동 방향 잠금 해제
    public void UnlockDirection()
    {
        _lockedDirection = 0f;
    }

    private void Flip()
    {
        _facingRight = !_facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}