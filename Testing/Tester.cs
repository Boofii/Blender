using Blender.Content;
using Blender.Utility;
using DialoguerCore;
using HarmonyLib;
using System;
using UnityEngine;

namespace Blender.Testing;

internal static class Tester
{
    internal static GameObject Cupy { get; set; }
    internal static readonly LinkedRegistry<DialoguerDialogues, string> Dialogues = new();

    internal static void Initialize(Harmony harmony)
    {
        harmony.PatchAll(typeof(Tester));
        Dialogues.Register("Cupy_W1", "Value");

        EquipRegistries.Weapons.Register("level_weapon_sword", new WeaponInfo(typeof(WeaponSword))
            .SetBasicName("Sword")
            .SetBundleName("extraweapons")
            .SetNormalIcons(["sword0", "sword1", "sword2"])
            .SetGreyIcons(["sword_grey0", "sword_grey1", "sword_grey2"])
            .AsWeaponInfo());

        EquipRegistries.Weapons.Register("level_weapon_spark", new WeaponInfo(typeof(WeaponSpark))
            .SetBasicName("Spark")
            .SetExName("SparkEX")
            .SetBasicEffectName("Effect")
            .SetBasicEffectType(typeof(SparkEffect))
            .SetBundleName("extraweapons")
            .SetNormalIcons(["spark0", "spark1", "spark2"])
            .SetGreyIcons(["spark_grey0", "spark_grey1", "spark_grey2"])
            .AsWeaponInfo());

        CustomData.DataLoadedEvent += delegate
        {
            Weapon sword = (Weapon)Enum.Parse(typeof(Weapon), "level_weapon_sword");
            Weapon spark = (Weapon)Enum.Parse(typeof(Weapon), "level_weapon_spark");
            PlayerData.Data.Gift(PlayerId.PlayerOne, sword);
            PlayerData.Data.Gift(PlayerId.PlayerOne, spark);
            PlayerData.Data.Gift(PlayerId.PlayerTwo, sword);
            PlayerData.Data.Gift(PlayerId.PlayerTwo, spark);
        };

        Cupy = AssetHelper.CacheAsset<GameObject>("Blender", "extraweapons", "Cupy");
        SpriteRenderer renderer = Cupy.GetComponent<SpriteRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingLayerName = "Map";
        Cupy.transform.position = new Vector3(4.2319F, -3.7021F, -3.5F);
    }

    [HarmonyPatch(typeof(Map), nameof(Map.Awake))]
    [HarmonyPostfix]
    private static void Patch_Awake(Map __instance)
    {
        DialoguerDataManager._data.dialogues.Add(new DialoguerDialogue("Cupy_W1", 0,
            new DialoguerVariables([], [], []),
            [
                new Phase1([]),
                new Phase2([]),
                new Phase3([])
            ]));

        if (__instance.scene == Scenes.scene_map_world_1)
        {
            GameObject cupy = UnityEngine.Object.Instantiate(Cupy);
            cupy.AddComponent<CupyInteraction>();
        }
    }
}