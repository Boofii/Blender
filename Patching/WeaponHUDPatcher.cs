using Blender.Content;
using Blender.Utility;
using HarmonyLib;
using System.Collections;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Blender.Patching;

[HarmonyPatch(typeof(LevelHUDWeapon))]
internal static class WeaponHUDPatcher
{
    [HarmonyPatch(nameof(LevelHUDWeapon.Create))]
    [HarmonyPrefix]
    private static bool OnIconCreated(LevelHUDWeapon __instance, Transform parent, Weapon weapon, ref LevelHUDWeapon __result)
    {
        if (!EquipRegistries.Weapons.ContainsName(weapon.ToString()))
            return true;

        LevelHUDWeapon levelHUDWeapon = __instance.InstantiatePrefab<LevelHUDWeapon>();
        levelHUDWeapon.animator.enabled = false;
        levelHUDWeapon.transform.SetParent(parent, false);
        CustomAnimation animComponent = levelHUDWeapon.gameObject.AddComponent<CustomAnimation>();
        animComponent.Init(weapon);
        __result = levelHUDWeapon;

        return false;
    }

    internal static void Initialize(Harmony harmony)
    {
        harmony.PatchAll(typeof(WeaponHUDPatcher));
    }

    public class CustomAnimation : MonoBehaviour
    {
        public Weapon weapon;
        private Sprite[] sprites;
        private Image image;
        private int index = 0;

        public void Init(Weapon weapon)
        {
            this.weapon = weapon;
            this.image = gameObject.GetComponent<Image>();
            this.GetSprites();
            if (sprites == null)
                this.enabled = false;
            else
                StartCoroutine(SwitchSprite());
        }

        private IEnumerator SwitchSprite()
        {
            yield return new WaitForSeconds(0.07F);
            image.overrideSprite = sprites[index];
            index += 1;
            if (index >= 3)
                index = 0;
            StartCoroutine(SwitchSprite());
        }

        private void GetSprites()
        {
            WeaponInfo info = EquipRegistries.Weapons.GetValue(weapon.ToString());
            if (info.NormalIcons.Length == 0)
                return;

            if (SingleLoader.Instance.tryGetAsset(info.AtlasPath, out Object obj))
            {
                SpriteAtlas atlas = obj as SpriteAtlas;
                string[] icons = info.NormalIcons;
                this.sprites = new Sprite[icons.Length];
                sprites[0] = atlas.GetSprite(icons[0]);
                sprites[1] = atlas.GetSprite(icons[1]);
                sprites[2] = atlas.GetSprite(icons[2]);
            }
        }
    }
}