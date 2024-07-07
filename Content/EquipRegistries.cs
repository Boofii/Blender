using Blender.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Blender.Content;

public static class EquipRegistries
{
    public static readonly LinkedRegistry<Charm, EquipInfo> Charms = new(
        (name, info) =>
        {
            Setup(name, info, CharmPages, Charm.None, BlenderAPI.HasDLC ? 9 : 6);
        });

    public static readonly LinkedRegistry<Weapon, WeaponInfo> Weapons = new(
        (name, info) =>
        {
            Setup(name, info, WeaponPages, Weapon.None, BlenderAPI.HasDLC ? 9 : 6);
        });

    internal static readonly Dictionary<int, Charm[]> CharmPages = [];
    internal static readonly Dictionary<int, Weapon[]> WeaponPages = [];

    private static void Setup<TEnum>(string name, EquipInfo info, Dictionary<int, TEnum[]> pagesDict, TEnum noneValue, int pageAmount) where TEnum : Enum
    {
        foreach (string icon in info.NormalIcons)
            AssetHelper.CacheAsset<Sprite>(info.BundleId, icon, null);
        foreach (string icon in info.GreyIcons)
            AssetHelper.CacheAsset<Sprite>(info.BundleId, icon, null);

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
            SetupWeapon(name, weaponInfo);

        ShopInfo shopInfo = info.ShopInfo;
        if (shopInfo != null)
            SetupItem(info, shopInfo);
    }

    private static void SetupItem(EquipInfo info, ShopInfo shopInfo)
    {
        AssetHelper.CacheAsset<GameObject>(info.BundleId, shopInfo.ItemName, (item) =>
        {
            ShopSceneItem itemComponent = item.AddComponent<ShopSceneItem>();
            itemComponent.itemType = shopInfo.ItemType;
            itemComponent.charm = (Charm)Enum.Parse(typeof(Charm), shopInfo.Charm);
            itemComponent.weapon = (Weapon)Enum.Parse(typeof(Weapon), shopInfo.Weapon);
            itemComponent.super = (Super)Enum.Parse(typeof(Super), shopInfo.Super);

            for (int i = 0; i < 4; i++)
            {
                SpriteRenderer renderer = item.transform.GetChild(i).GetComponent<SpriteRenderer>();
                renderer.material = new Material(Shader.Find("Sprites/Default"));
                renderer.sortingLayerName = "Foreground";

                switch (i)
                {
                    case 0:
                        itemComponent.spriteInactive = renderer;
                        renderer.sortingOrder = 399;
                        renderer.enabled = false;
                        break;
                    case 1:
                        itemComponent.spriteSelected = renderer;
                        renderer.sortingOrder = 400;
                        renderer.enabled = true;
                        break;
                    case 2:
                        itemComponent.spritePurchased = renderer;
                        renderer.sortingOrder = 400;
                        renderer.enabled = false;
                        break;
                    case 3:
                        itemComponent.spriteShadowObject = renderer;
                        itemComponent.spriteShadow = renderer.sprite;
                        itemComponent.originalShadowScale = shopInfo.OriginalShadowScale;
                        renderer.sortingOrder = 200;
                        renderer.enabled = true;
                        break;
                }
            }
        });
    }

    private static void SetupWeapon(string name, WeaponInfo info)
    {
        GameObject weapon = new GameObject(name);
        AssetHelper.AddPrefab(weapon, false);

        if (!info.WeaponType.IsSubclassOf(typeof(AbstractLevelWeapon)))
        {
            BlenderAPI.LogError($"Couldn't register weapon \"{name}\" because its weapon type is not of type \"AbstractLevelWeapon\".");
            return;
        }

        AbstractLevelWeapon weaponComponent = (AbstractLevelWeapon)weapon.AddComponent(info.WeaponType);

        if (!info.BasicType.IsSubclassOf(typeof(AbstractProjectile)))
        {
            BlenderAPI.LogError($"Couldn't register weapon \"{name}\" because its basic type is not of type \"AbstractProjectile\".");
            return;
        }
        AssetHelper.LoadAsset<GameObject>(info.BundleId, info.BasicName, (basic) =>
        {
            AbstractProjectile basicComponent = (AbstractProjectile)basic.AddComponent(info.BasicType);
            SetupProjectile(basicComponent);
            weaponComponent.basicPrefab = basicComponent;
        });

        if (!info.ExType.IsSubclassOf(typeof(AbstractProjectile)))
        {
            BlenderAPI.LogError($"Couldn't register weapon \"{name}\" because its ex type is not of type \"AbstractProjectile\".");
            return;
        }
        AssetHelper.LoadAsset<GameObject>(info.BundleId, info.ExName, (ex) =>
        {
            AbstractProjectile exComponent = (AbstractProjectile)ex.AddComponent(info.ExType);
            SetupProjectile(exComponent);
            weaponComponent.exPrefab = exComponent;
        });

        AssetHelper.LoadAsset<GameObject>(info.BundleId, info.BasicEffectName, (basicEffect) =>
        {
            WeaponSparkEffect effect = SetupEffect(basicEffect, info.BasicEffectType);
            weaponComponent.basicEffectPrefab = effect;
        });

        AssetHelper.LoadAsset<GameObject>(info.BundleId, info.ExEffectName, (exEffect) =>
        {
            WeaponSparkEffect effect = SetupEffect(exEffect, info.ExEffectType);
            weaponComponent.exEffectPrefab = effect;
        });
    }

    private static void SetupProjectile(AbstractProjectile projectile)
    {
        SpriteRenderer renderer = projectile.GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.sortingLayerName = "Projectiles";

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

    private static WeaponSparkEffect SetupEffect(GameObject effect, Type effectType)
    {
        WeaponSparkEffect effectComponent = (WeaponSparkEffect)effect.AddComponent(effectType);
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
        if (BlenderAPI.HasDLC)
        {
            CharmPages[1] = MapEquipUICardBackSelect.CHARMS;
            WeaponPages[1] = MapEquipUICardBackSelect.WEAPONS;
        }
        else
        {
            List<Charm> charms = MapEquipUICardBackSelect.CHARMS.ToList();
            charms.RemoveRange(6, 3);
            CharmPages[1] = charms.ToArray();

            List<Weapon> weapons = MapEquipUICardBackSelect.WEAPONS.ToList();
            weapons.RemoveRange(6, 3);
            WeaponPages[1] = weapons.ToArray();
        }
    }
}