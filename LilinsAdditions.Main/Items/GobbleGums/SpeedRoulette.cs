using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using UnityEngine;

namespace LilinsAdditions.Main.Items.GobbleGums;

[CustomItem(ItemType.AntiSCP207)]
public class SpeedRoulette : FortunaFizzItem
{
    private const string SPEED_MODIFIER_KEY = "SpeedModifierNet";

    public SpeedRoulette()
    {
        Buyable = true;
    }

    public override uint Id { get; set; } = 821;
    public override string Name { get; set; } = "Speed Roulette";
    public override string Description { get; set; } = "50% chance for +10% speed, 50% chance for -20% speed.";
    public override float Weight { get; set; } = 0.5f;
    public int SpeedBoostAmount { get; set; } = 10;
    public int SlownessBoostAmount { get; set; } = 20;
    public float EffectDuration { get; set; } = 999f;
    public byte IntensityPer10Percent { get; set; } = 8;
    public override SpawnProperties SpawnProperties { get; set; }

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
        base.UnsubscribeEvents();
    }

    private void OnUsingItem(UsingItemEventArgs ev)
    {
        if (!Check(ev.Player.CurrentItem))
            return;

        ev.IsAllowed = false;

        ExecuteSpeedEffect(ev);
    }

    private void ExecuteSpeedEffect(UsingItemEventArgs ev)
    {
        if (!IsValidItemUse(ev))
            return;

        var netModifier = GetOrInitializeNetModifier(ev.Player);
        var isPositive = Random.Range(0, 2) == 0;

        if (isPositive)
        {
            netModifier += SpeedBoostAmount;
            ev.Player.ShowHint($"⚡ SPEED BOOST! (+{SpeedBoostAmount}% speed.", 5f);
        }
        else
        {
            netModifier -= SlownessBoostAmount;
            ev.Player.ShowHint($"🐌 SLOWED DOWN! (-{SlownessBoostAmount}% speed.", 5f);
        }

        ev.Player.SessionVariables[SPEED_MODIFIER_KEY] = netModifier;

        ApplyNetSpeedEffect(ev.Player, netModifier);

        ev.Item?.Destroy();
    }

    private void ApplyNetSpeedEffect(Player player, int netModifier)
    {
        player.DisableEffect(EffectType.MovementBoost);
        player.DisableEffect(EffectType.Slowness);

        if (netModifier > 0)
        {
            var intensity = CalculateIntensity(netModifier);
            player.EnableEffect(EffectType.MovementBoost, intensity, EffectDuration);
        }
        else if (netModifier < 0)
        {
            var intensity = CalculateIntensity(Mathf.Abs(netModifier));
            player.EnableEffect(EffectType.Slowness, intensity, EffectDuration);
        }
    }

    private byte CalculateIntensity(int percentModifier)
    {
        var intensity = percentModifier * IntensityPer10Percent / 10;

        return (byte)Mathf.Clamp(intensity, 0, 255);
    }

    private static bool IsValidItemUse(UsingItemEventArgs ev)
    {
        return ev.Player != null &&
               ev.Player.IsAlive &&
               ev.Player.CurrentItem == ev.Item;
    }

    private static int GetOrInitializeNetModifier(Player player)
    {
        if (!player.SessionVariables.ContainsKey(SPEED_MODIFIER_KEY))
            player.SessionVariables[SPEED_MODIFIER_KEY] = 0;

        return (int)player.SessionVariables[SPEED_MODIFIER_KEY];
    }
}