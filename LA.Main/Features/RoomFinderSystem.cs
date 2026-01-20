using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using CommandSystem;
using MEC;
using UnityEngine;

namespace RoomFinder
{
    public class RoomFinderSystem
    {
        private Dictionary<Player, RoomTracker> activeTrackers = new Dictionary<Player, RoomTracker>();

        public class RoomTracker
        {
            public Room TargetRoom { get; set; }
            public CoroutineHandle CoroutineHandle { get; set; }
        }
        
        public void StartTracking(Player player, Room targetRoom)
        {
            StopTracking(player);

            var tracker = new RoomTracker
            {
                TargetRoom = targetRoom
            };
            
            tracker.CoroutineHandle = Timing.RunCoroutine(TrackingCoroutine(player, tracker));
            activeTrackers[player] = tracker;
        }
        
        public void StopTracking(Player player)
        {
            if (activeTrackers.TryGetValue(player, out var tracker))
            {
                Timing.KillCoroutines(tracker.CoroutineHandle);
                activeTrackers.Remove(player);
                player.ShowHint("", 1);
            }
        }
        
        private IEnumerator<float> TrackingCoroutine(Player player, RoomTracker tracker)
        {
            while (player.IsAlive && tracker.TargetRoom != null)
            {
                Vector3 targetPos = tracker.TargetRoom.Position;
                Vector3 playerPos = player.Position;
                Vector3 direction = targetPos - playerPos;
                float distance = direction.magnitude;
                
                if (player.CurrentRoom == tracker.TargetRoom)
                {
                    player.ShowHint("<size=40><color=green>★ YOU ARE HERE ★</color></size>", 2f);
                }
                else
                {
                    string arrow = GetDirectionalArrow(player, targetPos);
                    string color = GetDistanceColor(distance);
                    
                    string hint = $"<size=35><color={color}>{arrow}</color></size>\n" +
                                  $"<size=20>{GetRoomDisplayName(tracker.TargetRoom)}\n" +
                                  $"{distance:F1}m</size>";
                    
                    player.ShowHint(hint, 1.5f);
                }
                
                yield return Timing.WaitForSeconds(0.5f);
            }
            
            activeTrackers.Remove(player);
        }
        
        private string GetDirectionalArrow(Player player, Vector3 targetPos)
        {
            Vector3 direction = targetPos - player.Position;
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float viewerYaw = player.CameraTransform.rotation.eulerAngles.y;
            float relative = Mathf.DeltaAngle(viewerYaw, angle);
            
            if (relative > -22.5f && relative <= 22.5f) return "▲"; // Forward
            if (relative > 22.5f && relative <= 67.5f) return "◥";  // Forward-Right
            if (relative > 67.5f && relative <= 112.5f) return "▶"; // Right
            if (relative > 112.5f && relative <= 157.5f) return "◢"; // Back-Right
            if (relative > 157.5f || relative <= -157.5f) return "▼"; // Back
            if (relative > -157.5f && relative <= -112.5f) return "◣"; // Back-Left
            if (relative > -112.5f && relative <= -67.5f) return "◀"; // Left
            return "◤"; // Forward-Left
        }
        
        private string GetDistanceColor(float distance)
        {
            if (distance < 10f) return "green";
            if (distance < 25f) return "yellow";
            if (distance < 50f) return "orange";
            return "red";
        }
        
        private string GetRoomDisplayName(Room room)
        {
            if (room == null) return "Unknown Room";
            
            string name = room.Type.ToString();
            
            name = name.Replace("Lcz", "LCZ ")
                       .Replace("Hcz", "HCZ ")
                       .Replace("Ez", "EZ ")
                       .Replace("Surface", "Surface ");
            
            return name;
        }
        
        public void ShowRoomMenu(Player player, int page = 0)
        {
            var rooms = Room.List.OrderBy(r => r.Type.ToString()).ToList();
            int roomsPerPage = 10;
            int totalPages = (int)Math.Ceiling(rooms.Count / (float)roomsPerPage);
            page = Mathf.Clamp(page, 0, totalPages - 1);

            var pageRooms = rooms.Skip(page * roomsPerPage).Take(roomsPerPage).ToList();

            string menu = $"<b>=== ROOM FINDER (Page {page + 1}/{totalPages}) ===</b>\n";
            
            for (int i = 0; i < pageRooms.Count; i++)
            {
                Room room = pageRooms[i];
                int roomIndex = (page * roomsPerPage) + i;
                menu += $"{roomIndex}: {GetRoomDisplayName(room)}\n";
            }

            menu += "\nUse: .findroom <number> to track a room";
            menu += "\nUse: .stopfind to stop tracking";
            
            player.Broadcast(10, menu);
        }
        
        public void FindRoomByIndex(Player player, int index)
        {
            var rooms = Room.List.OrderBy(r => r.Type.ToString()).ToList();
            
            if (index < 0 || index >= rooms.Count)
            {
                player.Broadcast(3, $"<color=red>Invalid room index. Use 0-{rooms.Count - 1}</color>");
                return;
            }

            StartTracking(player, rooms[index]);
        }
        
        public void FindNearestRoomByName(Player player, string roomName)
        {
            var matchingRooms = Room.List
                .Where(r => r.Type.ToString().ToLower().Contains(roomName.ToLower()))
                .OrderBy(r => Vector3.Distance(player.Position, r.Position))
                .ToList();

            if (!matchingRooms.Any())
            {
                player.Broadcast(3, $"<color=red>No rooms found matching '{roomName}'</color>");
                return;
            }
            
            StartTracking(player, matchingRooms.First());
        }
    }
    
        [CommandHandler(typeof(RemoteAdminCommandHandler))]
        [CommandHandler(typeof(GameConsoleCommandHandler))]
        public class RoomFinderParent : ParentCommand
        {
            public static RoomFinderSystem RoomFinder = new RoomFinderSystem();
            
            public RoomFinderParent()
            {
                LoadGeneratedCommands();
            }
            
            public override string Command { get; } = "roomfinder";
            public override string[] Aliases { get; } = { "rf" };
            public override string Description { get; } = "Room tracking system";
            
            public override void LoadGeneratedCommands()
            {
                RegisterCommand(new MenuCommand());
                RegisterCommand(new FindCommand());
                RegisterCommand(new StopCommand());
            }
            
            protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                response = "Usage: roomfinder <menu|find|stop>";
                return false;
            }
        }
        
        public class MenuCommand : ICommand
        {
            public string Command { get; } = "menu";
            public string[] Aliases { get; } = { "m" };
            public string Description { get; } = "Show room selection menu";
            
            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                Player player = Player.Get(sender);
                if (player == null)
                {
                    response = "This command can only be used by players.";
                    return false;
                }
                
                int page = arguments.Count > 0 && int.TryParse(arguments.At(0), out int p) ? p : 0;
                RoomFinderParent.RoomFinder.ShowRoomMenu(player, page);
                
                response = "Room menu displayed.";
                return true;
            }
        }
        
        public class FindCommand : ICommand
        {
            public string Command { get; } = "find";
            public string[] Aliases { get; } = { "f" };
            public string Description { get; } = "Find and track a room";
            
            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                Player player = Player.Get(sender);
                if (player == null)
                {
                    response = "This command can only be used by players.";
                    return false;
                }
                
                if (arguments.Count == 0)
                {
                    response = "Usage: find <room number or name>";
                    return false;
                }
                
                string target = arguments.At(0);
                
                if (int.TryParse(target, out int roomIndex))
                {
                    RoomFinderParent.RoomFinder.FindRoomByIndex(player, roomIndex);
                }
                else
                {
                    RoomFinderParent.RoomFinder.FindNearestRoomByName(player, target);
                }
                
                response = "Room tracking started.";
                return true;
            }  
        }
        
        public class StopCommand : ICommand
        {
            public string Command { get; } = "stop";
            public string[] Aliases { get; } = { "s" };
            public string Description { get; } = "Stop room tracking";
            
            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                Player player = Player.Get(sender);
                if (player == null)
                {
                    response = "This command can only be used by players.";
                    return false;
                }
                RoomFinderParent.RoomFinder.StopTracking(player);
                response = "Room tracking stopped.";
                return true;
            }
        }
}