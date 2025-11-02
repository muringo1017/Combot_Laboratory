using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina;
    [SerializeField] private float staminaRegenRate = 2f; // 초당 회복량
    [SerializeField] private float lightAttackStaminaCost = 2f; // Z 공격 스테미나 소모량
    [SerializeField] private float heavyAttackStaminaCost = 5f; // X 공격 스테미나 소모량
    
    public float LightAttackStaminaCost => lightAttackStaminaCost;
    public float HeavyAttackStaminaCost => heavyAttackStaminaCost;
    
    [Header("Dodge Gauge Settings")]
    [SerializeField] private int maxDodgeCount = 2; // 최대 회피 횟수
    [SerializeField] private float currentDodgeGauge = 2f; // 현재 회피 게이지 (소수점 가능)
    [SerializeField] private float dodgeRegenRate = 0.5f; // 초당 회복량
    
    [Header("UI References")]
    [SerializeField] private UnityEngine.UI.Slider healthBar; // 체력바 (선택사항)
    [SerializeField] private UnityEngine.UI.Slider staminaBar; // 스테미나바 (선택사항)
    [SerializeField] private UnityEngine.UI.Slider dodgeGaugeBar; // 회피 게이지바 (선택사항)
    
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0f;
    
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    
    public float CurrentDodgeGauge => currentDodgeGauge;
    public int MaxDodgeCount => maxDodgeCount;
    public int AvailableDodgeCount => Mathf.FloorToInt(currentDodgeGauge); // 사용 가능한 회피 횟수
    
    private PlayerStateMachine _stateMachine;
    
    private void Awake()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentDodgeGauge = maxDodgeCount;
        _stateMachine = GetComponent<PlayerStateMachine>();
        
        UpdateAllUI();
    }
    
    private void Update()
    {
        // 스테미나 자동 회복 (초당 0.5f)
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
        }
        
        // 회피 게이지 자동 회복 (초당 0.1f)
        if (currentDodgeGauge < maxDodgeCount)
        {
            currentDodgeGauge += dodgeRegenRate * Time.deltaTime;
            currentDodgeGauge = Mathf.Min(currentDodgeGauge, maxDodgeCount);
        }
        
        UpdateAllUI();
    }
    
    public void TakeDamage(float damage)
    {
        if (IsDead) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log($"[PlayerHealth] 데미지 받음: {damage}, 남은 체력: {currentHealth:F1}/{maxHealth}");
        
        UpdateAllUI();
        
        // 체력이 0이 되면 사망
        if (IsDead)
        {
            Die();
        }
        else
        {
            // 피격 상태로 전환 (경직)
            TriggerHitState();
        }
    }
    
    private void TriggerHitState()
    {
        if (_stateMachine != null)
        {
            // 현재 상태가 회피나 사망이 아니면 피격 상태로 전환
            var currentState = _stateMachine.CurrentStateType;
            if (currentState != PlayerState.Dodge && 
                currentState != PlayerState.Dead &&
                currentState != PlayerState.Hit) // 이미 피격 중이면 무시
            {
                Debug.Log("[PlayerHealth] Hit 상태로 전환");
                _stateMachine.TransitionTo(PlayerState.Hit);
            }
        }
    }
    
    // 스테미나 소모
    public bool UseStamina(float amount)
    {
        if (currentStamina < amount)
        {
            Debug.LogWarning($"[PlayerHealth] 스테미나 부족: 필요={amount}, 현재={currentStamina:F1}");
            return false; // 스테미나 부족
        }
        
        currentStamina -= amount;
        currentStamina = Mathf.Max(0, currentStamina);
        
        Debug.Log($"[PlayerHealth] 스테미나 소모: {amount}, 남은 스테미나: {currentStamina:F1}/{maxStamina}");
        
        UpdateAllUI();
        return true;
    }
    
    // 회피 게이지 소모
    public bool UseDodgeGauge()
    {
        if (currentDodgeGauge < 1f)
        {
            Debug.LogWarning($"[PlayerHealth] 회피 게이지 부족: {currentDodgeGauge:F2}/{maxDodgeCount}");
            return false; // 회피 불가
        }
        
        currentDodgeGauge -= 1f;
        currentDodgeGauge = Mathf.Max(0, currentDodgeGauge);
        
        Debug.Log($"[PlayerHealth] 회피 사용! 남은 횟수: {AvailableDodgeCount}/{maxDodgeCount} (게이지: {currentDodgeGauge:F2})");
        
        UpdateAllUI();
        return true;
    }
    
    public void Heal(float amount)
    {
        if (IsDead) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        
        Debug.Log($"[PlayerHealth] 회복: {amount}, 현재 체력: {currentHealth:F1}/{maxHealth}");
        
        UpdateAllUI();
    }
    
    // 스테미나 회복
    public void RecoverStamina(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Min(maxStamina, currentStamina);
        
        UpdateAllUI();
    }
    
    private void Die()
    {
        Debug.Log("[PlayerHealth] 플레이어 사망!");
        
        // Dead 상태로 전환
        if (_stateMachine != null)
        {
            _stateMachine.TransitionTo(PlayerState.Dead);
        }
    }
    
    private void UpdateAllUI()
    {
        // 체력바
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }
        
        // 스테미나바
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina / maxStamina;
        }
        
        // 회피 게이지바
        if (dodgeGaugeBar != null)
        {
            dodgeGaugeBar.value = currentDodgeGauge / maxDodgeCount;
        }
    }
    
    // 디버그용: 화면에 스탯 표시
    private void OnGUI()
    {
        if (Event.current.type == EventType.Repaint)
        {
            GUI.Label(new Rect(10, 10, 300, 30), $"HP: {currentHealth:F0}/{maxHealth:F0}");
            GUI.Label(new Rect(10, 35, 300, 30), $"SP: {currentStamina:F1}/{maxStamina:F0}");
            GUI.Label(new Rect(10, 60, 300, 30), $"Dodge: {AvailableDodgeCount}/{maxDodgeCount} ({currentDodgeGauge:F2})");
        }
    }
}

