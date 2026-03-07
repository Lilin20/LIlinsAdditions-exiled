using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Roles;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using MEC;

namespace LilinsAdditions.Main.Items.GobbleGums;

[CustomItem(ItemType.AntiSCP207)]
public class InPlainSight : FortunaFizzItem
{
    private static readonly HashSet<Player> InvisiblePlayers = new();

    public InPlainSight()
    {
        Buyable = true;
    }

    public override uint Id { get; set; } = 802;
    public override string Name { get; set; } = "In Plain Sight";
    public override string Description { get; set; } = "Makes you invisible for a period of time.";
    public override float Weight { get; set; } = 0.5f;
    public override SpawnProperties SpawnProperties { get; set; }

    public float InvisibleDuration { get; set; } = 5;

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
        Exiled.Events.Handlers.Player.Shooting += OnShooting;
        Exiled.Events.Handlers.Player.ChangingItem += OnChangingItem;
        Exiled.Events.Handlers.Player.DroppingItem += OnDroppingItem;
        Exiled.Events.Handlers.Player.PickingUpItem += OnPickingUpItem;
        Exiled.Events.Handlers.Player.InteractingDoor += OnInteractingDoor;
        Exiled.Events.Handlers.Player.InteractingElevator += OnInteractingElevator;
        Exiled.Events.Handlers.Player.InteractingLocker += OnInteractingLocker;
        Exiled.Events.Handlers.Player.Handcuffing += OnHandcuffing;
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
        Exiled.Events.Handlers.Player.Shooting -= OnShooting;
        Exiled.Events.Handlers.Player.ChangingItem -= OnChangingItem;
        Exiled.Events.Handlers.Player.DroppingItem -= OnDroppingItem;
        Exiled.Events.Handlers.Player.PickingUpItem -= OnPickingUpItem;
        Exiled.Events.Handlers.Player.InteractingDoor -= OnInteractingDoor;
        Exiled.Events.Handlers.Player.InteractingElevator -= OnInteractingElevator;
        Exiled.Events.Handlers.Player.InteractingLocker -= OnInteractingLocker;
        Exiled.Events.Handlers.Player.Handcuffing -= OnHandcuffing;
        base.UnsubscribeEvents();
    }

    private void OnUsingItem(UsingItemEventArgs ev)
    {
        if (!Check(ev.Player.CurrentItem))
            return;

        ev.IsAllowed = false;
        ApplyInvisibility(ev);
    }

    private void ApplyInvisibility(UsingItemEventArgs ev)
    {
        if (ev.Player == null || !ev.Player.IsAlive)
            return;

        if (ev.Player.Role is FpcRole fpcRole)
        {
            fpcRole.IsInvisible = true;
            InvisiblePlayers.Add(ev.Player);
            ev.Item?.Destroy();

            Timing.CallDelayed(InvisibleDuration, () =>
            {
                if (ev.Player != null && InvisiblePlayers.Contains(ev.Player))
                {
                    fpcRole.IsInvisible = false;
                    InvisiblePlayers.Remove(ev.Player);
                }
            });

            Log.Debug($"[InPlainSight] {ev.Player.Nickname} became invisible for {InvisibleDuration}s");
        }
    }

    private void OnShooting(ShootingEventArgs ev)
    {
        BreakInvisibility(ev.Player);
    }

    private void OnChangingItem(ChangingItemEventArgs ev)
    {
        BreakInvisibility(ev.Player);
    }

    private void OnDroppingItem(DroppingItemEventArgs ev)
    {
        BreakInvisibility(ev.Player);
    }

    private void OnPickingUpItem(PickingUpItemEventArgs ev)
    {
        BreakInvisibility(ev.Player);
    }

    private void OnInteractingDoor(InteractingDoorEventArgs ev)
    {
        BreakInvisibility(ev.Player);
    }

    private void OnInteractingElevator(InteractingElevatorEventArgs ev)
    {
        BreakInvisibility(ev.Player);
    }

    private void OnInteractingLocker(InteractingLockerEventArgs ev)
    {
        BreakInvisibility(ev.Player);
    }

    private void OnHandcuffing(HandcuffingEventArgs ev)
    {
        BreakInvisibility(ev.Player);
    }

    private void BreakInvisibility(Player player)
    {
        if (player != null && InvisiblePlayers.Contains(player))
            if (player.Role.Is(out FpcRole role))
            {
                role.IsInvisible = false;
                InvisiblePlayers.Remove(player);
                Log.Debug($"[InPlainSight] {player.Nickname} revealed due to interaction");
            }
    }
}