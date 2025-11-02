using UnityEngine;

public class WeaponSpin : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 90f; // 초당 회전 각도
    [SerializeField] private bool isSpinning = false;
    
    private void Update()
    {
        if (isSpinning)
        {
            // Y축을 중심으로 회전
            transform.Rotate(0, spinSpeed * Time.deltaTime, 0);
        }
    }
    
    public void StartSpinning()
    {
        isSpinning = true;
    }
    
    public void StopSpinning()
    {
        isSpinning = false;
    }
    
    // 무기를 줍을 때 회전 중지
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopSpinning();
        }
    }
}



