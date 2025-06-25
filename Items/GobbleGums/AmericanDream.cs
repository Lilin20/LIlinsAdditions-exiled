using Exiled.API.Enums;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class AmericanDream : CustomItem
    {
        public override uint Id { get; set; } = 806;
        public override string Name { get; set; } = "American Dream";
        public override string Description { get; set; } = "Das gute aus Amerika.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingAmericanDream;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingAmericanDream;
            base.UnsubscribeEvents();
        }

        private void OnUsingAmericanDream(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem)) return;

            Timing.CallDelayed(2f, () =>
            {
                float random = UnityEngine.Random.value;
                if (random <= 0.33f)
                {
                    ev.Player.Role.Set(PlayerRoles.RoleTypeId.Scp0492, PlayerRoles.RoleSpawnFlags.None);
                    ev.Player.MaxHealth = 100;
                    ev.Player.Health = 100;
                    ev.Player.EnableEffect(EffectType.Slowness, 200);
                    ev.Player.EnableEffect(EffectType.Concussed, 60);
                }
                else if (random <= 0.66f)
                {
                    ev.Player.Vaporize();
                }
                else
                {
                    ev.Player.EnableEffect(EffectType.Slowness, 200);
                    ev.Player.EnableEffect(EffectType.DamageReduction, 150);
                    ev.Player.EnableEffect(EffectType.AmnesiaItems, 100);
                    ev.Player.EnableEffect(EffectType.Blinded, 90);
                }

                ev.Item.Destroy();
            });
        }
    }
}
