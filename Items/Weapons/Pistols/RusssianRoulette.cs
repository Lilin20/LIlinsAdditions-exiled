using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.Handlers;
using MEC;
using PlayerStatsSystem;

namespace GockelsAIO_exiled.Items.Weapons.Pistols
{
    [CustomItem(ItemType.GunRevolver)]
    public class RussianRoulette : CustomWeapon
    {
        public override uint Id { get; set; } = 202;
        public override string Name { get; set; } = "Russian Roulette";
        public override string Description { get; set; } = "Let the fate decide.";
        public override byte ClipSize { get; set; } = 6;
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Player.Shot += OnShotDD;
            Player.Hurting += PlayerHurting;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Player.Shot -= OnShotDD;
            Player.Hurting -= PlayerHurting;
            base.UnsubscribeEvents();
        }
        public void PlayerHurting(HurtingEventArgs ev)
        {
            if (ev.Player is null) return;
            if (ev.Attacker is null) return;
            if (!Check(ev.Attacker.CurrentItem)) return;

            ev.Amount = 0f;
            ev.IsAllowed = false;

            base.OnHurting(ev);
        }

        private void OnShotDD(ShotEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            if (ev.Target == null)
                return;

            ev.CanHurt = false;

            float random = UnityEngine.Random.value;


            Timing.CallDelayed(0.5f, () =>
            {
                if (ev.Target.Role.Team != PlayerRoles.Team.SCPs)
                {
                    if (random <= 0.65f)
                    {
                        ev.Target.Hurt(new UniversalDamageHandler(-1f, DeathTranslations.Unknown));
                        ev.Player.AddItem(ItemType.Coin);
                    }
                    else
                    {
                        ev.Player.Hurt(new UniversalDamageHandler(-1f, DeathTranslations.Unknown));
                    }
                }
                else if (ev.Target.Role.Team == PlayerRoles.Team.SCPs)
                {
                    if (random <= 0.05)
                    {
                        ev.Target.Hurt(new UniversalDamageHandler(-1f, DeathTranslations.Unknown));
                        ev.Player.AddItem(ItemType.Coin);
                    }
                    else
                    {
                        ev.Player.Hurt(new UniversalDamageHandler(-1f, DeathTranslations.Unknown));
                    }
                }
            });
        }
    }
}
