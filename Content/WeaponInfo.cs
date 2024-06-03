using System;

namespace Blender.Content;

public class WeaponInfo(Type weaponType, Type projectileType, string projectileName) : EquipInfo
{
    public Type WeaponType { get; private set; } = weaponType;
    public Type ProjectileType { get; private set; } = projectileType;
    public string ProjectileName { get; private set; } = projectileName;
}