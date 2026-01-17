using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class SilentStep : FortunaFizzItem
    {
        private const float USE_DELAY = 2f;
        private const byte SILENT_WALK_INTENSITY = 255;
        private const float EFFECT_DURATION = 30f;

        public override uint Id { get; set; } = 812;
        public override string Name { get; set; } = "Silent Step";
        public override string Description { get; set; } = $"Unable to make sounds. ({EFFECT_DURATION} sec.)";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        public SilentStep()
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

            Timing.CallDelayed(USE_DELAY, () => ApplySilentWalk(ev));
        }

        private static void ApplySilentWalk(UsingItemEventArgs ev)
        {
            if (ev.Player == null || !ev.Player.IsAlive)
                return;

            ev.Player.EnableEffect(EffectType.SilentWalk, SILENT_WALK_INTENSITY, EFFECT_DURATION);
            ev.Item?.Destroy();
            
            Log.Debug($"[SilentStep] {ev.Player.Nickname} enabled silent walk for {EFFECT_DURATION}s");
        }
    }
}
