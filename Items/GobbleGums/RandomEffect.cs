using Exiled.API.Enums;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using ProjectMER.Commands.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class RandomEffect : CustomItem
    {
        public override uint Id { get; set; } = 814;
        public override string Name { get; set; } = "Luck of the Draw";
        public override string Description { get; set; } = "Einmal ziehen, alles riskieren.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        public static List<EffectType> EffectList = Enum.GetValues(typeof(EffectType))
            .Cast<EffectType>()
            .Where(e => e != EffectType.PocketCorroding &&
                        e != EffectType.PitDeath &&
                        e != EffectType.Decontaminating &&
                        e != EffectType.FogControl &&
                        e != EffectType.None &&
                        e != EffectType.Marshmallow &&
                        e != EffectType.InsufficientLighting &&
                        e != EffectType.Scp1344 &&
                        e != EffectType.Scp1344Detected &&
                        e != EffectType.Scp956Target &&
                        e != EffectType.Scp559 &&
                        e != EffectType.Snowed &&
                        e != EffectType.SoundtrackMute &&
                        e != EffectType.SpawnProtected)
            .ToList();


        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingRandomEffect;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingRandomEffect;
            base.UnsubscribeEvents();
        }

        private void OnUsingRandomEffect(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem)) return;

            Timing.CallDelayed(2f, () =>
            {
                var random = new System.Random();
                int effectCount = random.Next(3, 6); // 3 bis 5 Effekte

                // Shuffle die Effektliste und nimm die ersten effectCount Einträge
                var selectedEffects = EffectList.OrderBy(_ => random.Next()).Take(effectCount);

                foreach (var effect in selectedEffects)
                {
                    byte intensity = (byte)random.Next(1, 255); // 1–254
                    float duration = (float)(random.NextDouble() * 20 + 10); // 10–30s

                    ev.Player.EnableEffect(effect, intensity, duration, false);
                }

                ev.Item.Destroy();
            });
        }
    }
}
