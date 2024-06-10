namespace Blender;

public class WeaponSpark : AbstractLevelWeapon
{
    public override bool rapidFire => true;
    public override float rapidFireRate => 0.16F;

    public override AbstractProjectile fireBasic()
    {
        BasicProjectile spark = base.fireBasic() as BasicProjectile;
        spark.Speed = 1125F;
        spark.Damage = 5F;
        spark.PlayerId = player.id;
        spark.DamagesType.PlayerProjectileDefault();
        spark.CollisionDeath.PlayerProjectileDefault();
        player.stats.SuperMeter += 10;
        player.stats.Health += 1;
        player.stats.OnSuperChanged();
        player.stats.OnHealthChanged();
        return spark;
    }

    public override AbstractProjectile fireEx()
    {
        BasicProjectile ex = base.fireEx() as BasicProjectile;
        ex.Speed = 0F;
        ex.Damage = 20F;
        ex.PlayerId = player.id;
        ex.DamagesType.PlayerProjectileDefault();
        ex.CollisionDeath.PlayerProjectileDefault();
        ex.CollisionDeath.SetBounds(false);
        MeterScoreTracker meterScoreTracker = new MeterScoreTracker(MeterScoreTracker.Type.Ex);
        meterScoreTracker.Add(ex);
        ex.transform.SetPosition(y: 100);
        return ex;
    }

    public override void BeginBasic()
    {
        base.BeginBasic();
        BasicSoundLoop("player_weapon_spread_loop", "player_weapon_spread_loop_p2");
    }

    public override void EndBasic()
    {
        ActivateCooldown();
        base.EndBasic();
        StopLoopSound("player_weapon_spread_loop", "player_weapon_spread_loop_p2");
    }
}