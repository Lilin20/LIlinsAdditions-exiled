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
        public override AttachmentName[] Attachments { get; set; } = new[]
        {
            AttachmentName.ShotgunSingleShot,
        };

        protected override void OnReloading(ReloadingWeaponEventArgs ev)
        {
            ev.IsAllowed = false;
        }
        protected override void OnShot(ShotEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            ev.CanHurt = false;
            try
            {
                if (!Physics.Raycast(ev.Player.CameraTransform.position, ev.Player.CameraTransform.forward, out RaycastHit raycastHit,
                    20, ~(1 << 1 | 1 << 13 | 1 << 16 | 1 << 28)))
                    return;

                if (raycastHit.collider is null)
                    return;

                DoorVariant dv = raycastHit.collider.gameObject.GetComponentInParent<DoorVariant>();
                if (dv is null)
                {
                    return;
                }

                var d = Door.Get(raycastHit.collider.gameObject.GetComponentInParent<DoorVariant>());

                d.As<BreakableDoor>().Break();
            }
            catch
            {
                return;
            }
        }
    }
}
