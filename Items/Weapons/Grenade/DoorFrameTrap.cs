using Exiled.API.Features.Attributes;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.API.Features.Toys;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GockelsAIO_exiled.Items.Weapons.Grenade
{
    [CustomItem(ItemType.GrenadeHE)]
    public class DoorFrameGrenade : CustomItem
    {
        public override uint Id { get; set; } = 1002;
        public override string Name { get; set; } = "Door Frame Grenade";
        public override string Description { get; set; } = "test";
        public override float Weight { get; set; } = 1.15f;
        public override ItemType Type { get; set; } = ItemType.GrenadeHE;
        public override SpawnProperties? SpawnProperties { get; set; } = null;

        protected override void SubscribeEvents()
        {
            base.SubscribeEvents();
        }
    }
}
