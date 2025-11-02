using UnityEngine;

public static class WeaponFactory
{
    public static IWeapon Create(WeaponData data)
    {
        if (data == null)
        {
            return new Unarmed(null);
        }

        switch (data.weaponType)
        {
            case WeaponType.Melee:
                if (data.weaponName == "LongSword") return new LongSword(data);
                return new Unarmed(data);
            case WeaponType.Ranged:
                if (data.weaponName == "Pistol") return CreatePistol(data);
                return new Unarmed(data);
            case WeaponType.Unarmed:
            default:
                return new Unarmed(data);
        }
    }
    
    private static IWeapon CreatePistol(WeaponData data)
    {
        // Pistol은 이제 일반 클래스로 변경되어 생성자로 생성
        return new Pistol(data);
    }
}


