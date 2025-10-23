using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.CustomRoles.API.Features;
using MEC;
using PlayerRoles;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using UnityEngine;

namespace GockelsAIO_exiled.Abilities.Active
{
    [CustomAbility]
    public class Burst : ActiveAbility
    {
        public override string Name { get; set; } = "Burst";
        public override string Description { get; set; } = "Mix a chemical cocktail to burst.";
        public override float Duration { get; set; } = 1f;
        public override float Cooldown { get; set; } = 60f;
        protected override void AbilityAdded(Player player)
        {
            SelectAbility(player);
            base.AbilityAdded(player);
        }

        protected override void AbilityUsed(Player player)
        {
            Exiled.API.Features.Toys.Light light = Exiled.API.Features.Toys.Light.Create(Vector3.zero);
            light.Color = Color.red;
            light.Range = 2f;
            light.Intensity = 2f;
            light.Transform.parent = player.Transform;
            light.Transform.localPosition = Vector3.zero;

            AudioPlayer audioPlayer = AudioPlayer.CreateOrGet($"PlayerSpeaker{UnityEngine.Random.Range(1, 10000)}", onIntialCreation: (p) =>
            {
                Speaker speaker = p.AddSpeaker($"Main{UnityEngine.Random.Range(1, 10000)}", isSpatial: true, minDistance: 1f, maxDistance: 15f);
                speaker.transform.parent = player.Transform;
                speaker.transform.localPosition = Vector3.zero;
            });

            audioPlayer.AddClip("bombsound", loop: false, volume: LilinsAdditions.Instance.Config.BurstSoundVolume, destroyOnEnd: true);

            Timing.CallDelayed(7f, () =>
            {
                light.Destroy();
                if (player.Role == RoleTypeId.Scp0492)
                {
                    ExplosiveGrenade grenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
                    grenade.FuseTime = 0.1f;
                    grenade.SpawnActive(player.Position);
                }
            });

            base.AbilityUsed(player);
        }
    }
}
