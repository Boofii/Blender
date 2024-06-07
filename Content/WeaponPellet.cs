namespace Blender.Content;

public class WeaponPellet : AbstractLevelWeapon
{

    private readonly float[] yPositions = new float[8] { 0f, 20f, 40f, 60F, 80F, 60F, 40F, 20F };

    private int currentY;

    public override bool rapidFire => true;

    public override float rapidFireRate => 0.10F;

    public override AbstractProjectile fireBasic()
    {
        BasicProjectile basicProjectile = base.fireBasic() as BasicProjectile;
        basicProjectile.Speed = 1125F;
        basicProjectile.Damage = 5F;
        basicProjectile.PlayerId = player.id;
        basicProjectile.DamagesType.PlayerProjectileDefault();
        basicProjectile.CollisionDeath.PlayerProjectileDefault();
        float y = yPositions[currentY];
        currentY++;
        if (currentY >= yPositions.Length)
        {
            currentY = 0;
        }

        basicProjectile.transform.AddPosition(0f, y);
        return basicProjectile;
    }

    public override AbstractProjectile fireEx()
    {
        BasicProjectile basicProjectile = base.fireEx() as BasicProjectile;
        basicProjectile.Speed = 1125F;
        basicProjectile.Damage = 10F;
        basicProjectile.PlayerId = player.id;
        basicProjectile.DamagesType.PlayerProjectileDefault();
        basicProjectile.CollisionDeath.PlayerProjectileDefault();
        MeterScoreTracker meterScoreTracker = new MeterScoreTracker(MeterScoreTracker.Type.Ex);
        meterScoreTracker.Add(basicProjectile);
        return basicProjectile;
    }

    public override void BeginBasic()
    {
        OneShotCooldown("player_default_fire_start");
        BasicSoundLoop("player_default_fire_loop", "player_default_fire_loop_p2");
        base.BeginBasic();
    }

    public override void EndBasic()
    {
        ActivateCooldown();
        base.EndBasic();
        StopLoopSound("player_default_fire_loop", "player_default_fire_loop_p2");
    }
}