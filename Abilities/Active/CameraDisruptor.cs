
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Roles;
using Exiled.CustomRoles.API.Features;
using MapGeneration;
using MEC;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079.Cameras;
using UnityEngine.LowLevel;

namespace GockelsAIO_exiled.Abilities.Active
{
    [CustomAbility]
    public class CameraDisruptor : ActiveAbility
    {
        public override string Name { get; set; } = "CCD - Chaos Camera Disruptor";
        public override string Description { get; set; } = "Schaltet Kameras in der Umgebung aus.";
        public override float Duration { get; set; } = 20f;
        public override float Cooldown { get; set; } = 1f;

        private readonly Dictionary<Player, CoroutineHandle> _activeCoroutines = new();

        protected override void AbilityAdded(Player player)
        {
            SelectAbility(player);
            base.AbilityAdded(player);
        }

        protected override void AbilityRemoved(Player player)
        {
            if (_activeCoroutines.TryGetValue(player, out var handle))
            {
                Timing.KillCoroutines(handle);
                _activeCoroutines.Remove(player);
            }
            base.AbilityRemoved(player);
        }

        protected override void AbilityEnded(Player player)
        {
            if (_activeCoroutines.TryGetValue(player, out var handle))
            {
                Timing.KillCoroutines(handle);
                _activeCoroutines.Remove(player);
            }

            base.AbilityEnded(player);
        }

        protected override void AbilityUsed(Player player)
        {
            var handle = Timing.RunCoroutine(DisableCamerasCoroutine(player));
            _activeCoroutines[player] = handle; // Store the handle  
            base.AbilityUsed(player);
        }

        private IEnumerator<float> DisableCamerasCoroutine(Player player)
        {
            while (true)
            {
                Room room = player.CurrentRoom;
                if (room == null)
                    yield break;

                // Get SCP-079 player if present  
                Player scp079Player = Player.Get(RoleTypeId.Scp079).FirstOrDefault();

                // Only check for SCP-079 camera ejection if they exist  
                if (scp079Player?.Role is Scp079Role scp079Role)
                {
                    Camera currentScp079Camera = scp079Role.Camera;

                    foreach (Camera camera in room.Cameras)
                    {
                        if (currentScp079Camera == camera)
                        {
                            Log.Info("Kicking SCP-079 out of camera in room");
                            scp079Role.Camera = Camera.Get(CameraType.Hcz079ContChamber);
                            scp079Role.LoseSignal(2f);
                            yield return Timing.WaitForSeconds(0.1f);
                            break;
                        }
                    }
                }
            }
        }
    }
}
