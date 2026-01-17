using System.Collections.Generic;
using Exiled.API.Features.Spawn;
using Exiled.CustomRoles.API.Features;
using LilinsAdditions.CustomRoles.Abilities.Active;
using PlayerRoles;
using VVUP.CustomRoles.Abilities.Passive;
using VVUP.CustomRoles.API;

namespace LilinsAdditions.CustomRoles.Roles.NTF
{
    public class RiotOperator: CustomRole, ICustomRole
    {
        public override uint Id { get; set; } = 100;
        public StartTeam StartTeam { get; set; } = StartTeam.Ntf;
        public int Chance { get; set; } = 15;
        public override int MaxHealth { get; set; } = 200;
        public override string Name { get; set; } = "MTF Nu-7";
        public override string Description { get; set; } = "A specially equipped unit. Comes equipped with a riot shield.";
        public override string CustomInfo { get; set; } = "MTF Nu-7";
        public override RoleTypeId Role { get; set; } = RoleTypeId.NtfCaptain;
        public override List<CustomAbility> CustomAbilities { get; set; } = new()
        {
            new RiotShield
            {
                Name = "Riot Shield",
                Description = "Toggles your Riot Shield.",
            },
            new RestrictedItems
            {
                Name = "Restricted Items",
                Description = "Restricts certain items from being used.",
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
    }
}
