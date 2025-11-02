using UnityEngine;


public enum ComboState
{
    NONE,
    LightAttack_1, 
    LightAttack_2, 
    LightAttack_3, 
    HeavyAttack_1, 
    HeavyAttack_2, 
    HeavyAttack_3  
}

[RequireComponent(typeof(WeaponManager))]
[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerCombat : MonoBehaviour
{
    // --- 컴포넌트 참조 ---
    private WeaponManager _weaponManager;
    private CharacterAnimation _characterAnimation;
    private PlayerStateMachine _stateMachine;
    private PlayerController _playerController;

    public PlayerStateMachine PlayerStateMachine => _stateMachine;

    // --- 외부 데이터 참조 ---
    [SerializeField] private WeaponData unarmedWeaponData; // 맨손용 WeaponData를 인스펙터에서 할당

    // --- 전략 패턴 ---
    private IWeapon _unarmed; // 맨손(Unarmed) 상태일 때의 공격 전략

    // --- 무기 상호작용 ---
    public float pickupRange = 1.5f;
    [SerializeField] private LayerMask pickupLayer;
    
    [SerializeField] private float attackMoveForce = 1.0f;
    public float AttackMoveForce => attackMoveForce;

    private void Awake()
    {
        // 컴포넌트 초기화
        _characterAnimation = GetComponentInChildren<CharacterAnimation>();
        _stateMachine = GetComponent<PlayerStateMachine>();
        _weaponManager = GetComponent<WeaponManager>();
        _playerController = GetComponent<PlayerController>();

        _unarmed = new Unarmed(unarmedWeaponData);
    }

    private void OnEnable()
    {
        if (_weaponManager != null)
            _weaponManager.OnWeaponChanged += HandleWeaponChanged;
    }

    private void OnDisable()
    {
        if (_weaponManager != null)
            _weaponManager.OnWeaponChanged -= HandleWeaponChanged;
    }

    private void Update()
    {
        if (_weaponManager.HasWeapon)
            _weaponManager.CurrentWeapon.HandleInput(this, AttackType.None, false);
        else
            _unarmed.HandleInput(this, AttackType.None, false);
    }

    public void RequestAttack(AttackType attackType, bool isInputReleased)
    {
        if (_weaponManager.HasWeapon)
        {
            _weaponManager.CurrentWeapon.HandleInput(this, attackType, isInputReleased);
        }
        else
        {
            _unarmed.HandleInput(this, attackType, isInputReleased);
        }
    }

    public void PerformCurrentAttack()
    {
        // Range weapon(Pistol)이 아닌 경우에만 moveforce 적용
        bool isRangeWeapon = _weaponManager.HasWeapon && _weaponManager.CurrentWeapon is Pistol;
        
        if (!isRangeWeapon)
        {
            float baseDir = transform.localScale.x > 0 ? 1f : -1f;
            float impulse = baseDir * attackMoveForce;
            
            // 디버그 로그 추가
            Debug.Log($"공격 이동: baseDir={baseDir}, attackMoveForce={attackMoveForce}, impulse={impulse}");
            
            if (_playerController != null)
            {
                var rb = _playerController.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 velocity = rb.linearVelocity;
                    velocity.x = impulse;
                    rb.linearVelocity = velocity;
                    Debug.Log($"Rigidbody velocity 설정: {rb.linearVelocity}");
                }
                else
                {
                    Debug.LogWarning("Rigidbody를 찾을 수 없습니다!");
                }
            }
            else
            {
                Debug.LogWarning("PlayerController를 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.Log("Range weapon 사용 - moveforce 적용 안함");
        }
        
        
        if (_weaponManager.HasWeapon)
            _weaponManager.CurrentWeapon.PerformAttack(_characterAnimation);
        else
            _unarmed.PerformAttack(_characterAnimation);
    }

    public void HandleWeaponInteraction()
    {
        if (_weaponManager.HasWeapon)
            _weaponManager.DropCurrentWeapon();
        else
            TryToPickupWeapon();
    }

    private void TryToPickupWeapon()
    {
        int layerMask = pickupLayer.value == 0 ? ~0 : pickupLayer.value; // 인스펙터 미설정 시 전체 레이어 검색
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            pickupRange,
            layerMask,
            QueryTriggerInteraction.Collide
        );

        WeaponPickup closest = null;
        float bestSqr = float.MaxValue;

        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent<WeaponPickup>(out var weaponPickup))
            {
                float sqr = (weaponPickup.transform.position - transform.position).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    closest = weaponPickup;
                }
            }
        }

        if (closest != null)
            _weaponManager.EquipWeapon(closest);
    }

    private void HandleWeaponChanged(IWeapon newWeapon)
    {
        // TODO: UI 갱신/콤보 리셋 등 필요 시 구현
    }
    public IWeapon GetCurrentWeaponStrategy()
    {
        if (_weaponManager.HasWeapon)
        {
            return _weaponManager.CurrentWeapon;
        }
        else
        {
            return _unarmed;
        }
    }
    
    public float GetCurrentAttackAnimationLength()
    {
        if (_weaponManager.HasWeapon)
            return _weaponManager.CurrentWeapon.GetCurrentAttackAnimationLength();
        else
            return _unarmed.GetCurrentAttackAnimationLength();
    }
    
    public float GetCurrentAttackDamage()
    {
        if (_weaponManager.HasWeapon)
            return _weaponManager.CurrentWeapon.GetCurrentAttackDamage();
        else
            return _unarmed.GetCurrentAttackDamage();
    }
    
    public float GetCurrentAttackStaminaCost()
    {
        if (_weaponManager.HasWeapon)
            return _weaponManager.CurrentWeapon.GetCurrentAttackStaminaCost();
        else
            return _unarmed.GetCurrentAttackStaminaCost();
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
    
}