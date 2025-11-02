using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
public string weaponName;

public WeaponType weaponType;

public GameObject weaponPickupPrefab; // 필드에 드랍될 때 생성될 프리팹

// Pistol 전용 설정
public GameObject bulletPrefab;
public float bulletSpeed = 20f;
public float bulletDamage = 15f;
public int burstCount = 3;
public float burstDelay = 0.1f;

}