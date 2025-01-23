using Blender.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Blender.Utility.AssetHelper;

namespace Blender.Content;

public static class EquipRegistries
{
    public static readonly LinkedRegistry<Charm, EquipInfo> Charms = new(
        (charm, info) =>
        {
            Setup(charm, info, CharmPages, Charm.None, BlenderAPI.HasDLC ? 9 : 6);
        });

    public static readonly LinkedRegistry<Weapon, WeaponInfo> Weapons = new(
        (weapon, info) =>
        {
            Setup(weapon, info, WeaponPages, Weapon.None, BlenderAPI.HasDLC ? 9 : 6);
        });

    public static readonly LinkedRegistry<Super, SuperInfo> Supers = new(
    (super, info) =>
    {
        Setup(super, info, SuperPages, Super.None, 3);
    });

    internal static readonly Dictionary<int, Charm[]> CharmPages = [];
    internal static readonly Dictionary<int, Weapon[]> WeaponPages = [];
    internal static readonly Dictionary<int, Super[]> SuperPages = [];
    internal static readonly List<string> ProcessedItemBundles = [];
    internal static readonly Dictionary<string, ItemType> ProcessedItems = [];
    internal static readonly List<string> ProcessedSuperBundles = [];
    internal static readonly List<string> ProcessedWeaponBundles = [];

    private static void Setup<TEnum>(TEnum instance, EquipInfo info, Dictionary<int, TEnum[]> pagesDict, TEnum noneValue, int pageAmount) where TEnum : Enum
    {
        if (info.AtlasPath != null)
            AssetHelper.AddPersistentPath(LoaderType.Single, info.AtlasPath);

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
        array[index] = instance;

        if (info is WeaponInfo weaponInfo && !ProcessedWeaponBundles.Contains(weaponInfo.BundlePath))
            SetupWeapons(weaponInfo.BundlePath);

        if (info is SuperInfo superInfo && !ProcessedSuperBundles.Contains(superInfo.BundlePath))
            SetupSupers(superInfo.BundlePath);

        if (info.ShopInfo != null && !ProcessedItemBundles.Contains(info.ShopInfo.BundlePath))
            SetupItems(info.ShopInfo.BundlePath);
    }

    private static void SetupItems(string bundlePath)
    {
        ProcessedItemBundles.Add(bundlePath);
        AssetHelper.AddScenePathMapping(LoaderType.Multiple, "scene_shop", [bundlePath]);
        AssetHelper.AddScenePathMapping(LoaderType.Multiple, "scene_shop_DLC", [bundlePath]);
        MultiLoader.LoadActions[bundlePath] += (name, items) =>
        {
            foreach (UnityEngine.Object item in items)
            {
                if (item is not GameObject itemObj)
                    continue;

                ShopInfo shopInfo = null;
                if (Charms.ContainsName(item.name))
                {
                    shopInfo = Charms.GetValue(item.name).ShopInfo;
                    ProcessedItems.Add(item.name, ItemType.Charm);
                }
                else if (Weapons.ContainsName(item.name))
                {
                    shopInfo = Weapons.GetValue(item.name).ShopInfo;
                    ProcessedItems.Add(item.name, ItemType.Weapon);
                }
                else if (Supers.ContainsName(item.name))
                {
                    shopInfo = Supers.GetValue(item.name).ShopInfo;
                    ProcessedItems.Add(item.name, ItemType.Super);
                }
                else
                {
                    BlenderAPI.LogWarning($"Tried to load a shop item named \"{item.name}\" that didn't have a matching charm/weapon/super.");
                    continue;
                }

                if (shopInfo == null)
                {
                    BlenderAPI.LogWarning($"Tried to load a shop item named \"{item.name}\" that didn't have a shop info value.");
                    continue;
                }

                ShopSceneItem itemComponent = itemObj.AddComponent<ShopSceneItem>();
                itemComponent.originalShadowScale = shopInfo.OriginalShadowScale;
                itemComponent.itemType = shopInfo.ItemType;
                itemComponent.charm = (Charm)Enum.Parse(typeof(Charm), shopInfo.Charm);
                itemComponent.weapon = (Weapon)Enum.Parse(typeof(Weapon), shopInfo.Weapon);
                itemComponent.super = (Super)Enum.Parse(typeof(Super), shopInfo.Super);

                for (int i = 0; i < 4; i++)
                {
                    SpriteRenderer renderer = itemObj.transform.GetChild(i).GetComponent<SpriteRenderer>();
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
                            renderer.sortingOrder = 200;
                            renderer.enabled = true;
                            break;
                    }
                }
            }
        };
    }

    private static void SetupSupers(string bundlePath)
    {
        ProcessedSuperBundles.Add(bundlePath);
        AssetHelper.AddPersistentPath(LoaderType.Multiple, bundlePath);
        MultiLoader.LoadActions[bundlePath] += (name, supers) =>
        {
            foreach (UnityEngine.Object super in supers)
            {
                if (super is not GameObject superObj)
                    continue;

                if (!Supers.ContainsName(super.name))
                {
                    BlenderAPI.LogWarning($"Tried to load a super named \"{super.name}\" that didn't have a matching super.");
                    continue;
                }

                SuperInfo info = Supers.GetValue(super.name);
                if (!info.SuperType.IsSubclassOf(typeof(AbstractPlayerSuper)))
                {
                    BlenderAPI.LogError($"Couldn't load a super named \"{super.name}\" because its super type is not of type \"AbstractPlayerSuper\".");
                    continue;
                }

                AbstractPlayerSuper superComponent = (AbstractPlayerSuper)superObj.AddComponent(info.SuperType);
                foreach (SpriteRenderer renderer in superObj.GetComponentsInChildren<SpriteRenderer>())
                    renderer.material = new Material(Shader.Find("Sprites/Default"));
                if (superObj.GetComponent<Animator>() == null)
                    superObj.AddComponent<Animator>();
                superObj.AddComponent<AnimationHelper>();
                superComponent.cuphead = superObj.transform.GetChild(0).GetComponent<SpriteRenderer>();
                superComponent.mugman = superObj.transform.GetChild(1).GetComponent<SpriteRenderer>();
                superComponent.isChaliceSuper = info.IsChaliceSuper;
                AssetHelper.AddPrefab(superObj, false);
            }
        };
    }

    private static void SetupWeapons(string bundlePath)
    {
        if (bundlePath == null)
            return;

        ProcessedWeaponBundles.Add(bundlePath);
        AssetHelper.AddPersistentPath(LoaderType.Multiple, bundlePath);
        MultiLoader.LoadActions[bundlePath] += (name, weapons) =>
        {
            foreach (UnityEngine.Object weapon in weapons)
            {
                if (weapon is not GameObject weaponObj)
                    continue;

                if (!Weapons.ContainsName(weapon.name))
                {
                    BlenderAPI.LogWarning($"Tried to load a weapon named \"{weapon.name}\" that didn't have a matching weapon.");
                    continue;
                }

                WeaponInfo info = Weapons.GetValue(weapon.name);
                if (!info.WeaponType.IsSubclassOf(typeof(AbstractLevelWeapon)))
                {
                    BlenderAPI.LogError($"Couldn't load a weapon named \"{weapon.name}\" because its weapon type is not of type \"AbstractLevelWeapon\".");
                    continue;
                }
                AbstractLevelWeapon weaponComponent = (AbstractLevelWeapon)weaponObj.AddComponent(info.WeaponType);
                
                if (weaponObj.transform.childCount < 1)
                {
                    BlenderAPI.LogError($"Couldn't load a weapon named \"{weapon.name}\" because it was missing a basic projectile child.");
                    continue;
                }

                Transform basic = weaponObj.transform.GetChild(0);
                AbstractProjectile basicComponent = (AbstractProjectile)basic.gameObject.AddComponent(info.BasicType);
                SetupProjectile(basicComponent);
                weaponComponent.basicPrefab = AssetHelper.AddPrefab(basic.gameObject, true).GetComponent<AbstractProjectile>();
                GameObject.Destroy(basic.gameObject);

                if (weaponObj.transform.childCount > 1)
                {
                    Transform ex = weaponObj.transform.GetChild(1);
                    AbstractProjectile exComponent = (AbstractProjectile)ex.gameObject.AddComponent(info.ExType);
                    SetupProjectile(exComponent);
                    weaponComponent.exPrefab = AssetHelper.AddPrefab(ex.gameObject, true).GetComponent<AbstractProjectile>();
                    GameObject.Destroy(ex.gameObject);

                    if (weaponObj.transform.childCount > 2)
                    {
                        Transform basicEffect = weaponObj.transform.GetChild(2);
                        WeaponSparkEffect effectComponent = SetupEffect(basicEffect.gameObject, info.BasicEffectType);
                        weaponComponent.basicEffectPrefab = AssetHelper.AddPrefab(basicEffect.gameObject, true).GetComponent<WeaponSparkEffect>();
                        GameObject.Destroy(effectComponent.gameObject);

                        if (weaponObj.transform.childCount > 3)
                        {
                            Transform exEffect = weaponObj.transform.GetChild(3);
                            weaponComponent.exEffectPrefab  = AssetHelper.AddPrefab(exEffect.gameObject, true).GetComponent<WeaponSparkEffect>();
                            GameObject.Destroy(exEffect.gameObject);
                        }
                    }
                }
                AssetHelper.AddPrefab(weaponObj, false);
            }
        };
    }

    private static void SetupProjectile(AbstractProjectile projectile)
    {
        SpriteRenderer renderer = projectile.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.sortingLayerName = "Projectiles";
        }

        Animator animator = projectile.GetComponent<Animator>();
        if (animator != null)
        {
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name.ToLower().Contains("death"))
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
        projectile.gameObject.AddComponent<AnimationHelper>();

        projectile.gameObject.tag = "PlayerProjectile";
        projectile.gameObject.layer = LayerMask.NameToLayer("Projectile");
    }

    private static WeaponSparkEffect SetupEffect(GameObject effect, Type effectType)
    {
        SpriteRenderer renderer = effect.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.sortingLayerName = "Player";
        }

        WeaponSparkEffect effectComponent = (WeaponSparkEffect)effect.AddComponent(effectType);
        effectComponent.randomMirrorX = true;
        effectComponent.randomMirrorY = true;
        effectComponent.randomRotation = true;
        effect.AddComponent<AnimationHelper>();
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
        SuperPages[1] = MapEquipUICardBackSelect.SUPERS;
    }
}