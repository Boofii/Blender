using System;

namespace Blender.Content;

public class WeaponInfo(Type weaponType, string bundlePath) : EquipInfo
{
    public Type WeaponType { get; private set; } = weaponType;
    public string BundlePath { get; private set; } = bundlePath;
    public Type BasicType { get; private set; } = typeof(BasicProjectile);
    public Type ExType { get; private set; } = typeof(BasicProjectile);
    public Type BasicEffectType { get; private set; } = typeof(WeaponSparkEffect);
    public Type ExEffectType { get; private set; } = typeof(WeaponSparkEffect);

    public WeaponInfo SetBasicType(Type basicType)
    {
        this.BasicType = basicType;
        return this;
    }

    public WeaponInfo SetExType(Type exEffectType)
    {
        this.ExEffectType = exEffectType;
        return this;
    }

    public WeaponInfo SetBasicEffectType(Type basicEffectType)
    {
        this.BasicEffectType = basicEffectType;
        return this;
    }

    public WeaponInfo SetExEffectType(Type exType)
    {
        this.ExEffectType = exType;
        return this;
    }
}