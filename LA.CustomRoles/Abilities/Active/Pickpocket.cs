using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.CustomRoles.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LilinsAdditions.CustomRoles.Abilities.Active
{
    [CustomAbility]
    public class Pickpocket : ActiveAbility
    {
        public override string Name { get; set; } = "Pickpocket Ability";
        public override string Description { get; set; } = "Steals a random item from a players inventory.";
        public override float Duration { get; set; } = 1f;
        public override float Cooldown { get; set; } = 15f;

        protected override void AbilityUsed(Player player)
        {
            if (!Physics.Raycast(player.CameraTransform.position, player.CameraTransform.forward, out RaycastHit raycastHit,
                       3, ~(1 << 1 | 1 << 13 | 1 << 16 | 1 << 28)))
                return;

            if (raycastHit.collider is null)
                return;

            try
            {
                Player robbedPlayer = Player.Get(raycastHit.transform.GetComponentInParent<ReferenceHub>());
                if (robbedPlayer != null && robbedPlayer.Role.Team != PlayerRoles.Team.SCPs)
                {
                    if (robbedPlayer.Items.Count <= 0)
                    {
                        player.ShowHint("This player doesnt have any items.");
                    }
                    Item randomItem = robbedPlayer.Items.ToList().RandomItem();
                    player.AddItem(randomItem.Type);

                    robbedPlayer.RemoveItem(randomItem);
                    player.ShowHint($"You stole something...");
                }
            }
            catch (Exception e)
            {
                Log.Debug(e);
            }
        }
    }
}
