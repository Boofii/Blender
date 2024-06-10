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
            Setup(name, modName, info, CharmPages, Charm.None, 9);
        });

    public static readonly LinkedRegistry<Weapon, WeaponInfo> Weapons = new(
        (name, modName, info) =>
        {
            Setup(name, modName, info, WeaponPages, Weapon.None, 9);
        });

    internal static readonly Dictionary<int, Charm[]> CharmPages = [];
    internal static readonly Dictionary<int, Weapon[]> WeaponPages = [];

    private static void Setup<TEnum>(string name, string modName, EquipInfo info, Dictionary<int, TEnum[]> pagesDict, TEnum noneValue, int pageAmount) where TEnum : Enum
    {
        foreach (string icon in info.NormalIcons)
            AssetHelper.CacheAsset<Sprite>(modName, info.BundleName, icon);
        foreach (string icon in info.GreyIcons)
            AssetHelper.CacheAsset<Sprite>(modName, info.BundleName, icon);

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

        if (info is WeaponInfo weaponInfo)
            SetupWeapon(name, modName, weaponInfo);
    }

    private static void SetupWeapon(string name, string modName, WeaponInfo info)
    {
        GameObject weapon = new GameObject(name);
        AssetHelper.AddPrefab(weapon);

        if (!info.WeaponType.IsSubclassOf(typeof(AbstractLevelWeapon)))
        {
            BlenderAPI.LogError($"Couldn't register weapon {name} because its weapon type " +
                "is not of type AbstractLevelWeapon.");
            return;
        }
        AbstractLevelWeapon weaponComponent = (AbstractLevelWeapon)weapon.AddComponent(info.WeaponType);

        if (!info.BasicType.IsSubclassOf(typeof(AbstractProjectile)))
        {
            BlenderAPI.LogError($"Couldn't register weapon {name} because its basic type " +
                "is not of type AbstractProjectile.");
            return;
        }
        GameObject basic = AssetHelper.CacheAsset<GameObject>(modName, info.BundleName, info.BasicName);
        if (basic != null)
        {
            AbstractProjectile basicComponent = (AbstractProjectile)basic.AddComponent(info.BasicType);
            SetupProjectile(basicComponent);
            weaponComponent.basicPrefab = basicComponent;
        }

        if (!info.ExType.IsSubclassOf(typeof(AbstractProjectile)))
        {
            BlenderAPI.LogError($"Couldn't register weapon {name} because its ex type " +
                "is not of type AbstractProjectile.");
            return;
        }
        GameObject ex = AssetHelper.CacheAsset<GameObject>(modName, info.BundleName, info.ExName);
        if (ex != null)
        {
            AbstractProjectile exComponent = (AbstractProjectile)ex.AddComponent(info.ExType);
            SetupProjectile(exComponent);
            weaponComponent.exPrefab = exComponent;
        }

        GameObject basicEffect = AssetHelper.CacheAsset<GameObject>(modName, info.BundleName, info.BasicEffectName);
        if (basicEffect != null)
        {
            WeaponSparkEffect effect = SetupEffect(basicEffect);
            weaponComponent.basicEffectPrefab = effect;
        }
        GameObject exEffect = AssetHelper.CacheAsset<GameObject>(modName, info.BundleName, info.ExEffectName);
        if (exEffect != null)
        {
            WeaponSparkEffect effect = SetupEffect(exEffect);
            weaponComponent.exEffectPrefab = effect;
        }
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

    private static WeaponSparkEffect SetupEffect(GameObject effect)
    {
        WeaponSparkEffect effectComponent = effect.AddComponent<WeaponSparkEffect>();
        SpriteRenderer renderer = effect.GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.material = new Material(Shader.Find("Sprites/Default"));

        Animator animator = effect.GetComponent<Animator>();
        if (animator != null)
        {
            AnimationClip clip = animator.runtimeAnimatorController.animationClips[0];
            if (clip != null)
            {
                AnimationEvent evt = new()
                {
                    time = clip.length,
                    functionName = nameof(Effect.OnEffectComplete)
                };
                clip.AddEvent(evt);
            }
        }

        return effectComponent;
    }

    internal static void Initialize()
    {
        CharmPages[1] = MapEquipUICardBackSelect.CHARMS;
        WeaponPages[1] = MapEquipUICardBackSelect.WEAPONS;
    }
}