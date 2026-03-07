using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Roles;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace LilinsAdditions.Main.Items.GobbleGums;

[CustomItem(ItemType.AntiSCP207)]
public class LightHeaded : FortunaFizzItem
{
    private const float USE_DELAY = 2f;
    private const float EFFECT_DURATION = 15f;
    private const float REDUCED_GRAVITY_Y = -3.8f;
    private static readonly Dictionary<Player, Vector3> ActivePlayers = new();

    public LightHeaded()
    {
        Buyable = true;
    }

    public override uint Id { get; set; } = 815;
    public override string Name { get; set; } = "Light Headed";
    public override string Description { get; set; } = "Everything feels so light?";
    public override float Weight { get; set; } = 0.5f;
    public override SpawnProperties SpawnProperties { get; set; }

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
        Exiled.Events.Handlers.Player.Died += OnDied;
        Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
        Exiled.Events.Handlers.Player.Died -= OnDied;
        Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
        base.UnsubscribeEvents();
    }

    private void OnUsingItem(UsingItemEventArgs ev)
    {
        if (!Check(ev.Player.CurrentItem))
            return;

        ev.IsAllowed = false;

        if (ev.Player.Role is not FpcRole fpcRole)
            return;

        var originalGravity = fpcRole.Gravity;
        ApplyReducedGravity(ev, fpcRole, originalGravity);
    }

    private void OnDied(DiedEventArgs ev)
    {
        if (ActivePlayers.TryGetValue(ev.Player, out var originalGravity))
        {
            ActivePlayers.Remove(ev.Player);
            // Restore using the role at death time (still FpcRole here)
            if (ev.Player.Role is FpcRole fpcRole)
            {
                fpcRole.Gravity = originalGravity;
                Log.Debug($"[LightHeaded] {ev.Player.Nickname} gravity restored on death");
            }
        }
    }

    private void OnChangingRole(ChangingRoleEventArgs ev)
    {
        if (ActivePlayers.TryGetValue(ev.Player, out var originalGravity))
        {
            ActivePlayers.Remove(ev.Player);
            // Restore if the role we’re coming from was FpcRole
            if (ev.Player.Role is FpcRole fpcRole)
            {
                fpcRole.Gravity = originalGravity;
                Log.Debug($"[LightHeaded] {ev.Player.Nickname} gravity restored on role change");
            }
        }
    }

    private static void ApplyReducedGravity(UsingItemEventArgs ev, FpcRole fpcRole, Vector3 originalGravity)
    {
        if (ev.Player == null || !ev.Player.IsAlive || ev.Player.Role is not FpcRole)
            return;

        ActivePlayers[ev.Player] = originalGravity;

        fpcRole.Gravity = new Vector3(0, REDUCED_GRAVITY_Y, 0);
        ev.Item?.Destroy();

        Log.Debug($"[LightHeaded] {ev.Player.Nickname} gravity reduced for {EFFECT_DURATION}s");

        Timing.CallDelayed(EFFECT_DURATION, () => RestoreGravity(ev.Player, fpcRole, originalGravity));
    }

    private static void RestoreGravity(Player player, FpcRole fpcRole, Vector3 originalGravity)
    {
        if (player == null || player.Role is not FpcRole)
            return;

        ActivePlayers.Remove(player);
        fpcRole.Gravity = originalGravity;
        Log.Debug($"[LightHeaded] {player.Nickname} gravity restored");
    }
}