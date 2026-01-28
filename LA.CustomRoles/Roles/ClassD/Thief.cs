using Exiled.API.Features.Spawn;
using Exiled.CustomRoles.API.Features;
using LilinsAdditions.CustomRoles.Abilities.Active;
using PlayerRoles;
using System.Collections.Generic;
using VVUP.CustomRoles.API;
using YamlDotNet.Serialization;

namespace LilinsAdditions.CustomRoles.Roles.ClassD
{
    public class Thief : CustomRole, ICustomRole
    {
        public override uint Id { get; set; } = 203;
        public StartTeam StartTeam { get; set; } = StartTeam.ClassD;
        public int Chance { get; set; } = 50;
        public override int MaxHealth { get; set; } = 100;
        public override string Name { get; set; } = "Class-D - Thief";
        public override string Description { get; set; } = "You can steal from players with your ability..";
        public override string CustomInfo { get; set; } = "Class-D Personnel";
        public override RoleTypeId Role { get; set; } = RoleTypeId.ClassD;
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
            Limit = 1
        };
        
        [YamlIgnore] 
        public override float SpawnChance { get; set; } = 0;
        [YamlIgnore] 
        public override bool IgnoreSpawnSystem { get; set; } = true;
    }
}
