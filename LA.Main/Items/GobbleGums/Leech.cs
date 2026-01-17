using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class LifeLeech : FortunaFizzItem
    {
        private const float USE_DELAY = 2f;

        private readonly HashSet<Player> _activeLeechPlayers = new HashSet<Player>();

        public override uint Id { get; set; } = 818;
        public override string Name { get; set; } = "Vampiric Vigor";
        public override string Description { get; set; } = "Heal a percentage of damage dealt.";
        public override float Weight { get; set; } = 0.5f;
        public float LeechDuration { get; set; } = 30f;
        public float LeechPercentage { get; set; } = 0.25f;
        public string ActivationMessage { get; set; } = $"Life Leech activated!";
        public string ExpirationMessage { get; set; } = "Life Leech has expired!";
        public override SpawnProperties SpawnProperties { get; set; }

        public LifeLeech()
        {
            Buyable = true;
        }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
            Exiled.Events.Handlers.Player.Hurting += OnHurting;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
            Exiled.Events.Handlers.Player.Hurting -= OnHurting;
            base.UnsubscribeEvents();
        }

        private void OnUsingItem(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            float cooldownEndTime = ev.Player.GetCooldownItem(ItemType.AntiSCP207);
            if (cooldownEndTime > Time.timeSinceLevelLoad)
            {
                ev.IsAllowed = false;
                return;
            }
            
            ev.Player.SetCooldownItem(USE_DELAY, ItemType.AntiSCP207);
            
            Timing.CallDelayed(USE_DELAY, () => ActivateLifeLeech(ev));
        }

        private void ActivateLifeLeech(UsingItemEventArgs ev)
        {
            if (!IsValidUser(ev.Player))
                return;

            _activeLeechPlayers.Add(ev.Player);
            ev.Player.ShowHint(ActivationMessage);
            ev.Item?.Destroy();

            Timing.CallDelayed(LeechDuration, () => DeactivateLifeLeech(ev.Player));

            Log.Debug($"[LifeLeech] {ev.Player.Nickname} activated Life Leech effect");
        }

        private void DeactivateLifeLeech(Player player)
        {
            if (player == null)
                return;

            _activeLeechPlayers.Remove(player);

            if (player.IsAlive)
                player.ShowHint(ExpirationMessage);

            Log.Debug($"[LifeLeech] {player.Nickname} Life Leech effect expired");
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            if (!ShouldApplyLeech(ev))
                return;

            ApplyLeechHealing(ev);
        }

        private bool ShouldApplyLeech(HurtingEventArgs ev)
        {
            return ev.Attacker != null
                && ev.Player != null
                && ev.Attacker != ev.Player
                && ev.Attacker.IsAlive
                && _activeLeechPlayers.Contains(ev.Attacker)
                && ev.Amount > 0;
        }

        private void ApplyLeechHealing(HurtingEventArgs ev)
        {
            var healAmount = ev.Amount * LeechPercentage;
            var newHealth = ev.Attacker.Health + healAmount;
            var maxHealth = ev.Attacker.MaxHealth;

            ev.Attacker.Health = newHealth > maxHealth ? maxHealth : newHealth;

            Log.Debug($"[LifeLeech] {ev.Attacker.Nickname} leeched {healAmount} HP from {ev.Player.Nickname}");
        }

        private bool IsValidUser(Player player)
        {
            return player != null && player.IsAlive;
        }
    }
}