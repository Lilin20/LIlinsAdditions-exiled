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

        public void OnStart()
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


            //used to spawn a single instance of the custom keycard so it spawns correctly when using 914 - need to change that.
            //CustomItem.TrySpawn(4444, Room.Get(RoomType.HczCrossRoomWater).Position, out Pickup test);
        }
    }
}
