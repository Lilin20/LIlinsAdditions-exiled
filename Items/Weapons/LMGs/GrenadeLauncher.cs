using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GockelsAIO_exiled.Items.Weapons.LMGs
{
    [CustomItem(ItemType.GunLogicer)]
    public class GrenadeLauncher : CustomWeapon
    {
        public override uint Id { get; set; } = 400;
        public override float Damage { get; set; } = 0.1f;
        public override string Name { get; set; } = "Grenade Launcher";
        public override string Description { get; set; } = "...";
        public override byte ClipSize { get; set; } = 3;
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shot += OnShotDD;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shot -= OnShotDD;
            base.UnsubscribeEvents();
        }

        private void OnShotDD(ShotEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            ev.CanHurt = false;

            Vector3 spawnPos = ev.Player.CameraTransform.position + ev.Player.CameraTransform.forward * 1.0f;
            Quaternion rotation = Quaternion.identity;

            Pickup grenade = Pickup.CreateAndSpawn(ItemType.GrenadeHE, spawnPos, rotation);

            if (grenade.Rigidbody is Rigidbody rb)
            {
                rb.isKinematic = false;
                rb.useGravity = true;

                Vector3 launchDirection = ev.Player.CameraTransform.forward + Vector3.up * 0.2f;
                launchDirection.Normalize();

                float launchForce = 20f;
                rb.velocity = launchDirection * launchForce;
            }
            else
            {
                Log.Warn("not a rigidbody (sad emoji).");
            }

            Timing.CallDelayed(4f, () =>
            {
                grenade.As<GrenadePickup>().FuseTime = 1f;
                grenade.As<GrenadePickup>().Explode();
            });
        }
    }
}
