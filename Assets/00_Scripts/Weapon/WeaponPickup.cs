using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private WeaponData weaponData;
    private IWeapon _weaponInstance;

    private void Awake()
    {
        if (weaponData == null)
        {
            Debug.LogWarning("WeaponPickup: WeaponData is null. Defaulting to Unarmed.");
            _weaponInstance = new Unarmed(null);
            return;
        }

        _weaponInstance = WeaponFactory.Create(weaponData);
    }

    public IWeapon GetWeapon()
    {
        return _weaponInstance;
    }
}