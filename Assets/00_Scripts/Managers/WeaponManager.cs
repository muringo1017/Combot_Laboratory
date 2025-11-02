using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerCombat))]
public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Transform weaponHand;
    [Header("Weapon Position Settings")]
    [SerializeField] private Vector3 weaponPositionOffset = Vector3.zero;
    [SerializeField] private Vector3 weaponRotationOffset = Vector3.zero;
    [SerializeField] private bool useCustomPosition = false;
    
    [Header("Weapon Type Specific Settings")]
    [SerializeField] private Vector3 meleeWeaponRotation = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 rangedWeaponRotation = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 unarmedRotation = new Vector3(0, 0, 0);
    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo = true; 
    
    public IWeapon CurrentWeapon { get; private set; }
    public bool HasWeapon => CurrentWeapon != null;
    public bool IsSwitchingWeapon { get; private set; }
    public event System.Action<IWeapon> OnWeaponChanged;
    
    private GameObject _equippedWeaponObject;
    public GameObject EquippedWeaponObject => _equippedWeaponObject; // 외부에서 접근 가능하도록
    private PlayerCombat _playerCombat;

    private void Awake()
    {
        _playerCombat = GetComponent<PlayerCombat>();
        if (weaponHand == null)
        {
            Debug.LogWarning("WeaponManager: weaponHand is not assigned. Trying to find RightHand...");
            // 자동으로 RightHand 찾기 시도
            weaponHand = FindChildByName(transform, "RightHand");
            if (weaponHand == null)
            {
                weaponHand = FindChildByName(transform, "rightHand");
            }
            if (weaponHand == null)
            {
                weaponHand = FindChildByName(transform, "Hand_R");
            }
            if (weaponHand == null)
            {
                Debug.LogError("WeaponManager: Could not find RightHand transform. Please assign weaponHand manually.");
            }
            else
            {
                Debug.Log($"WeaponManager: Found weaponHand: {weaponHand.name}");
            }
        }
    }
    
    private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
            
            Transform found = FindChildByName(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
    
    private Vector3 GetWeaponTypeRotation()
    {
        if (CurrentWeapon == null)
        {
            return Vector3.zero;
        }
            
        // WeaponData에서 무기 타입 확인
        var weaponData = CurrentWeapon.Data;
        if (weaponData == null)
        {
            return Vector3.zero;
        }
        
        Vector3 rotationToReturn = Vector3.zero;
        
        switch (weaponData.weaponType)
        {
            case WeaponType.Melee:
                rotationToReturn = meleeWeaponRotation;
                break;
            case WeaponType.Ranged:
                rotationToReturn = rangedWeaponRotation;
                break;
            case WeaponType.Unarmed:
                rotationToReturn = unarmedRotation;
                break;
            default:
                rotationToReturn = Vector3.zero;
                break;
        }
        
        
        return rotationToReturn;
    }

    // [수정] 이제 WeaponPickup 컴포넌트 자체를 받습니다.
    public void EquipWeapon(WeaponPickup weaponPickup)
    {
        if (IsSwitchingWeapon) return;
        StartCoroutine(SwitchWeaponCoroutine(weaponPickup));
    }

    private IEnumerator SwitchWeaponCoroutine(WeaponPickup newWeaponPickup)
    {
        IsSwitchingWeapon = true;
        
        // 기존 무기가 있으면 먼저 내려놓습니다.
        if (CurrentWeapon != null)
        {
            DropCurrentWeapon();
            yield return new WaitForSeconds(0.5f); // 내려놓는 애니메이션 시간
        }

        // 새로운 무기(논리)를 가져옵니다.
        CurrentWeapon = newWeaponPickup.GetWeapon();
        
        // 새로운 무기(오브젝트)를 가져와 손으로 옮깁니다.
        _equippedWeaponObject = newWeaponPickup.gameObject;
        
        // 1. 부모를 weaponHand로 설정하고 위치를 초기화합니다.
        if (weaponHand == null)
        {
            IsSwitchingWeapon = false;
            yield break;
        }
        
        _equippedWeaponObject.transform.SetParent(weaponHand);
        
        // 커스텀 위치 사용 여부에 따라 위치 설정
        if (useCustomPosition)
        {
            _equippedWeaponObject.transform.localPosition = weaponPositionOffset;
            _equippedWeaponObject.transform.localRotation = Quaternion.Euler(weaponRotationOffset);
        }
        else
        {
            _equippedWeaponObject.transform.localPosition = Vector3.zero;
            // 무기 타입에 따른 회전 적용 (CurrentWeapon이 설정된 후)
            Vector3 rotationToApply = GetWeaponTypeRotation();
            _equippedWeaponObject.transform.localRotation = Quaternion.Euler(rotationToApply);
            
        }
        

        // 2. 무기 회전 애니메이션 중지
        var weaponSpin = _equippedWeaponObject.GetComponent<WeaponSpin>();
        if (weaponSpin != null)
        {
            weaponSpin.StopSpinning();
        }
        
        // 3. WeaponPickup 스크립트만 비활성화 (콜라이더는 유지)
        newWeaponPickup.enabled = false;

        CurrentWeapon.OnEquip(_playerCombat);
        yield return new WaitForSeconds(0.5f); // 줍는 애니메이션 시간

        IsSwitchingWeapon = false;
        OnWeaponChanged?.Invoke(CurrentWeapon);
    }

    public void DropCurrentWeapon()
    {
        if (CurrentWeapon == null || IsSwitchingWeapon) return;

        // 1. 부모 연결을 해제하여 월드에 독립적으로 만듭니다.
        _equippedWeaponObject.transform.SetParent(null);
        
        // 2. 무기 위치와 회전 초기화
        Vector3 dropPosition = _playerCombat.transform.position;
        dropPosition.y = _equippedWeaponObject.transform.position.y; // Y는 기존 높이 유지
        _equippedWeaponObject.transform.position = dropPosition;
        
        // 3. 무기 회전을 기본값으로 초기화 (무기 타입에 관계없이)
        _equippedWeaponObject.transform.rotation = Quaternion.identity;
        
        
        // 4. 무기 회전 애니메이션 시작
        var weaponSpin = _equippedWeaponObject.GetComponent<WeaponSpin>();
        if (weaponSpin == null)
        {
            weaponSpin = _equippedWeaponObject.AddComponent<WeaponSpin>();
        }
        weaponSpin.StartSpinning();
        
        // 5. 다시 주울 수 있도록 모든 BoxCollider를 활성화합니다.
        var boxColliders = _equippedWeaponObject.GetComponents<BoxCollider>();
        foreach (var boxCollider in boxColliders)
        {
            boxCollider.enabled = true;
        }
        
    
        
        // WeaponPickup 컴포넌트 활성화
        var weaponPickup = _equippedWeaponObject.GetComponent<WeaponPickup>();
        if (weaponPickup != null)
        {
            weaponPickup.enabled = true;
        }

        CurrentWeapon.OnUnequip(_playerCombat);
        
        // 참조를 초기화합니다.
        CurrentWeapon = null;
        _equippedWeaponObject = null;
        OnWeaponChanged?.Invoke(null);
    }
}