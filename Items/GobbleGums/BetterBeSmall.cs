using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class BetterBeSmall : CustomItem
    {
        public override uint Id { get; set; } = 810;
        public override string Name { get; set; } = "Better Be Small";
        public override string Description { get; set; } = "Klein sein hat auch seine Vorteile.";
        public override float Weight { get; set; } = 0.5f;

        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingBetterBeSmall;

            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingBetterBeSmall;

            base.UnsubscribeEvents();
        }

        private void OnUsingBetterBeSmall(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            Timing.CallDelayed(2f, () =>
            {
                Vector3 currentSize = ev.Player.Scale;

                float minScale = 0.2f;

                Vector3 newScale = new Vector3(
                    Mathf.Max(currentSize.x - 0.2f, minScale),
                    Mathf.Max(currentSize.y - 0.2f, minScale),
                    Mathf.Max(currentSize.z - 0.2f, minScale)
                );

                ev.Player.Scale = newScale;

                ev.Item.Destroy();
            });
        }
    }
}
