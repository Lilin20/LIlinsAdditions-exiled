using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items.FirearmModules.Primary;
using Exiled.API.Features.Roles;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class ShrinkAndRun : CustomItem
    {
        public override uint Id { get; set; } = 818;
        public override string Name { get; set; } = "Shrink and Run";
        public override string Description { get; set; } = "Für kurze Zeit ganz klein!";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingShrinkAndRun;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingShrinkAndRun;
            base.UnsubscribeEvents();
        }

        private void OnUsingShrinkAndRun(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem)) return;

            // Starte nach 2 Sekunden die Schrumpf-Animation
            Timing.CallDelayed(2f, () =>
            {
                ev.Item.Destroy();
                Timing.RunCoroutine(SmoothShrink(ev.Player, new Vector3(0.2f, 0.2f, 0.2f), 0.5f)); // Zielgröße & Dauer
                ev.Player.EnableEffect(EffectType.MovementBoost, 50, 5f);

                // Nach 15 Sekunden wächst der Spieler wieder
                Timing.CallDelayed(5f, () =>
                {
                    Timing.RunCoroutine(SmoothShrink(ev.Player, Vector3.one, 0.5f)); // Zurück auf Normalgröße
                });
            });
        }

        private IEnumerator<float> SmoothShrink(Player player, Vector3 targetScale, float duration)
        {
            Vector3 initialScale = player.Scale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                player.Scale = Vector3.Lerp(initialScale, targetScale, t);
                elapsed += Time.deltaTime;
                yield return Timing.WaitForOneFrame;
            }

            player.Scale = targetScale;
        }
    }
}
