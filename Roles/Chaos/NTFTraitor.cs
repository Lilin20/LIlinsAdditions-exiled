using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Exiled.CustomRoles.API.Features;
using GockelsAIO_exiled.Abilities.Active;
using GockelsAIO_exiled.Abilities.Passive;
using PlayerRoles;
using Respawning.Waves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GockelsAIO_exiled.Roles.Chaos
{
    public class NTFTraitor : CustomRole
    {
        public override uint Id { get; set; } = 300;
        public override int MaxHealth { get; set; } = 150;
        public override string Name { get; set; } = "NINE-TAILED FOX SERGEANT ";
        public override string Description { get; set; } = "Du bist ein Verräter. Töte die Facility Forces!";
        public override string CustomInfo { get; set; }
        public override RoleTypeId Role { get; set; } = RoleTypeId.ChaosRifleman;
        public override float SpawnChance { get; set; } = 25;
        public override List<CustomAbility> CustomAbilities { get; set; }

        public override SpawnProperties SpawnProperties { get; set; } = new()
        {
            Limit = 1,
            RoleSpawnPoints =
            [
                new()
                {
                    Role = RoleTypeId.NtfSergeant,
                }
            ]
        };

        public override List<string> Inventory { get; set; } = new()
        {
            ItemType.KeycardMTFCaptain.ToString(),
            ItemType.GunCOM15.ToString(),
            ItemType.Medkit.ToString(),
            ItemType.ArmorHeavy.ToString(),
            ItemType.Ammo9x19.ToString(),
            ItemType.Ammo9x19.ToString(),
            ItemType.Ammo9x19.ToString(),
            ItemType.Ammo9x19.ToString(),
        };

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);
        }
    }
}
