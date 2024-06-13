using UnityEngine;

namespace Blender.Testing;

public class WeaponSword : AbstractLevelWeapon
{
    public override bool rapidFire => false;

    public override float rapidFireRate => 0.5F;

    public override AbstractProjectile fireBasic()
    {
        BasicProjectile sword = base.fireBasic() as BasicProjectile;
        sword.Speed = 0F;
        sword.Damage = 10F;
        sword.PlayerId = player.id;
        sword.DamagesType.PlayerProjectileDefault();
        sword.CollisionDeath.PlayerProjectileDefault();
        sword.CollisionDeath.EnemyProjectiles = true;
        sword.transform.parent = player.transform;
        sword.transform.position += new Vector3(1F, 0F);
        AudioManager.Play("genie_chest_sword_attack");
        return sword;
    }
}