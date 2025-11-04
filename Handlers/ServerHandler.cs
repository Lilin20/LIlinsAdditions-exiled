using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using GockelsAIO_exiled.Features;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GockelsAIO_exiled.Handlers
{
    public class ServerHandler
    {
        public static CoroutineHandle tickCoroutine;
        public SchematicSpawner ss = new();

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            Timing.KillCoroutines(tickCoroutine);
        }

        public void OnMapGeneration()
        {
            Timing.CallDelayed(2f, () =>
            {
                if (LilinsAdditions.Instance.Config.EnableMysteryBox)
                {
                    ss.SpawnMysteryBoxes(8);
                }

                if (LilinsAdditions.Instance.Config.EnableFortunaFizz)
                {
                    ss.SpawnGobblegumMachines(5);
                }

                if (LilinsAdditions.Instance.Config.EnableCeilingTrap)
                {
                    ss.SpawnTrapTest();
                }

                if (LilinsAdditions.Instance.Config.EnableHiddenCoins)
                {
                    ss.SpawnCoins(2);
                }
            });
        }

        public void OnStart()
        {
            Timing.CallDelayed(LilinsAdditions.Instance.Config.PointsOverTimeDelay, () =>
            {
                tickCoroutine = Timing.RunCoroutine(PlayerHandler.AddPointsOverTime());
            });
        }

        public void OnSpawningGuards()
        {
            if (LilinsAdditions.Instance.Config.EnableRandomGuardSpawn)
            {

                Room[] allRooms = Room.List.ToArray();
                List<RoomType> forbiddenRoomTypes = new List<RoomType>
                    {
                        RoomType.Hcz079,
                        RoomType.Hcz106,
                        RoomType.HczHid,
                        RoomType.Hcz096,
                        RoomType.Hcz939,
                        RoomType.HczTestRoom,
                        RoomType.Hcz049,
                        RoomType.EzCollapsedTunnel,
                        RoomType.EzGateA,
                        RoomType.EzGateB,
                        RoomType.Lcz173,
                        RoomType.LczCrossing,
                        RoomType.HczTesla,
                        RoomType.EzShelter,
                        RoomType.Pocket,
                        RoomType.HczCrossRoomWater,
                        RoomType.Surface,
                    };

                foreach (Player player in Player.List)
                {
                    if (player.Role == RoleTypeId.FacilityGuard)
                    {
                        Room randomRoom = allRooms[UnityEngine.Random.Range(0, allRooms.Length)];
                        while (forbiddenRoomTypes.Contains(randomRoom.Type))
                        {
                            randomRoom = allRooms[UnityEngine.Random.Range(0, allRooms.Length)];
                        }
                        Log.Debug($"Guard {player.CustomName} spawned in {randomRoom.Type}");
                        player.Teleport(randomRoom.Position + Vector3.up);
                    }
                }

            }
        }

        public IEnumerator<float> PeriodicPointSystemCheck()
        {
            for (; ; ) // Run indefinitely  
            {
                yield return Timing.WaitForSeconds(2f); // Check every 5 seconds (adjust as needed)  

                foreach (Player player in Player.List)
                {
                    if (player == null) continue;

                    RoleTypeId currentRole = player.Role.Type;
                    bool shouldBeInSystem = ShouldPlayerBeInPointSystem(currentRole);
                    bool isInSystem = PlayerHandler.PlayerPoints.ContainsKey(player);

                    if (shouldBeInSystem && !isInSystem)
                    {
                        // Add player to point system  
                        PlayerHandler.PlayerPoints[player] = LilinsAdditions.Instance.Config.StartingPoints;
                        Log.Debug($"[PointSystem] {player.Nickname} added to point system. Role: {currentRole}");
                    }
                    else if (!shouldBeInSystem && isInSystem)
                    {
                        // Remove player from point system  
                        PlayerHandler.PlayerPoints.Remove(player);
                        Log.Debug($"[PointSystem] {player.Nickname} removed from point system. Role: {currentRole}");
                    }
                }
            }
        }

        private bool ShouldPlayerBeInPointSystem(RoleTypeId role)
        {
            // Invalid/dead roles should not be in system  
            if (role == RoleTypeId.Spectator ||
                role == RoleTypeId.Overwatch ||
                role == RoleTypeId.Destroyed ||
                role == RoleTypeId.None ||
                role == RoleTypeId.Filmmaker ||
                role == RoleTypeId.Tutorial)
            {
                return false;
            }

            // Check if role belongs to valid team  
            var team = Exiled.API.Extensions.RoleExtensions.GetTeam(role);
            return team == Team.FoundationForces ||
                   team == Team.ChaosInsurgency ||
                   team == Team.Scientists ||
                   team == Team.ClassD;
        }
    }
}
