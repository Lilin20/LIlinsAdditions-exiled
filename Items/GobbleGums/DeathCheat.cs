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

        private readonly Dictionary<Player, Role> _protectedPlayers = new();

        public override uint Id { get; set; } = 804;
        public override string Name { get; set; } = "Death Cheat";
        public override string Description { get; set; } = "Revives you from the dead. Maybe.";
        public override float Weight { get; set; } = 0.5f;
        public float ReviveChance { get; set; } = 0.8f;
        public float MinReviveDelay { get; set; }  = 30f;
        public float MaxReviveDelay { get; set; }  = 121f;
        public string DeathHint { get; set; }  = "Something grasps your soul...";
        public string ReviveHint { get; set; }  = "Wake up.";
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

            var shouldRevive = Random.value <= ReviveChance;
            
            if (shouldRevive)
            {
                var reviveDelay = Random.Range(MinReviveDelay, MaxReviveDelay);
                ev.Player.ShowHint(DeathHint);
                
                Timing.CallDelayed(reviveDelay, () => AttemptRevive(ev.Player, originalRole));
            }

            _protectedPlayers.Remove(ev.Player);
            Log.Debug($"[DeathCheat] {ev.Player.Nickname} protection consumed (revive: {shouldRevive})");
        }

        private void AttemptRevive(Player player, Role originalRole)
        {
            if (player == null || player.Role is not SpectatorRole)
                return;

            player.Role.Set(originalRole, RoleSpawnFlags.None);
            player.ShowHint(ReviveHint);
            
            Log.Debug($"[DeathCheat] {player.Nickname} revived as {originalRole.Type}");
        }

        private void OnPlayerLeft(LeftEventArgs ev)
        {
            _protectedPlayers.Remove(ev.Player);
        }
    }
}
