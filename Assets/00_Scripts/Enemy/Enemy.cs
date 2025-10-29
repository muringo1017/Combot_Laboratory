using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float health = 100f;
    
    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log($"{gameObject.name} 체력: {health}");
        
        if (health <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        Debug.Log($"{gameObject.name} 사망!");
        Destroy(gameObject);
    }
}