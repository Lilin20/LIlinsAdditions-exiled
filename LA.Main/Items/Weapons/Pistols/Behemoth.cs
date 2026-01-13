using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace GockelsAIO_exiled.Items.Weapons.Pistols
{
    [CustomItem(ItemType.GunRevolver)]
    public class Behemoth : CustomWeapon
    {
        private const float DEFAULT_DAMAGING_DISTANCE = 100f;
        private const float DEFAULT_DAMAGE_OVERRIDE = 80f;
        private const float DAMAGE_DELAY = 0.25f;
        private const int PLAYER_LAYER = 13;

        public override uint Id { get; set; } = 203;
        public override string Name { get; set; } = "Behemoth 50.cal";
        public override string Description { get; set; } = "A very deadly revolver. Can shoot through walls.";
        public override byte ClipSize { get; set; } = 6;
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        public float DamagingDistance { get; set; } = DEFAULT_DAMAGING_DISTANCE;
        public float DamageOverride { get; set; } = DEFAULT_DAMAGE_OVERRIDE;

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shooting += OnShootingPlayer;
            Exiled.Events.Handlers.Player.Hurting += OnHurtingPlayer;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shooting -= OnShootingPlayer;
            Exiled.Events.Handlers.Player.Hurting -= OnHurtingPlayer;
            base.UnsubscribeEvents();
        }

        private void OnHurtingPlayer(HurtingEventArgs ev)
        {
            if (ev.Player == null || ev.Attacker == null)
                return;

            if (!Check(ev.Attacker.CurrentItem))
                return;
            
            ev.Amount = 0f;
            ev.IsAllowed = false;

            base.OnHurting(ev);
        }

        private void OnShootingPlayer(ShootingEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            var target = FindTargetThroughWalls(ev.Player);
            if (target != null)
            {
                ev.Player.ShowHitMarker();
                Timing.CallDelayed(DAMAGE_DELAY, () => ApplyDamageToTarget(target, ev.Player));
            }
        }

        private Player FindTargetThroughWalls(Player shooter)
        {
            var origin = shooter.CameraTransform.position;
            var direction = shooter.CameraTransform.forward;
            var playerLayerMask = 1 << PLAYER_LAYER;

            var hits = Physics.RaycastAll(origin, direction, DamagingDistance, playerLayerMask);

            foreach (var hit in hits)
            {
                try
                {
                    var target = Player.Get(hit.collider);
                    if (IsValidTarget(target, shooter))
                    {
                        Log.Debug($"[Behemoth] {shooter.Nickname} hit {target.Nickname} through walls");
                        return target;
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Warn($"[Behemoth] Error processing raycast hit: {ex.Message}");
                }
            }

            return null;
        }

        private static bool IsValidTarget(Player target, Player shooter)
        {
            return target != null 
                   && target.IsAlive 
                   && target != shooter;
        }

        private void ApplyDamageToTarget(Player target, Player shooter)
        {
            if (target == null || !target.IsAlive)
            {
                Log.Debug($"[Behemoth] Target no longer valid for damage from {shooter.Nickname}");
                return;
            }

            target.Hurt(DamageOverride);
            Log.Debug($"[Behemoth] {shooter.Nickname} dealt {DamageOverride} damage to {target.Nickname}");
        }
    }
}