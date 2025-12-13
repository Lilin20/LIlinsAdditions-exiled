using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GockelsAIO_exiled.Handlers
{
    public class ServerHandler
    {
        private const float MAP_GENERATION_DELAY = 2f;
        private const float POINT_SYSTEM_CHECK_INTERVAL = 2f;
        private const int DEFAULT_MYSTERY_BOX_COUNT = 8;
        private const int DEFAULT_GOBBLEGUM_COUNT = 5;
        private const int DEFAULT_COIN_COUNT = 2;
        
        private static readonly HashSet<RoomType> ForbiddenGuardSpawnRooms = new()
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

        private static readonly HashSet<RoleTypeId> InvalidRoles = new()
        {
            RoleTypeId.Spectator,
            RoleTypeId.Overwatch,
            RoleTypeId.Destroyed,
            RoleTypeId.None,
            RoleTypeId.Filmmaker,
            RoleTypeId.Tutorial
        };
        
        public static CoroutineHandle TickCoroutine { get; private set; }
        private readonly SchematicSpawner _schematicSpawner = new();

        #region Round Events

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            Timing.KillCoroutines(TickCoroutine);
        }

        public void OnStart()
        {
            var delay = LilinsAdditions.Instance.Config.PointsOverTimeDelay;
            Timing.CallDelayed(delay, () =>
            {
                TickCoroutine = Timing.RunCoroutine(PlayerHandler.AddPointsOverTime());
            });
        }

        #endregion

        #region Map Generation

        public void OnMapGeneration()
        {
            Timing.CallDelayed(MAP_GENERATION_DELAY, SpawnMapFeatures);
        }

        private void SpawnMapFeatures()
        {
            var config = LilinsAdditions.Instance.Config;

            if (config.EnableMysteryBox)
                _schematicSpawner.SpawnMysteryBoxes(DEFAULT_MYSTERY_BOX_COUNT);

            if (config.EnableFortunaFizz)
                _schematicSpawner.SpawnGobblegumMachines(DEFAULT_GOBBLEGUM_COUNT);

            if (config.EnableHiddenCoins)
                _schematicSpawner.SpawnCoins(DEFAULT_COIN_COUNT);
        }

        #endregion

        #region Guard Spawning

        public void OnSpawningGuards()
        {
            if (!LilinsAdditions.Instance.Config.EnableRandomGuardSpawn)
                return;

            var eligibleRooms = GetEligibleGuardSpawnRooms();
            if (eligibleRooms.Length == 0)
            {
                Log.Warn("[GuardSpawn] No eligible rooms found for guard spawning!");
                return;
            }

            SpawnGuardsInRandomRooms(eligibleRooms);
        }

        private static Room[] GetEligibleGuardSpawnRooms()
        {
            return Room.List
                .Where(r => !ForbiddenGuardSpawnRooms.Contains(r.Type))
                .ToArray();
        }

        private static void SpawnGuardsInRandomRooms(Room[] eligibleRooms)
        {
            var guards = Player.List.Where(p => p.Role == RoleTypeId.FacilityGuard);

            foreach (var guard in guards)
            {
                var randomRoom = eligibleRooms[UnityEngine.Random.Range(0, eligibleRooms.Length)];
                guard.Teleport(randomRoom.Position + Vector3.up);
            
                Log.Debug($"[GuardSpawn] {guard.Nickname} spawned in {randomRoom.Type}");
            }
        }

        #endregion

        #region Point System Management

        public IEnumerator<float> PeriodicPointSystemCheck()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(POINT_SYSTEM_CHECK_INTERVAL);

                UpdatePointSystemMembership();
            }
        }

        private void UpdatePointSystemMembership()
        {
            var config = LilinsAdditions.Instance.Config;

            foreach (var player in Player.List)
            {
                if (player == null) 
                    continue;

                var shouldBeInSystem = ShouldPlayerBeInPointSystem(player.Role.Type);
                var isInSystem = PlayerHandler.PlayerPoints.ContainsKey(player);

                if (shouldBeInSystem && !isInSystem)
                {
                    AddPlayerToPointSystem(player, config.StartingPoints);
                }
                else if (!shouldBeInSystem && isInSystem)
                {
                    RemovePlayerFromPointSystem(player);
                }
            }
        }

        private static void AddPlayerToPointSystem(Player player, int startingPoints)
        {
            PlayerHandler.PlayerPoints[player] = startingPoints;
            Log.Debug($"[PointSystem] {player.Nickname} added with {startingPoints} points. Role: {player.Role.Type}");
        }

        private static void RemovePlayerFromPointSystem(Player player)
        {
            PlayerHandler.PlayerPoints.Remove(player);
            Log.Debug($"[PointSystem] {player.Nickname} removed. Role: {player.Role.Type}");
        }

        private static bool ShouldPlayerBeInPointSystem(RoleTypeId role)
        {
            if (InvalidRoles.Contains(role))
                return false;

            var team = Exiled.API.Extensions.RoleExtensions.GetTeam(role);
            return team is Team.FoundationForces 
                or Team.ChaosInsurgency 
                or Team.Scientists 
                or Team.ClassD;
        }

        #endregion
    }
}
