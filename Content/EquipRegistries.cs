using Blender.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Blender.Content;

public static class EquipRegistries
{

    public static readonly LinkedRegistry<Charm, EquipInfo> Charms = new(
        (name, modName, info) =>
        {
            HandlePages(name, modName, info, CharmPages,
                MapEquipUICardBackSelect.CHARMS, Charm.None, 9);
        });

    public static readonly LinkedRegistry<Weapon, WeaponInfo> Weapons = new(
        (name, modName, info) =>
        {
            HandlePages(name, modName, info, WeaponPages,
                MapEquipUICardBackSelect.WEAPONS, Weapon.None, 9);
        });

    internal static readonly Dictionary<int, Charm[]> CharmPages = [];
    internal static readonly Dictionary<int, Weapon[]> WeaponPages = [];

    private static void HandlePages<TEnum>(string name, string modName, EquipInfo info, Dictionary<int, TEnum[]> pagesDict, TEnum[] firstPageValues, TEnum noneValue, int pageAmount) where TEnum : Enum
    {
        if (!pagesDict.ContainsKey(1))
            pagesDict[1] = firstPageValues;

        foreach (string icon in info.NormalIcons)
            ObjectHelper.CacheAsset<Sprite>(modName, info.BundleName, icon);
        foreach (string icon in info.GreyIcons)
            ObjectHelper.CacheAsset<Sprite>(modName, info.BundleName, icon);

        if (info is WeaponInfo weaponInfo)
            HandleWeapons(name, modName, weaponInfo);

        int count = 1;
        foreach (var pageValues in pagesDict.Values)
            count += pageValues.Where(value => !value.Equals(noneValue)).Count();
        int page = (int)Math.Ceiling((double)count / pageAmount);

        if (!pagesDict.ContainsKey(page))
        {
            TEnum[] values = new TEnum[pageAmount];
            for (int i = 0; i < values.Length; i++)
                values[i] = noneValue;

            pagesDict[page] = values;
        }

        TEnum[] array = pagesDict[page];
        int index = array.Where(value => !value.Equals(noneValue)).Count();
        array[index] = (TEnum)Enum.Parse(typeof(TEnum), name);
    }

    private static void HandleWeapons(string name, string modName, WeaponInfo info)
    {
        GameObject weapon = new GameObject(name);
        ObjectHelper.AddPrefab(weapon);
        GameObject projectile = ObjectHelper.CacheAsset<GameObject>(modName, info.BundleName, info.ProjectileName);

        if (!info.WeaponType.IsSubclassOf(typeof(AbstractLevelWeapon)))
        {
            BlenderAPI.LogError($"Couldn't register weapon {name} because its weapon type " +
                "is not of type AbstractLevelWeapon.");
            return;
        }
        if (!info.ProjectileType.IsSubclassOf(typeof(AbstractProjectile)))
        {
            BlenderAPI.LogError($"Couldn't register weapon {name} because its projectile type " +
                "is not of type AbstractProjectile.");
            return;
        }

        AbstractLevelWeapon weaponComponent = (AbstractLevelWeapon)weapon.AddComponent(info.WeaponType);
        AbstractProjectile projectileComponent = (AbstractProjectile)projectile.AddComponent(info.ProjectileType);
        
        SpriteRenderer renderer = projectile.GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.sortingLayerName = "Projectiles";
        projectile.tag = "PlayerProjectile";
        projectile.layer = LayerMask.NameToLayer("Projectile");

        weaponComponent.basicPrefab = projectileComponent;
    }
}