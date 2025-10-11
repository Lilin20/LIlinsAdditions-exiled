using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomRoles.API.Features;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using UnityEngine;

namespace GockelsAIO_exiled.Abilities.Active
{
    [CustomAbility]
    public class RiotShield : ActiveAbility
    {
        public override string Name { get; set; } = "Riot Shield";
        public override string Description { get; set; } = "Toggles the riot shield.";
        public override float Duration { get; set; } = 1f;
        public override float Cooldown { get; set; } = 2f;
        public static readonly Dictionary<Player, SchematicObject> activeShields = new();
        protected override void AbilityAdded(Player player)
        {
            SelectAbility(player);
            base.AbilityAdded(player);
        }

        protected override void AbilityRemoved(Player player)
        {
            if (activeShields.TryGetValue(player, out var shield))
            {
                shield.Destroy();
                activeShields.Remove(player);
            }

            base.AbilityRemoved(player);
        }

        protected override void AbilityUsed(Player player)
        {
            if (activeShields.TryGetValue(player, out var existingShield))
            {
                existingShield.Destroy();
                activeShields.Remove(player);
                player.DisableEffect(Exiled.API.Enums.EffectType.Stained);
            }
            else
            {
                if (ObjectSpawner.TrySpawnSchematic("RiotShield", player.Position, player.Rotation, out var schematic))
                {
                    schematic.transform.parent = player.Transform;

                    Vector3 relativeOffset = new Vector3(0f, -1f, 0.5f);
                    schematic.transform.localPosition = relativeOffset;
                    schematic.transform.localRotation = Quaternion.identity;

                    player.EnableEffect(Exiled.API.Enums.EffectType.Stained);

                    activeShields[player] = schematic;
                }
            }

            base.AbilityUsed(player);
        }
    }
}
