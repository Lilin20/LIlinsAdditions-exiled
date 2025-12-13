using Exiled.API.Features.Attributes;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms.Attachments;
using UnityEngine;

namespace GockelsAIO_exiled.Items.Weapons.Shotguns
{
    [CustomItem(ItemType.GunShotgun)]
    public class BreachShotgun : CustomWeapon
    {
        public override uint Id { get; set; } = 300;
        public override string Name { get; set; } = "Kerberos-12";
        public override string Description { get; set; } = "Destroys doors. Deals no damage to players.";
        public override float Damage { get; set; } = 0;
        public override byte ClipSize { get; set; } = 20;
        public override float Weight { get; set; } = 1.5f;
        public override SpawnProperties SpawnProperties { get; set; }
        public override AttachmentName[] Attachments { get; set; } = new[] { AttachmentName.ShotgunSingleShot };
        
        private const float MaxRaycastDistance = 20f;
        private const int RaycastMask = ~(1 << 1 | 1 << 13 | 1 << 16 | 1 << 28);

        protected override void OnReloading(ReloadingWeaponEventArgs ev)
        {
            ev.IsAllowed = false;
        }

        protected override void OnShot(ShotEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            ev.CanHurt = false;

            if (TryGetTargetDoor(ev.Player, out Door door))
            {
                BreakDoor(door);
            }
        }

        private bool TryGetTargetDoor(Exiled.API.Features.Player player, out Door door)
        {
            door = null;

            if (!Physics.Raycast(
                player.CameraTransform.position,
                player.CameraTransform.forward,
                out RaycastHit hit,
                MaxRaycastDistance,
                RaycastMask))
            {
                return false;
            }

            if (hit.collider == null)
                return false;

            DoorVariant doorVariant = hit.collider.gameObject.GetComponentInParent<DoorVariant>();
            if (doorVariant == null)
                return false;

            door = Door.Get(doorVariant);
            return door != null;
        }

        private void BreakDoor(Door door)
        {
            if (door is BreakableDoor breakableDoor)
            {
                breakableDoor.Break();
            }
        }
    }
}