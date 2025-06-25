using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Exiled.CustomRoles.API.Features;
using GockelsAIO_exiled.Abilities.Active;
using GockelsAIO_exiled.Abilities.Passive;
using PlayerRoles;

namespace GockelsAIO_exiled.Roles.NTF
{
    public class RiotOperator: CustomRole
    {
        public override uint Id { get; set; } = 100;
        public override int MaxHealth { get; set; } = 200;
        public override string Name { get; set; } = "MTF Nu-7";
        public override string Description { get; set; } = "Eine speziell ausgerüstete Einheit für größere Einsätze. Kommt mit einem Einsatzschild.";
        public override string CustomInfo { get; set; } = "MTF Nu-7";
        public override RoleTypeId Role { get; set; } = RoleTypeId.NtfCaptain;
        public override float SpawnChance { get; set; } = 25;
        public override List<CustomAbility> CustomAbilities { get; set; } = new()
        {
            new RiotShield
            {
                Name = "Riot Shield",
                Description = "Toggled dein Riot Shield.",
            },
            new RestrictedItems
            {
                Name = "Restricted Items",
                Description = "Managed nicht benutzbare Items",
                RestrictedItemList =
                {
                    ItemType.Jailbird,
                    ItemType.MicroHID,
                    ItemType.ParticleDisruptor,
                },
                RestrictPickingUpItems = true,
                RestrictUsingItems = false,
                RestrictDroppingItems = false,
            }
        };

        public override SpawnProperties SpawnProperties { get; set; } = new()
        {
            Limit = 1,
            RoleSpawnPoints =
            [
                new()
                {
                    Role = RoleTypeId.NtfCaptain,
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
