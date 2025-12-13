using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Interactables.Interobjects.DoorUtils;
using MapGeneration.Distributors;
using UnityEngine;

namespace GockelsAIO_exiled.Items.Weapons.Pistols
{
    [CustomItem(ItemType.GunCOM15)]
    public class HackGun : CustomWeapon
    {
        private const float RAYCAST_DISTANCE = 20f;
        private const float DOOR_LOCK_DURATION = 5f;
        private const float BLACKOUT_DURATION = 10f;
        private const string GENERATOR_CASSIE_MESSAGE = "GENERATOR DAMAGE DETECTED . REPAIRING GENERATOR";
        private const string GENERATOR_CASSIE_SUBTITLE = "Generator Malfunction";
        
        private const int RAYCAST_LAYER_MASK = ~(1 << 1 | 1 << 13 | 1 << 16 | 1 << 28);

        public override uint Id { get; set; } = 201;
        public override string Name { get; set; } = "Nullifier";
        public override string Description { get; set; } = "A tool gun which locks doors when shot. Also causes a blackout on generators.";
        public override float Damage { get; set; } = 1;
        public override byte ClipSize { get; set; } = 3;
        public override float Weight { get; set; } = 0.1f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void OnShot(ShotEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            try
            {
                var hit = PerformRaycast(ev.Player);
                if (!hit.HasValue)
                    return;

                ProcessHit(hit.Value, ev.Player);
            }
            catch (System.Exception ex)
            {
                Log.Error($"[HackGun] Error processing shot from {ev.Player.Nickname}: {ex}");
            }
        }

        private static RaycastHit? PerformRaycast(Player player)
        {
            var origin = player.CameraTransform.position;
            var direction = player.CameraTransform.forward;

            if (!Physics.Raycast(origin, direction, out var hit, RAYCAST_DISTANCE, RAYCAST_LAYER_MASK))
                return null;

            if (hit.collider == null)
                return null;

            return hit;
        }

        private static void ProcessHit(RaycastHit hit, Player shooter)
        {
            var doorVariant = hit.collider.gameObject.GetComponentInParent<DoorVariant>();
            if (doorVariant != null)
            {
                ToggleDoorLock(doorVariant, shooter);
                return;
            }
            
            var generator = hit.collider.gameObject.GetComponentInParent<Scp079Generator>();
            if (generator != null)
            {
                TriggerGeneratorBlackout(shooter);
                return;
            }

            Log.Debug($"[HackGun] {shooter.Nickname} hit non-interactive object: {hit.collider.gameObject.name}");
        }

        private static void ToggleDoorLock(DoorVariant doorVariant, Player shooter)
        {
            var door = Door.Get(doorVariant);
            if (door == null)
            {
                Log.Warn($"[HackGun] Failed to get Door from DoorVariant for {shooter.Nickname}");
                return;
            }

            if (door.IsLocked)
            {
                door.Unlock();
                Log.Debug($"[HackGun] {shooter.Nickname} unlocked door: {door.Type}");
            }
            else
            {
                door.Lock(DOOR_LOCK_DURATION, DoorLockType.Lockdown2176);
                Log.Debug($"[HackGun] {shooter.Nickname} locked door: {door.Type} for {DOOR_LOCK_DURATION}s");
            }
        }

        private static void TriggerGeneratorBlackout(Player shooter)
        {
            Cassie.MessageTranslated(
                GENERATOR_CASSIE_MESSAGE,
                GENERATOR_CASSIE_SUBTITLE,
                isHeld: false,
                isNoisy: true,
                isSubtitles: true
            );

            Map.TurnOffAllLights(BLACKOUT_DURATION);

            Log.Debug($"[HackGun] {shooter.Nickname} triggered generator blackout for {BLACKOUT_DURATION}s");
        }
    }
}