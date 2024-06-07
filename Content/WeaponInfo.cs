using System;

namespace Blender.Content;

public class WeaponInfo(Type weaponType, Type basicType, Type exType, string basicName, string exName) : EquipInfo
{
    public Type WeaponType { get; private set; } = weaponType;
    public Type BasicType { get; private set; } = basicType;
    public Type ExType { get; private set; } = exType;
    public string BasicName { get; private set; } = basicName;
    public string ExName { get; private set; } = exName;
}