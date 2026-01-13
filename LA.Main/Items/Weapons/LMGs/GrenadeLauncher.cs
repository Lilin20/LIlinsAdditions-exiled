
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace GockelsAIO_exiled.Items.Weapons.LMGs
{
    [CustomItem(ItemType.GunLogicer)]
    public class GrenadeLauncher : CustomWeapon
    {
        private const float SPAWN_DISTANCE = 1.0f;
        private const float UPWARD_ARC = 0.2f;
        private const float LAUNCH_FORCE = 20f;
        private const float FLIGHT_TIME = 4f;
        private const float EXPLODE_FUSE_TIME = 1f;

        public override uint Id { get; set; } = 400;
        public override float Damage { get; set; } = 0.1f;
        public override string Name { get; set; } = "Grenade Launcher";
        public override string Description { get; set; } = "Shoots grenades.";
        public override byte ClipSize { get; set; } = 3;
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shot += OnShotPlayer;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shot -= OnShotPlayer;
            base.UnsubscribeEvents();
        }

        protected override void OnReloading(ReloadingWeaponEventArgs ev)
        {
            ev.IsAllowed = false;
            base.OnReloading(ev);
        }

        private void OnShotPlayer(ShotEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            ev.CanHurt = false;

            LaunchGrenade(ev.Player);
        }

        private static void LaunchGrenade(Player player)
        {
            var spawnPosition = CalculateSpawnPosition(player);
            var grenade = Pickup.CreateAndSpawn(ItemType.GrenadeHE, spawnPosition, Quaternion.identity);

            if (!ApplyPhysics(grenade, player))
            {
                Log.Error($"[GrenadeLauncher] Failed to apply physics to grenade for {player.Nickname}");
                grenade.Destroy();
                return;
            }

            Timing.CallDelayed(FLIGHT_TIME, () => ExplodeGrenade(grenade, player));
            Log.Debug($"[GrenadeLauncher] {player.Nickname} launched grenade");
        }

        private static Vector3 CalculateSpawnPosition(Player player)
        {
            return player.CameraTransform.position + (player.CameraTransform.forward * SPAWN_DISTANCE);
        }

        private static bool ApplyPhysics(Pickup grenade, Player player)
        {
            if (grenade?.Rigidbody is not Rigidbody rb)
                return false;

            rb.isKinematic = false;
            rb.useGravity = true;

            var launchDirection = CalculateLaunchDirection(player);
            rb.velocity = launchDirection * LAUNCH_FORCE;

            return true;
        }

        private static Vector3 CalculateLaunchDirection(Player player)
        {
            var direction = player.CameraTransform.forward + (Vector3.up * UPWARD_ARC);
            return direction.normalized;
        }

        private static void ExplodeGrenade(Pickup grenade, Player player)
        {
            if (grenade == null)
            {
                Log.Debug($"[GrenadeLauncher] Grenade from {player.Nickname} was destroyed before explosion");
                return;
            }

            if (grenade is not GrenadePickup grenadePickup)
            {
                Log.Error($"[GrenadeLauncher] Pickup is not a GrenadePickup");
                grenade.Destroy();
                return;
            }

            grenadePickup.FuseTime = EXPLODE_FUSE_TIME;
            grenadePickup.Explode();

            Log.Debug($"[GrenadeLauncher] Grenade from {player.Nickname} exploded");
        }
    }
}