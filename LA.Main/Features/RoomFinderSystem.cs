using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using CommandSystem;
using Exiled.API.Features.Toys;
using MEC;
using UnityEngine;

namespace RoomFinder
{
    public class RoomFinderSystem
    {
        private const float UPDATE_INTERVAL = 0.5f;
        private const float PLAYER_UPDATE_INTERVAL = 5f;
        private const float HINT_DURATION = 1.5f;

        private readonly Dictionary<Player, RoomTracker> _activeTrackers = new();
        private readonly Dictionary<Player, PlayerTracker> _activePlayerTrackers = new();
        private readonly Dictionary<Player, CoordinateTracker> _activeCoordinateTrackers = new();  
  
        #region Coordinate Tracking
        
        public class CoordinateTracker  
        {  
            public Vector3 TargetPosition { get; set; }  
            public string TargetName { get; set; }  
            public CoroutineHandle CoroutineHandle { get; set; }  
        }

        #endregion
        
        #region Room Tracking

        public void StartTracking(Player player, Room targetRoom)
        {
            if (player == null || targetRoom == null) return;

            StopTracking(player);

            var tracker = new RoomTracker
            {
                TargetRoom = targetRoom,
                CoroutineHandle = Timing.RunCoroutine(TrackingCoroutine(player, targetRoom))
            };

            _activeTrackers[player] = tracker;
        }

        public void StopTracking(Player player)
        {
            if (player == null) return;

            if (_activeTrackers.TryGetValue(player, out var tracker))
            {
                Timing.KillCoroutines(tracker.CoroutineHandle);
                _activeTrackers.Remove(player);
                player.ShowHint(string.Empty, 1);
            }
        }

        private IEnumerator<float> TrackingCoroutine(Player player, Room targetRoom)
        {
            while (player?.IsAlive == true && targetRoom != null)
            {
                if (player.CurrentRoom == targetRoom)
                {
                    player.ShowHint("<size=40><color=green>★ YOU ARE HERE ★</color></size>", HINT_DURATION);
                }
                else
                {
                    DisplayRoomTracking(player, targetRoom);
                }

                yield return Timing.WaitForSeconds(UPDATE_INTERVAL);
            }

            _activeTrackers.Remove(player);
        }

        private void DisplayRoomTracking(Player player, Room targetRoom)
        {
            var distance = Vector3.Distance(player.Position, targetRoom.Position);
            var arrow = DirectionHelper.GetDirectionalArrow(player, targetRoom.Position);
            var color = DistanceHelper.GetDistanceColor(distance);

            var hint = $"<size=35><color={color}>{arrow}</color></size>\n" +
                      $"<size=20>{RoomHelper.GetDisplayName(targetRoom)}\n" +
                      $"{distance:F1}m</size>";

            player.ShowHint(hint, HINT_DURATION);
        }

        #endregion

        #region Player Tracking

        public void StartPlayerTracking(Player player, Player targetPlayer)
        {
            if (player == null || targetPlayer == null) return;

            StopPlayerTracking(player);

            var tracker = new PlayerTracker
            {
                TargetPlayer = targetPlayer,
                CoroutineHandle = Timing.RunCoroutine(PlayerTrackingCoroutine(player, targetPlayer))
            };

            _activePlayerTrackers[player] = tracker;
        }

        public void StopPlayerTracking(Player player)
        {
            if (player == null) return;

            if (_activePlayerTrackers.TryGetValue(player, out var tracker))
            {
                Timing.KillCoroutines(tracker.CoroutineHandle);
                _activePlayerTrackers.Remove(player);
                player.ShowHint(string.Empty, 1);
            }
        }

        private IEnumerator<float> PlayerTrackingCoroutine(Player player, Player targetPlayer)
        {
            while (player?.IsAlive == true && targetPlayer?.IsAlive == true)
            {
                if (player == targetPlayer)
                {
                    player.ShowHint("<color=yellow>Why track yourself?</color>", HINT_DURATION);
                }
                else
                {
                    DisplayPlayerTracking(player, targetPlayer);
                }

                yield return Timing.WaitForSeconds(PLAYER_UPDATE_INTERVAL);
            }

            _activePlayerTrackers.Remove(player);
        }

        private void DisplayPlayerTracking(Player player, Player targetPlayer)
        {
            var distance = Vector3.Distance(player.Position, targetPlayer.Position);
            var arrow = DirectionHelper.GetDirectionalArrow(player, targetPlayer.Position);
            var color = DistanceHelper.GetDistanceColor(distance);

            var hint = $"<size=35><color={color}>{arrow}</color></size>\n" +
                      $"<size=20>{targetPlayer.Nickname} ({targetPlayer.Role.Type})\n" +
                      $"{distance:F1}m</size>";

            player.ShowHint(hint, HINT_DURATION);
        }

        #endregion
        
        #region Pocket Dimension Exit Tracking  
  
        public void FindPocketDimensionExit(Player player)
        {
            if (player == null) return;
            
            if (!player.IsInPocketDimension)
            {
                player.ShowHint("<color=red>You must be in the Pocket Dimension to use this command</color>", 3);
                return;
            }
            
            var teleports = Map.PocketDimensionTeleports;
            if (teleports == null || teleports.Count == 0)
            {
                player.ShowHint("<color=red>No pocket dimension exits found</color>", 3);
                return;
            }
            
            var exitTeleport = teleports.FirstOrDefault(t => t._type == PocketDimensionTeleport.PDTeleportType.Exit);
            if (exitTeleport == null)
            {
                player.ShowHint("<color=red>Pocket dimension exit not found</color>", 3);
                return;
            }
            player.ShowHint("<color=green>Tracking: Pocket Dimension Exit</color>", 3);
        }
          
        public void StartCoordinateTracking(Player player, Vector3 position, string name)  
        {  
            if (player == null) return;  
          
            StopTracking(player);  
          
            var tracker = new CoordinateTracker  
            {  
                TargetPosition = position,  
                TargetName = name,  
                CoroutineHandle = Timing.RunCoroutine(CoordinateTrackingCoroutine(player, position, name))  
            };  
          
            _activeCoordinateTrackers[player] = tracker;  
        }  
          
        public void StopCoordinateTracking(Player player)  
        {  
            if (player == null) return;  
          
            if (_activeCoordinateTrackers.TryGetValue(player, out var tracker))  
            {  
                Timing.KillCoroutines(tracker.CoroutineHandle);  
                _activeCoordinateTrackers.Remove(player);  
                player.ShowHint(string.Empty, 1);  
            }  
        }  
          
        private IEnumerator<float> CoordinateTrackingCoroutine(Player player, Vector3 targetPosition, string name)  
        {  
            while (player?.IsAlive == true)  
            {  
                var distance = Vector3.Distance(player.Position, targetPosition);  
                  
                if (distance < 2f)  
                {  
                    player.ShowHint($"<size=40><color=green>★ {name.ToUpper()} ★</color></size>", HINT_DURATION);  
                }  
                else  
                {  
                    var arrow = DirectionHelper.GetDirectionalArrow(player, targetPosition);  
                    var color = DistanceHelper.GetDistanceColor(distance);  
          
                    var hint = $"<size=35><color={color}>{arrow}</color></size>\n" +  
                              $"<size=20>{name}\n" +  
                              $"{distance:F1}m</size>";  
          
                    player.ShowHint(hint, HINT_DURATION);  
                }  
          
                yield return Timing.WaitForSeconds(UPDATE_INTERVAL);  
            }  
          
            _activeCoordinateTrackers.Remove(player);  
        }  
          
        #endregion

        #region Room Menu & Search

        public void FindRoomByIndex(Player player, int index)
        {
            if (player == null) return;

            var rooms = Room.List.OrderBy(r => r.Type.ToString()).ToList();

            if (index < 0 || index >= rooms.Count)
            {
                player.Broadcast(3, $"<color=red>Invalid index. Use 0-{rooms.Count - 1}</color>");
                return;
            }

            StartTracking(player, rooms[index]);
            player.Broadcast(3, $"<color=green>Tracking: {RoomHelper.GetDisplayName(rooms[index])}</color>");
        }

        public void FindNearestRoomByName(Player player, string roomName)
        {
            if (player == null || string.IsNullOrWhiteSpace(roomName)) return;

            var matchingRooms = Room.List
                .Where(r => r.Type.ToString().ToLower().Contains(roomName.ToLower()))
                .OrderBy(r => Vector3.Distance(player.Position, r.Position))
                .ToList();

            if (!matchingRooms.Any())
            {
                player.Broadcast(3, $"<color=red>No rooms found matching '{roomName}'</color>");
                return;
            }

            var nearestRoom = matchingRooms.First();
            StartTracking(player, nearestRoom);
            player.Broadcast(3, $"<color=green>Tracking: {RoomHelper.GetDisplayName(nearestRoom)}</color>");
        }

        #endregion

        #region Tracker Classes

        public class RoomTracker
        {
            public Room TargetRoom { get; set; }
            public CoroutineHandle CoroutineHandle { get; set; }
        }

        public class PlayerTracker
        {
            public Player TargetPlayer { get; set; }
            public CoroutineHandle CoroutineHandle { get; set; }
        }

        #endregion
    }

    #region Helper Classes

    public static class DirectionHelper
    {
        public static string GetDirectionalArrow(Player player, Vector3 targetPos)
        {
            var direction = targetPos - player.Position;
            var angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            var viewerYaw = player.CameraTransform.rotation.eulerAngles.y;
            var relativeAngle = Mathf.DeltaAngle(viewerYaw, angle);

            return relativeAngle switch
            {
                > -22.5f and <= 22.5f => "▲",      // Forward
                > 22.5f and <= 67.5f => "◥",       // Forward-Right
                > 67.5f and <= 112.5f => "▶",      // Right
                > 112.5f and <= 157.5f => "◢",     // Back-Right
                > 157.5f or <= -157.5f => "▼",     // Back
                > -157.5f and <= -112.5f => "◣",   // Back-Left
                > -112.5f and <= -67.5f => "◀",    // Left
                _ => "◤"                            // Forward-Left
            };
        }
    }

    public static class DistanceHelper
    {
        public static string GetDistanceColor(float distance) => distance switch
        {
            < 10f => "green",
            < 25f => "yellow",
            < 50f => "orange",
            _ => "red"
        };
    }

    public static class RoomHelper
    {
        public static string GetDisplayName(Room room)
        {
            if (room == null) return "Unknown Room";

            return room.Type.ToString()
                .Replace("Lcz", "LCZ ")
                .Replace("Hcz", "HCZ ")
                .Replace("Ez", "EZ ")
                .Replace("Surface", "Surface ");
        }
    }

    #endregion
}