using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Roles;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class DeathCheat : FortunaFizzItem
    {
        private const float USE_DELAY = 2f;
        private const float REVIVE_CHANCE = 0.8f;
        private const float MIN_REVIVE_DELAY = 30f;
        private const float MAX_REVIVE_DELAY = 121f;
        private const string DEATH_HINT = "Etwas greift nach deiner Seele...";
        private const string REVIVE_HINT = "Wake the fuck up Samurai - We got SCPs to kill...";

        private readonly Dictionary<Player, Role> _protectedPlayers = new();

        public override uint Id { get; set; } = 804;
        public override string Name { get; set; } = "Death Cheat";
        public override string Description { get; set; } = "Revives you from the dead. Maybe.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        public DeathCheat()
        {
            Buyable = true;
        }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
            Exiled.Events.Handlers.Player.Dying += OnPlayerDying;
            Exiled.Events.Handlers.Player.Left += OnPlayerLeft;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
            Exiled.Events.Handlers.Player.Dying -= OnPlayerDying;
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeft;
            base.UnsubscribeEvents();
        }

        private void OnUsingItem(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            Timing.CallDelayed(USE_DELAY, () => ActivateProtection(ev));
        }

        private void ActivateProtection(UsingItemEventArgs ev)
        {
            if (ev.Player == null || !ev.Player.IsAlive)
                return;

            _protectedPlayers[ev.Player] = ev.Player.Role;
            ev.Item?.Destroy();
            
            Log.Debug($"[DeathCheat] {ev.Player.Nickname} is now protected");
        }

        private void OnPlayerDying(DyingEventArgs ev)
        {
            if (!_protectedPlayers.TryGetValue(ev.Player, out var originalRole))
                return;

            var shouldRevive = Random.value <= REVIVE_CHANCE;
            
            if (shouldRevive)
            {
                var reviveDelay = Random.Range(MIN_REVIVE_DELAY, MAX_REVIVE_DELAY);
                ev.Player.ShowHint(DEATH_HINT);
                
                Timing.CallDelayed(reviveDelay, () => AttemptRevive(ev.Player, originalRole));
            }

            _protectedPlayers.Remove(ev.Player);
            Log.Debug($"[DeathCheat] {ev.Player.Nickname} protection consumed (revive: {shouldRevive})");
        }

        private static void AttemptRevive(Player player, Role originalRole)
        {
            if (player == null || player.Role is not SpectatorRole)
                return;

            player.Role.Set(originalRole, RoleSpawnFlags.None);
            player.ShowHint(REVIVE_HINT);
            
            Log.Debug($"[DeathCheat] {player.Nickname} revived as {originalRole.Type}");
        }

        private void OnPlayerLeft(LeftEventArgs ev)
        {
            _protectedPlayers.Remove(ev.Player);
        }
    }
}
