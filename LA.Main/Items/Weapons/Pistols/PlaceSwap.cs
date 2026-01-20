using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace LilinsAdditions.Items.Weapons.Pistols
{
    [CustomItem(ItemType.GunCOM15)]
    public class PlaceSwap : CustomWeapon
    {
        private const float SWAP_DELAY = 0.25f;

        public override uint Id { get; set; } = 200;
        public override float Damage { get; set; } = 1;
        public override string Name { get; set; } = "Entity Swapper";
        public override string Description { get; set; } = "Swap positions with your target.";
        public override byte ClipSize { get; set; } = 5;
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void OnReloading(ReloadingWeaponEventArgs ev)
        {
            ev.IsAllowed = false;
            base.OnReloading(ev);
        }

        protected override void OnShot(ShotEventArgs ev)
        {
            ev.CanHurt = false;

            if (!IsValidSwapTarget(ev.Target, ev.Player))
                return;

            var targetPosition = ev.Target.Position;
            var shooterPosition = ev.Player.Position;

            Timing.CallDelayed(SWAP_DELAY, () => ExecutePositionSwap(ev.Player, ev.Target, shooterPosition, targetPosition));
        }

        private static bool IsValidSwapTarget(Player target, Player shooter)
        {
            if (target == null)
            {
                Log.Debug($"[PlaceSwap] {shooter.Nickname} shot missed - no target");
                return false;
            }

            if (target == shooter)
            {
                Log.Debug($"[PlaceSwap] {shooter.Nickname} cannot swap with self");
                return false;
            }

            if (!target.IsAlive)
            {
                Log.Debug($"[PlaceSwap] {shooter.Nickname} cannot swap with dead target: {target.Nickname}");
                return false;
            }

            return true;
        }

        private static void ExecutePositionSwap(Player shooter, Player target, Vector3 shooterPosition, Vector3 targetPosition)
        {
            if (!ValidatePlayersForSwap(shooter, target))
                return;

            target.Position = shooterPosition;
            shooter.Position = targetPosition;

            Log.Debug($"[PlaceSwap] Swapped positions: {shooter.Nickname} <-> {target.Nickname}");
        }

        private static bool ValidatePlayersForSwap(Player shooter, Player target)
        {
            if (shooter == null || !shooter.IsAlive)
            {
                Log.Debug($"[PlaceSwap] Shooter no longer valid for swap");
                return false;
            }

            if (target == null || !target.IsAlive)
            {
                Log.Debug($"[PlaceSwap] Target no longer valid for swap");
                return false;
            }

            return true;
        }
    }
}