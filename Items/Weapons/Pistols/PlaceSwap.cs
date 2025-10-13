using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Enums;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace GockelsAIO_exiled.Items.Weapons.Pistols
{
    [CustomItem(ItemType.GunCOM15)]
    public class PlaceSwap : CustomWeapon
    {
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
        }
        protected override void OnShot(ShotEventArgs ev)
        {
            ev.CanHurt = false;

            if (!Check(ev.Player.CurrentItem))
                return;

            if (ev.Target == null)
                return;

            if (ev.Target == ev.Player)
                return;

            Vector3 targetPosition = ev.Target.Position;
            Vector3 shooterPosition = ev.Player.Position;

            Timing.CallDelayed(0.25f, () =>
            {
                ev.Target.Position = shooterPosition;
                ev.Player.Position = targetPosition;
            });

        }
    }
}
