using Exiled.API.Enums;  
using Exiled.API.Features;  
using Exiled.API.Features.Attributes;  
using Exiled.API.Features.Spawn;  
using Exiled.CustomItems.API.Features;  
using Exiled.Events.EventArgs.Player;  
using RoomFinder;  
using System.Collections.Generic;  
  
namespace LilinsAdditions.Items.Other  
{  
    [CustomItem(ItemType.Radio)]  
    public class NavigationDevice : CustomItem  
    {  
        public override uint Id { get; set; } = 1002;  
        public override string Name { get; set; } = "Room Tracker";  
        public override string Description { get; set; } = "Electronic navigation device with zone-based room selector";  
        public override float Weight { get; set; } = 0.5f;  
  
        public RoomFinderSystem roomFinder = new();  
        public override SpawnProperties SpawnProperties { get; set; }  
  
        // Define important rooms by zone  
        private readonly Dictionary<ZoneType, List<RoomType>> importantRooms = new()  
        {  
            {  
                ZoneType.LightContainment,  
                new List<RoomType>  
                {  
                    RoomType.Lcz914,        // SCP-914  
                    RoomType.LczArmory,     // Armory  
                    RoomType.LczCafe,       // Cafe  
                    RoomType.Lcz173,        // SCP-173  
                    RoomType.LczClassDSpawn // Class-D spawn  
                }  
            },  
            {  
                ZoneType.HeavyContainment,  
                new List<RoomType>  
                {  
                    RoomType.Hcz079,        // SCP-079  
                    RoomType.Hcz939,        // SCP-939  
                    RoomType.Hcz049,        // SCP-049  
                    RoomType.HczArmory,     // Armory  
                    RoomType.HczHid,         // MicroHID
                    RoomType.Hcz127
                }  
            },  
            {  
                ZoneType.Entrance,  
                new List<RoomType>  
                {  
                    RoomType.EzGateA,       // Gate A  
                    RoomType.EzGateB,       // Gate B  
                    RoomType.EzCafeteria,   // Cafeteria  
                    RoomType.EzShelter      // Surface shelter  
                }  
            }  
        };  
  
        protected override void SubscribeEvents()  
        {  
            Exiled.Events.Handlers.Player.ChangingRadioPreset += OnUsingItem;  
            base.SubscribeEvents();  
        }  
  
        protected override void UnsubscribeEvents()  
        {  
            Exiled.Events.Handlers.Player.ChangingRadioPreset -= OnUsingItem;  
            base.UnsubscribeEvents();  
        }  
  
        private void OnUsingItem(ChangingRadioPresetEventArgs ev)  
        {  
            // Get player's current zone  
            ZoneType currentZone = ev.Player.CurrentRoom.Zone;  
              
            // Use radio range as selector (0-3 for different room categories)  
            int selectedIndex = (int)ev.NewValue;  
              
            if (importantRooms.TryGetValue(currentZone, out var roomsInZone) &&   
                selectedIndex < roomsInZone.Count)  
            {  
                RoomType targetRoomType = roomsInZone[selectedIndex];  
                  
                // Use the RoomType enum name as search string (matches your FindNearestRoomByName function)  
                string roomTypeName = targetRoomType.ToString();  
                  
                ev.Player.ShowHint($"Tracking: {roomTypeName} (Range {selectedIndex + 1})", 3);  
                roomFinder.FindNearestRoomByName(ev.Player, roomTypeName);  
            }  
            else  
            {  
                ev.Player.ShowHint($"No room available for range {selectedIndex + 1} in {currentZone}", 3);  
            }  
        }  
    }  
}