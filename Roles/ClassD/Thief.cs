using Exiled.API.Features.Spawn;
using Exiled.CustomRoles.API.Features;
using GockelsAIO_exiled.Abilities.Active;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GockelsAIO_exiled.Roles.ClassD
{
    public class Thief : CustomRole
    {
        public override uint Id { get; set; } = 203;
        public override int MaxHealth { get; set; } = 100;
        public override string Name { get; set; } = "Class-D - Thief";
        public override string Description { get; set; } = "You can steal from players with your ability..";
        public override string CustomInfo { get; set; } = "Class-D Personnel";
        public override RoleTypeId Role { get; set; } = RoleTypeId.ClassD;
        public override float SpawnChance { get; set; } = 20;
        public override List<CustomAbility> CustomAbilities { get; set; } = new List<CustomAbility>
        {
            new Pickpocket()
            {
                Name = "Pickpocket [Active]",
                Description = "Stay near other players and look at them.",
            }
        };
        public override SpawnProperties SpawnProperties { get; set; } = new()
        {
            Limit = 1,
            RoleSpawnPoints =
            [
                new()
                {
                    Role = RoleTypeId.ClassD,
                }
            ]
        };
    }
}
