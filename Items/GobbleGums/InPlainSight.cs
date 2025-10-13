using Exiled.API.Enums;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class InPlainSight : CustomItem
    {
        public override uint Id { get; set; } = 802;
        public override string Name { get; set; } = "In Plain Sight";
        public override string Description { get; set; } = "Makes you invisible for a period of time.";
        public override float Weight { get; set; } = 0.5f;
        public float InvisibleDuration { get; set; } = 5f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingInPlainSight;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingInPlainSight;
            base.UnsubscribeEvents();
        }

        private void OnUsingInPlainSight(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem)) return;

            Timing.CallDelayed(2f, () =>
            {
                ev.Player.EnableEffect(EffectType.Invisible, InvisibleDuration, false);

                ev.Item.Destroy();
            });
        }
    }
}
