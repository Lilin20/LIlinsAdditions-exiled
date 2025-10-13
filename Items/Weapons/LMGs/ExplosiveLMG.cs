using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using UnityEngine;

namespace GockelsAIO_exiled.Items.Weapons.LMGs
{
    [CustomItem(ItemType.GunLogicer)]
    public class ExplosiveLMG : CustomWeapon
    {
        public override uint Id { get; set; } = 401;
        public override float Damage { get; set; } = 0.1f;
        public override string Name { get; set; } = "Prototype LMG - Nano Rockets";
        public override string Description { get; set; } = "Shoots nano rockets.";
        public override byte ClipSize { get; set; } = 200;
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shot += OnShotDD;
            Exiled.Events.Handlers.Player.Hurting += PlayerHurting;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shot -= OnShotDD;
            Exiled.Events.Handlers.Player.Hurting -= PlayerHurting;
            base.UnsubscribeEvents();
        }

        public void PlayerHurting(HurtingEventArgs ev)
        {
            if (ev.Player is null) return;
            if (ev.Attacker is null) return;
            if (!Check(ev.Attacker.CurrentItem)) return;

            if (ev.DamageHandler.Type == DamageType.Logicer)
            {
                ev.Player.ShowHitMarker();
                ev.Amount = 0f;
            }

            ev.IsAllowed = false;

            base.OnHurting(ev);
        }

        private void OnShotDD(ShotEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            ExplosiveGrenade grenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE, ev.Player);
            grenade.Projectile.Base._playerDamageOverDistance = grenade.Projectile.Base._playerDamageOverDistance.Multiply(0.01f);
            grenade.Projectile.Base._burnedDuration = 0;
            grenade.Projectile.Base._concussedDuration = 0;
            grenade.Projectile.Base._deafenedDuration = 0;
            grenade.Projectile.Base._shakeOverDistance = grenade.Projectile.Base._shakeOverDistance.Multiply(0f);
            grenade.Projectile.Base._doorDamageOverDistance = grenade.Projectile.Base._doorDamageOverDistance.Multiply(0f);
            grenade.Projectile.Base._effectDurationOverDistance = grenade.Projectile.Base._effectDurationOverDistance.Multiply(0f);
            
            grenade.FuseTime = 0.01f;
            
            grenade.SpawnActive(ev.Position + (Vector3.up * 0.1f));
        }
    }
}
