using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using UnityEngine;

namespace LilinsAdditions.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class AnywhereButHere : FortunaFizzItem
    {
        private static readonly HashSet<RoomType> ForbiddenRoomTypes = new()
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
            RoomType.HczIncineratorWayside
        };

        public override uint Id { get; set; } = 801;
        public override string Name { get; set; } = "Anywhere But Here";
        public override string Description { get; set; } = "Teleports you to a random position.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        public AnywhereButHere()
        {
            Buyable = true;
        }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
            base.UnsubscribeEvents();
        }

        private void OnUsingItem(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            ev.IsAllowed = false;
            
            ExecuteTeleport(ev);
        }

        private static void ExecuteTeleport(UsingItemEventArgs ev)
        {
            if (ev.Player == null || !ev.Player.IsAlive || ev.Player.CurrentItem != ev.Item)
                return;

            var targetRoom = GetRandomSafeRoom();
            if (targetRoom == null)
            {
                Log.Warn($"[AnywhereButHere] Could not find safe room for {ev.Player.Nickname}");
                return;
            }

            ev.Player.Teleport(targetRoom.Position + Vector3.up);
            Log.Debug($"[AnywhereButHere] {ev.Player.Nickname} teleported to {targetRoom.Type}");
            
            ev.Item?.Destroy();
        }

        private static Room GetRandomSafeRoom()
        {
            var allRooms = Room.List.ToList();
            var safeRooms = allRooms.Where(r => !ForbiddenRoomTypes.Contains(r.Type)).ToList();

            if (safeRooms.Count == 0)
            {
                Log.Error("[AnywhereButHere] No safe rooms available!");
                return null;
            }

            return safeRooms[Random.Range(0, safeRooms.Count)];
        }
    }
}
