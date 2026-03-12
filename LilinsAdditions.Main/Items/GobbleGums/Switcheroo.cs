using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Roles;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using UnityEngine;

namespace LilinsAdditions.Main.Items.GobbleGums;

[CustomItem(ItemType.AntiSCP207)]
public class Switcheroo : FortunaFizzItem
{
    private static readonly HashSet<RoleTypeId> SwapIgnoredRoles = new()
    {
        RoleTypeId.Spectator,
        RoleTypeId.Filmmaker,
        RoleTypeId.Overwatch,
        RoleTypeId.Scp079,
        RoleTypeId.Tutorial
    };

    private static readonly HashSet<RoleTypeId> ScpRoles = new()
    {
        RoleTypeId.Scp173,
        RoleTypeId.Scp049,
        RoleTypeId.Scp096,
        RoleTypeId.Scp106,
        RoleTypeId.Scp0492,
        RoleTypeId.Scp939,
        RoleTypeId.Scp3114
    };

    public Switcheroo()
    {
        Buyable = true;
    }

    public override uint Id { get; set; } = 813;
    public override string Name { get; set; } = "Switcherooo";
    public override string Description { get; set; } = "Swap places – the chaos decides.";
    public override float Weight { get; set; } = 0.5f;
    public string GroundedMessage { get; set; } = "You must be on the ground to use Switcheroo!";
    public string ScpNearbyMessage { get; set; } = "You cannot use Switcheroo while an SCP is nearby!";
    public string WarheadActiveMessage { get; set; } = "You cannot use Switcherooo while the Warhead is active!";

    public string LczDeconMessage { get; set; } =
        "You cannot use Switcherooo when you are in the LCZ while its being decontaminated!";

    public float ScpDetectionRange { get; set; } = 20f;
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

        ExecuteSwap(ev);
    }

    private void ExecuteSwap(UsingItemEventArgs ev)
    {
        if (ev.Player == null || !ev.Player.IsAlive)
            return;

        if (ev.Player.Role is FpcRole fpcRole && !fpcRole.IsGrounded)
        {
            ev.Player.ShowHint(GroundedMessage);
            return;
        }

        if (IsScpNearby(ev.Player))
        {
            ev.Player.ShowHint(ScpNearbyMessage);
            return;
        }

        if (Warhead.IsInProgress)
        {
            ev.Player.ShowHint(WarheadActiveMessage);
            return;
        }

        if (Map.IsLczDecontaminated && ev.Player.Zone == ZoneType.LightContainment)
        {
            ev.Player.ShowHint(LczDeconMessage);
            return;
        }

        var targetPlayer = GetRandomSwapTarget(ev.Player);
        if (targetPlayer == null)
            return;

        SwapPlayerPositions(ev.Player, targetPlayer);
        ev.Item?.Destroy();
    }

    private bool IsScpNearby(Player player)
    {
        return Player.List.Any(p =>
            p != player &&
            p.IsAlive &&
            ScpRoles.Contains(p.Role.Type) &&
            Vector3.Distance(player.Position, p.Position) <= ScpDetectionRange);
    }

    private static Player GetRandomSwapTarget(Player excludePlayer)
    {
        var eligiblePlayers = Player.List
            .Where(p => p != excludePlayer
                        && p.IsAlive
                        && !SwapIgnoredRoles.Contains(p.Role.Type))
            .ToList();

        return eligiblePlayers.Count > 0 ? eligiblePlayers.RandomItem() : null;
    }

    private static void SwapPlayerPositions(Player player1, Player player2)
    {
        var tempPosition = player2.Position;
        player2.Teleport(player1.Position);
        player1.Teleport(tempPosition);
    }
}