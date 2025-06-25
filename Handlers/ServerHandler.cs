using Exiled.API.Enums;
using Exiled.API.Features;
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

        public void OnStart()
        {
            ss.SpawnMysteryBoxes(8);
            ss.SpawnTrapTest();
            ss.SpawnGobblegumMachines(5);
            ss.SpawnCoins(1);

            foreach (var player in Player.List)
            {
                if (player.Role.Team is Team.FoundationForces or Team.ChaosInsurgency or Team.Scientists or Team.ClassD)
                {
                    if (!PlayerHandler.PlayerPoints.ContainsKey(player))
                    {
                        PlayerHandler.PlayerPoints[player] = 10000;
                        Log.Debug($"[OnStart] {player.Nickname} startet mit 800 Punkten.");
                    }
                }
            }

            ADSMonitor.Start();

            foreach (var player in Player.List)
            {
                Log.Debug($"[Check] {player.Nickname} | Alive: {player.IsAlive} | InPoints: {PlayerHandler.PlayerPoints.ContainsKey(player)}");
            }

            Timing.CallDelayed(120, () =>
            {
                tickCoroutine = Timing.RunCoroutine(PlayerHandler.AddPointsOverTime());
            });
        }

        public void OnSpawningGuards()
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
                    Log.Info($"Guard {player.CustomName} spawned in {randomRoom.Type}");
                    player.Teleport(randomRoom.Position + Vector3.up);
                }
            }
        }
    }
}
