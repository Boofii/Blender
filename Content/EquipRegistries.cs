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
            Setup(name, modName, info, CharmPages,
                MapEquipUICardBackSelect.CHARMS, Charm.None, 9);
        });

    public static readonly LinkedRegistry<Weapon, WeaponInfo> Weapons = new(
        (name, modName, info) =>
        {
            Setup(name, modName, info, WeaponPages,
                MapEquipUICardBackSelect.WEAPONS, Weapon.None, 9);
        });

    internal static readonly Dictionary<int, Charm[]> CharmPages = [];
    internal static readonly Dictionary<int, Weapon[]> WeaponPages = [];

    private static void Setup<TEnum>(string name, string modName, EquipInfo info, Dictionary<int, TEnum[]> pagesDict, TEnum[] firstPageValues, TEnum noneValue, int pageAmount) where TEnum : Enum
    {
        if (!pagesDict.ContainsKey(1))
            pagesDict[1] = firstPageValues;

        foreach (string icon in info.NormalIcons)
            AssetHelper.CacheAsset<Sprite>(modName, info.BundleName, icon);
        foreach (string icon in info.GreyIcons)
            AssetHelper.CacheAsset<Sprite>(modName, info.BundleName, icon);

        if (info is WeaponInfo weaponInfo)
            SetupWeapon(name, modName, weaponInfo);

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

    private static void SetupWeapon(string name, string modName, WeaponInfo info)
    {
        GameObject weapon = new GameObject(name);
        AssetHelper.AddPrefab(weapon);

        GameObject basic = AssetHelper.CacheAsset<GameObject>(modName, info.BundleName, info.BasicName);
        GameObject ex = AssetHelper.CacheAsset<GameObject>(modName, info.BundleName, info.ExName);

        if (!info.WeaponType.IsSubclassOf(typeof(AbstractLevelWeapon)))
        {
            BlenderAPI.LogError($"Couldn't register weapon {name} because its weapon type " +
                "is not of type AbstractLevelWeapon.");
            return;
        }
        if (!info.BasicType.IsSubclassOf(typeof(AbstractProjectile)))
        {
            BlenderAPI.LogError($"Couldn't register weapon {name} because its basic type " +
                "is not of type AbstractProjectile.");
            return;
        }
        if (!info.ExType.IsSubclassOf(typeof(AbstractProjectile)))
        {
            BlenderAPI.LogError($"Couldn't register weapon {name} because its ex type " +
                "is not of type AbstractProjectile.");
            return;
        }

        AbstractLevelWeapon weaponComponent = (AbstractLevelWeapon)weapon.AddComponent(info.WeaponType);
        AbstractProjectile basicComponent = (AbstractProjectile)basic.AddComponent(info.BasicType);
        AbstractProjectile exComponent = (AbstractProjectile)ex.AddComponent(info.ExType);

        SetupProjectile(basicComponent);
        SetupProjectile(exComponent);
        weaponComponent.basicPrefab = basicComponent;
        weaponComponent.exPrefab = exComponent;
    }

    private static void SetupProjectile(AbstractProjectile projectile)
    {
        SpriteRenderer renderer = projectile.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = "Projectiles";
            renderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        Animator animator = projectile.GetComponent<Animator>();
        if (animator != null)
        {
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == "Death")
                {
                    AnimationEvent evt = new()
                    {
                        time = clip.length,
                        functionName = nameof(AbstractProjectile.OnDieAnimationComplete)
                    };
                    clip.AddEvent(evt);
                }
            }
        }
        else
            projectile.gameObject.AddComponent<Animator>();

        projectile.gameObject.tag = "PlayerProjectile";
        projectile.gameObject.layer = LayerMask.NameToLayer("Projectile");
    }
}