using System;

namespace Blender.Content;

public class WeaponInfo(Type weaponType) : EquipInfo
{
    public Type WeaponType { get; private set; } = weaponType;
    public Type BasicType { get; private set; } = typeof(BasicProjectile);
    public Type ExType { get; private set; } = typeof(BasicProjectile);
    public string BasicName { get; private set; } = string.Empty;
    public string ExName { get; private set; } = string.Empty;
    public string BasicEffectName { get; private set; } = string.Empty;
    public string ExEffectName { get; private set; } = string.Empty;

    public WeaponInfo SetBasicType(Type basicType)
    {
        this.BasicType = basicType;
        return this;
    }

    public WeaponInfo SetExType(Type exType)
    {
        this.ExType = exType;
        return this;
    }

    public WeaponInfo SetBasicName(string basicName)
    {
        this.BasicName = basicName;
        return this;
    }

    public WeaponInfo SetExName(string exName)
    {
        this.ExName = exName;
        return this;
    }

    public WeaponInfo SetBasicEffectName(string effectName)
    {
        this.BasicEffectName = effectName;
        return this;
    }

    public WeaponInfo SetExEffectName(string effectName)
    {
        this.ExEffectName = effectName;
        return this;
    }
}