using Exiled.API.Enums;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items.FirearmModules.Primary;
using Exiled.API.Features.Roles;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class LightHeaded : CustomItem
    {
        public override uint Id { get; set; } = 815;
        public override string Name { get; set; } = "Light Headed";
        public override string Description { get; set; } = "Alles fühlt sich so leicht an?";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingLightHeaded;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingLightHeaded;
            base.UnsubscribeEvents();
        }

        private void OnUsingLightHeaded(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem)) return;

            var normalGravity = new Vector3(0, 0, 0);

            if (ev.Player.Role is FpcRole fpc)
            {
                normalGravity = fpc.Gravity;

                Timing.CallDelayed(2f, () =>
                {
                    fpc.Gravity = new Vector3(0, -3.8f, 0);

                    ev.Item.Destroy();

                    Timing.CallDelayed(15f, () =>
                    {
                        fpc.Gravity = normalGravity;
                    });
                });
            }
        }
    }
}
