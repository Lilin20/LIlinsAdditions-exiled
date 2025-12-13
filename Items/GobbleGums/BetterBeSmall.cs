using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class BetterBeSmall : FortunaFizzItem
    {
        private const float USE_DELAY = 2f;
        private const float SCALE_REDUCTION = 0.2f;
        private const float MIN_SCALE = 0.4f;
        
        public override uint Id { get; set; } = 810;
        public override string Name { get; set; } = "Better Be Small";
        public override string Description { get; set; } = "Being small also has its benefits.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        public BetterBeSmall()
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

            Timing.CallDelayed(USE_DELAY, () => ApplyScaleReduction(ev));
        }

        private static void ApplyScaleReduction(UsingItemEventArgs ev)
        {
            if (ev.Player == null || !ev.Player.IsAlive)
                return;

            var currentScale = ev.Player.Scale;
            ev.Player.Scale = new Vector3(
                Mathf.Max(currentScale.x - SCALE_REDUCTION, MIN_SCALE),
                Mathf.Max(currentScale.y - SCALE_REDUCTION, MIN_SCALE),
                Mathf.Max(currentScale.z - SCALE_REDUCTION, MIN_SCALE)
            );

            ev.Item?.Destroy();
        }
    }
}
