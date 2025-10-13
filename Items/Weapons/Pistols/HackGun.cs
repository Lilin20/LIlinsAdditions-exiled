using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public override uint Id { get; set; } = 201;
        public override string Name { get; set; } = "Nullifier";
        public override string Description { get; set; } = "A tool gun which locks doors when shot. Also causes a blackout on generators.";
        public override float Damage { get; set; } = 1;
        public override byte ClipSize { get; set; } = 3;
        public override float Weight { get; set; } = 0.1f;

        public override SpawnProperties SpawnProperties { get; set; }

        protected override void OnShot(ShotEventArgs ev)
        {
            try
            {
                if (Check(ev.Player.CurrentItem))
                {
                    if (!Physics.Raycast(ev.Player.CameraTransform.position, ev.Player.CameraTransform.forward, out RaycastHit raycastHit,
                       20, ~(1 << 1 | 1 << 13 | 1 << 16 | 1 << 28)))
                        return;

                    if (raycastHit.collider is null)
                        return;

                    //if (!Exiled.API.Features.Camera.TryGet(raycastHit.collider.gameObject.GetComponentInParent<Scp079Camera>(), out Exiled.API.Features.Camera hit))
                    //    return;

                    DoorVariant dv = raycastHit.collider.gameObject.GetComponentInParent<DoorVariant>();
                    if (dv is null)
                    {
                        Scp079Generator generator = raycastHit.collider.gameObject?.GetComponentInParent<Scp079Generator>();
                        if (generator != null)
                        {
                            Cassie.MessageTranslated("GENERATOR DAMAGE DETECTED . REPAIRING GENERATOR", "Generator Malfunction", false, true, true);
                            Map.TurnOffAllLights(10f);
                        }
                    }

                    Door door = Door.Get(dv);

                    if (door.IsLocked)
                    {
                        door.Unlock();
                    }
                    else
                    {
                        door.Lock(5, DoorLockType.Lockdown2176);
                    }
                }
            }
            catch
            {
                return;
            }
        }
    }
}
