using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class SilentStep : CustomItem
    {
        public override uint Id { get; set; } = 812;
        public override string Name { get; set; } = "Silent Step";
        public override string Description { get; set; } = "Unable to make sounds. (30 Sek.)";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingSilentStep;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingSilentStep;
            base.UnsubscribeEvents();
        }

        private void OnUsingSilentStep(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem)) return;

            Timing.CallDelayed(2f, () =>
            {
                ev.Player.EnableEffect(Exiled.API.Enums.EffectType.SilentWalk, 255, 30, false);

                ev.Item.Destroy();
            });
        }
    }
}
