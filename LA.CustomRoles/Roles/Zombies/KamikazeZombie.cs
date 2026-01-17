using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Exiled.CustomRoles.API.Features;
using LilinsAdditions.CustomRoles.Abilities.Active;
using PlayerRoles;
using System.Collections.Generic;
using VVUP.CustomRoles.API;

namespace LilinsAdditions.CustomRoles.Roles.Zombies
{
    public class KamikazeZombie: CustomRole, ICustomRole
    {
        public override uint Id { get; set; } = 800;
        public StartTeam StartTeam { get; set; } = StartTeam.Scp | StartTeam.Revived;
        public int Chance { get; set; } = 15;
        public override int MaxHealth { get; set; } = 350;
        public override string Name { get; set; } = "049-2/B ('Burst Variant')";
        public override string Description { get; set; } = "Blow yourself and others up.";
        public override string CustomInfo { get; set; } = "049-2/B ('Burst Variant')";
        public override RoleTypeId Role { get; set; } = RoleTypeId.Scp0492;
        public override List<CustomAbility> CustomAbilities { get; set; } = new()
        {
            new Burst
            {
                Name = "Burst",
                Description = "Make a chemical cocktail in your body and explode.",
            },
        };

        public override SpawnProperties SpawnProperties { get; set; } = new()
        {
            Limit = 1,
        };
    }
}
