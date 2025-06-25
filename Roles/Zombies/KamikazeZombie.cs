using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Exiled.CustomRoles.API.Features;
using GockelsAIO_exiled.Abilities.Active;
using GockelsAIO_exiled.Abilities.Passive;
using PlayerRoles;

namespace GockelsAIO_exiled.Roles.NTF
{
    public class KamikazeZombie: CustomRole
    {
        public override uint Id { get; set; } = 800;
        public override int MaxHealth { get; set; } = 350;
        public override string Name { get; set; } = "049-2/B ('Burst Variante')";
        public override string Description { get; set; } = "Jagt sich selber und andere in die Luft.";
        public override string CustomInfo { get; set; } = "049-2/B ('Burst Variante')";
        public override RoleTypeId Role { get; set; } = RoleTypeId.Scp0492;
        public override float SpawnChance { get; set; } = 25;
        public override List<CustomAbility> CustomAbilities { get; set; } = new()
        {
            new Burst
            {
                Name = "test",
                Description = "test.",
            },
        };

        public override SpawnProperties SpawnProperties { get; set; }

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);
        }
    }
}
