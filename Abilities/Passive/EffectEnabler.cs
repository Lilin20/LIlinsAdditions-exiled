using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using MEC;

namespace GockelsAIO_exiled.Abilities.Passive
{
    public class EffectEnabler : PassiveAbility
    {
        public override string Name { get; set; } = "Effect Giver";
        public override string Description { get; set; } = "Enables Effects to the player";

        public Dictionary<EffectType, byte> EffectsToApply { get; set; } = new Dictionary<EffectType, byte>();

        protected override void AbilityAdded(Player player)
        {
            Timing.CallDelayed(5f, () =>
            {
                foreach (var effect in EffectsToApply)
                {
                    player.EnableEffect(effect.Key, effect.Value, 0);
                }
            });
        }

        protected override void AbilityRemoved(Player player)
        {
            foreach (var effect in EffectsToApply)
            {
                player.DisableEffect(effect.Key);
            }
        }
    }
}
