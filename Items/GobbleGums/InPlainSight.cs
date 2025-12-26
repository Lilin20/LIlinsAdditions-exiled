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
    public class InPlainSight : FortunaFizzItem
    {
        private const float USE_DELAY = 2f;
        private const float DEFAULT_INVISIBLE_DURATION = 5f;

        public override uint Id { get; set; } = 802;
        public override string Name { get; set; } = "In Plain Sight";
        public override string Description { get; set; } = "Makes you invisible for a period of time.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        public float InvisibleDuration { get; set; } = DEFAULT_INVISIBLE_DURATION;

        public InPlainSight()
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

            Timing.CallDelayed(USE_DELAY, () => ApplyInvisibility(ev));
        }

        private void ApplyInvisibility(UsingItemEventArgs ev)
        {
            if (ev.Player == null || !ev.Player.IsAlive)
                return;

            ev.Player.EnableEffect(EffectType.Invisible, InvisibleDuration, addDurationIfActive: false);
            ev.Item?.Destroy();
            
            Log.Debug($"[InPlainSight] {ev.Player.Nickname} became invisible for {InvisibleDuration}s");
        }
    }
}
