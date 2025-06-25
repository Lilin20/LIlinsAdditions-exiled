using Exiled.API.Enums;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class ShadowStep : CustomItem
    {
        public override uint Id { get; set; } = 805;
        public override string Name { get; set; } = "Shadow Step";
        public override string Description { get; set; } = "Sieh was vor dir liegt.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingShadowStep;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingShadowStep;
            base.UnsubscribeEvents();
        }

        private void OnUsingShadowStep(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem)) return;

            Timing.CallDelayed(2f, () =>
            {
                Vector3 oldPos = ev.Player.Position;
                ev.Player.EnableEffect(EffectType.Ghostly);
                ev.Player.EnableEffect(EffectType.SilentWalk, 100);
                ev.Player.EnableEffect(EffectType.FogControl, 0);
                ev.Player.EnableEffect(EffectType.Invisible);
                ev.Player.Handcuff();

                ev.Item.Destroy();

                Timing.CallDelayed(15f, () =>
                {
                    ev.Player.Teleport(oldPos);
                    ev.Player.DisableAllEffects();
                    ev.Player.RemoveHandcuffs();
                });
            });

            
        }
    }
}
