using System.Collections.Generic;
using GockelsAIO_exiled.Abilities.Active;
using Exiled.API.Features.Spawn;
using Exiled.CustomRoles.API.Features;
using PlayerRoles;

namespace GockelsAIO_exiled.Roles.ClassD
{
    public class Lockpicker : CustomRole
    {
        public override uint Id { get; set; } = 200;
        public override int MaxHealth { get; set; } = 100;
        public override string Name { get; set; } = "Class-D - Lockpicker";
        public override string Description { get; set; } = "You can lockpick doors.";
        public override string CustomInfo { get; set; } = "Class-D - Lockpicker";
        public override RoleTypeId Role { get; set; } = RoleTypeId.ClassD;
        public int Chance { get; set; } = 20;
        public override List<CustomAbility> CustomAbilities { get; set; } = new()
        {
            new DoorPicking
            {
                Name = "Lockpicking Ability [Active]",
                Description = "Allows you to open any door for a short period of time.",
            }
        };
        public override SpawnProperties SpawnProperties { get; set; } = new()
        {
            Limit = 2,
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
