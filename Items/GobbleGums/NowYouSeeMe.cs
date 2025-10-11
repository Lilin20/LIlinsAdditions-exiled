using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Enums;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class NowYouSeeMe : CustomItem
    {
        public override uint Id { get; set; } = 803;
        public override string Name { get; set; } = "Now You See Me";
        public override string Description { get; set; } = "Shows you everything.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingNowYouSeeMe;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingNowYouSeeMe;
            base.UnsubscribeEvents();
        }

        private void OnUsingNowYouSeeMe(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem)) return;

            Timing.CallDelayed(2f, () =>
            {
                ev.Player.EnableEffect(EffectType.Scp1344, 30, false);

                ev.Item.Destroy();
            });
        }
    }
}
