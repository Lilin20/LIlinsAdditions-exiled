using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;

namespace LilinsAdditions.Items.Weapons.LMGs;

[CustomItem(ItemType.GunLogicer)]
public class ExplosiveLMG : CustomWeapon
{
    private const float PLAYER_DAMAGE_MULTIPLIER = 0.01f;
    private const float DISABLE_EFFECT_MULTIPLIER = 0f;
    private const float NANO_ROCKET_FUSE_TIME = 0.01f;
    private const float SPAWN_HEIGHT_OFFSET = 0.1f;

    public override uint Id { get; set; } = 401;
    public override float Damage { get; set; } = 0.1f;
    public override string Name { get; set; } = "Prototype LMG - Nano Rockets";
    public override string Description { get; set; } = "Shoots nano rockets.";
    public override byte ClipSize { get; set; } = 200;
    public override float Weight { get; set; } = 0.5f;
    public override SpawnProperties SpawnProperties { get; set; }

    protected override void SubscribeEvents()
    {
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        base.UnsubscribeEvents();
    }

    protected override void OnReloading(ReloadingWeaponEventArgs ev)
    {
        // Disabled for now.
        //ev.IsAllowed = false;
        base.OnReloading(ev);
    }
}