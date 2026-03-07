using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using Player = Exiled.Events.Handlers.Player;

namespace LilinsAdditions.Main.Items.GobbleGums;

[CustomItem(ItemType.AntiSCP207)]
public class NowYouSeeMe : FortunaFizzItem
{
    private const float USE_DELAY = 2f;
    private const float VISION_DURATION = 30f;

    public NowYouSeeMe()
    {
        Buyable = true;
    }

    public override uint Id { get; set; } = 803;
    public override string Name { get; set; } = "Now You See Me";
    public override string Description { get; set; } = "Shows you everything.";
    public override float Weight { get; set; } = 0.5f;
    public override SpawnProperties SpawnProperties { get; set; }

    protected override void SubscribeEvents()
    {
        Player.UsingItem += OnUsingItem;
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Player.UsingItem -= OnUsingItem;
        base.UnsubscribeEvents();
    }

    private void OnUsingItem(UsingItemEventArgs ev)
    {
        if (!Check(ev.Player.CurrentItem))
            return;

        ev.IsAllowed = false;

        ApplyEnhancedVision(ev);
    }

    private static void ApplyEnhancedVision(UsingItemEventArgs ev)
    {
        if (ev.Player == null || !ev.Player.IsAlive)
            return;

        ev.Player.EnableEffect(EffectType.Scp1344, VISION_DURATION, false);
        ev.Item?.Destroy();

        Log.Debug($"[NowYouSeeMe] {ev.Player.Nickname} gained enhanced vision for {VISION_DURATION}s");
    }
}