using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class Juggernaut : FortunaFizzItem
    {
        private const float USE_DELAY = 2f;
        private const float HEALTH_MULTIPLIER = 1.1f;
        private const float HEALTH_INCREASE_PERCENT = 10f;

        public override uint Id { get; set; } = 811;
        public override string Name { get; set; } = "Juggernaut";
        public override string Description { get; set; } = $"Increase your max. HP by {HEALTH_INCREASE_PERCENT}%";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        public Juggernaut()
        {
            Buyable = true;
        }

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
            
            float cooldownEndTime = ev.Player.GetCooldownItem(ItemType.AntiSCP207);
            if (cooldownEndTime > Time.timeSinceLevelLoad)
            {
                ev.IsAllowed = false;
                return;
            }
            
            ev.Player.SetCooldownItem(USE_DELAY, ItemType.AntiSCP207);
            
            Timing.CallDelayed(USE_DELAY, () => IncreaseMaxHealth(ev));
        }

        private static void IncreaseMaxHealth(UsingItemEventArgs ev)
        {
            if (ev.Player == null || !ev.Player.IsAlive)
                return;

            var oldMaxHealth = ev.Player.MaxHealth;
            ev.Player.MaxHealth *= HEALTH_MULTIPLIER;
            
            Log.Debug($"[Juggernaut] {ev.Player.Nickname} max health: {oldMaxHealth} -> {ev.Player.MaxHealth}");
            
            ev.Item?.Destroy();
        }
    }
}
