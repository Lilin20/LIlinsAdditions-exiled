using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class PeelAndRun : CustomItem
    {
        public override uint Id { get; set; } = 807;
        public override string Name { get; set; } = "Peel And Run";
        public override string Description { get; set; } = "Drop your skin and run. Has a chance to kill you.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingPeelAndRun;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingPeelAndRun;
            base.UnsubscribeEvents();
        }

        private void OnUsingPeelAndRun(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem)) return;

            Timing.CallDelayed(2f, () =>
            {
                if (ev.Item.Type == ItemType.Adrenaline)
                {
                    ev.IsAllowed = false;
                    ev.Item.Destroy();
                }

                float random = UnityEngine.Random.value;
                if (random <= 0.25f)
                {
                    ev.Player.EnableEffect(EffectType.CardiacArrest, 1, 100f, false);
                    ev.Player.EnableEffect(EffectType.AmnesiaItems, 1, 100f, false);
                    ev.Player.EnableEffect(EffectType.Bleeding, 1, 100f, false);
                    ev.Player.EnableEffect(EffectType.Poisoned, 1, 100f, false);
                    ev.Player.EnableEffect(EffectType.Hemorrhage, 1, 100f, false);
                    ev.Player.EnableEffect(EffectType.SeveredHands, 1, 100f, false);
                }
                else
                {
                    ev.Player.EnableEffect(EffectType.Invisible, 1, 15f, false);
                    ev.Player.EnableEffect(EffectType.MovementBoost, 50, 15f, false);
                    ev.Player.EnableEffect(EffectType.Flashed, 1, 1f, false);
                    ev.Player.EnableEffect(EffectType.SilentWalk, 10, 15f, false);
                    ev.Player.Health = 25;
                    ev.Player.MaxHealth = 25;
                    Ragdoll ragdoll = Ragdoll.CreateAndSpawn(ev.Player.Role, ev.Player.DisplayNickname, "Es sieht aus wie eine leblose Hülle.", ev.Player.Position, ev.Player.ReferenceHub.PlayerCameraReference.rotation);
                }

                ev.Item.Destroy();
            });
        }
    }
}
