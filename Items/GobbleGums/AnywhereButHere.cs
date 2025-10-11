using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class AnywhereButHere : CustomItem
    {
        public override uint Id { get; set; } = 801;
        public override string Name { get; set; } = "Anywhere But Here";
        public override string Description { get; set; } = "Teleports you to a random positon.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingAnywhereButHere;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingAnywhereButHere;
            base.UnsubscribeEvents();
        }

        private void OnUsingAnywhereButHere(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem)) return;

            

            Timing.CallDelayed(2f, () =>
            {
                if (ev.Player.CurrentItem != ev.Item) return;
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
                    RoomType.HczCrossRoomWater
                };

                Room randomRoom = allRooms[UnityEngine.Random.Range(0, allRooms.Length)];

                while (forbiddenRoomTypes.Contains(randomRoom.Type))
                {
                    randomRoom = allRooms[UnityEngine.Random.Range(0, allRooms.Length)];
                }

                Log.Debug($"Player {ev.Player.CustomName} teleported to {randomRoom.Type}");
                ev.Player.Teleport(randomRoom.Position + Vector3.up);

                ev.Item.Destroy();
            });
        }
    }
}
