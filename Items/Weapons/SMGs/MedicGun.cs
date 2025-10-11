using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Player = Exiled.Events.Handlers.Player;
using Light = Exiled.API.Features.Toys.Light;
using UnityEngine;
using Exiled.API.Features;

namespace GockelsAIO_exiled.Items.Weapons.SMGs
{
    [CustomItem(ItemType.GunCrossvec)]
    public class MedicGun : CustomWeapon
    {
        public override uint Id { get; set; } = 500;
        public override string Name { get; set; } = "MS9K - MedShot 9000";
        public override string Description { get; set; } = "A tool to heal entities.";
        public override byte ClipSize { get; set; } = 60;
        public override float Weight { get; set; } = 0.5f;
        public override float Damage { get; set; } = 0f;


        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Player.Shot += OnMedicShot;
            Player.Hurting += PlayerHurting;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Player.Shot -= OnMedicShot;
            Player.Hurting -= PlayerHurting;
            base.UnsubscribeEvents();
        }

        public void PlayerHurting(HurtingEventArgs ev)
        {
            if (ev.Player is null) return;
            if (ev.Attacker is null) return;
            if (!Check(ev.Attacker.CurrentItem)) return;

            ev.Amount = 0f;
            ev.IsAllowed = false;

            base.OnHurting(ev);
        }

        public void OnMedicShot(ShotEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem)) return;
            if (ev.Target is null) return;

            ev.CanHurt = false;

            if (ev.Player.Role.Team != Team.SCPs)
            {
                ev.Target.Heal(5);
            }
            else if (ev.Target.Role == RoleTypeId.Scp0492)
            {
                ev.Target.HumeShield += 20;

                if (ev.Target.HumeShield >= 300)
                {
                    ev.Target.Role.Set(RoleTypeId.ClassD, RoleSpawnFlags.None);
                }
            }
            else
            {
                return;
            }
        }
    }
}
