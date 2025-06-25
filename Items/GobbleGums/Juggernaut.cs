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
    public class Juggernaut : CustomItem
    {
        public override uint Id { get; set; } = 811;
        public override string Name { get; set; } = "Juggernaut";
        public override string Description { get; set; } = "Erhöht deine max. HP um 10%";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingJuggernaut;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingJuggernaut;
            base.UnsubscribeEvents();
        }

        private void OnUsingJuggernaut(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem)) return;

            Timing.CallDelayed(2f, () =>
            {
                ev.Player.MaxHealth *= 1.1f;

                ev.Item.Destroy();
            });
        }
    }
}
