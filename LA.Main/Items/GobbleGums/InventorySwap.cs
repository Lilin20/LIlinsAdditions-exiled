using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using MEC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class InventorySwap : FortunaFizzItem
    {
        private const float USE_DELAY = 2f;

        public override uint Id { get; set; } = 817;
        public override string Name { get; set; } = "Backpack Bandito";
        public override string Description { get; set; } = "Swap your entire inventory with a random player.";
        public override float Weight { get; set; } = 0.5f;
        public string NoTargetMessage { get; set; } = "No valid target found!";
        public string SwapMessageFormat { get; set; } = "Inventory swapped with {0}!";
        public override SpawnProperties SpawnProperties { get; set; }

        public InventorySwap()
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
            
            float cooldownEndTime = ev.Player.GetCooldownItem(ItemType.AntiSCP207);
            if (cooldownEndTime > Time.timeSinceLevelLoad)
            {
                ev.IsAllowed = false;
                return;
            }
            
            ev.Player.SetCooldownItem(USE_DELAY, ItemType.AntiSCP207);

            Timing.CallDelayed(USE_DELAY, () => ExecuteInventorySwap(ev));
        }

        private void ExecuteInventorySwap(UsingItemEventArgs ev)
        {
            if (!IsValidUser(ev.Player))
                return;

            var target = GetRandomValidPlayer(ev.Player);
            if (target == null)
            {
                HandleNoTarget(ev);
                return;
            }

            PerformSwap(ev.Player, target, ev.Item);
        }

        private bool IsValidUser(Player player)
        {
            return player != null && player.IsAlive;
        }

        private void HandleNoTarget(UsingItemEventArgs ev)
        {
            ev.Player.ShowHint(NoTargetMessage);
            ev.Item?.Destroy();
        }

        private void PerformSwap(Player user, Player target, Item item)
        {
            SwapInventories(user, target);
            item?.Destroy();

            Log.Debug($"[InventorySwap] {user.Nickname} swapped inventory with {target.Nickname}");
        }

        private Player GetRandomValidPlayer(Player user)
        {
            var validPlayers = Player.List
                .Where(p => IsValidTarget(p, user))
                .ToList();

            if (validPlayers.Count == 0)
                return null;

            return validPlayers[Random.Range(0, validPlayers.Count)];
        }

        private bool IsValidTarget(Player target, Player user)
        {
            return target != null
                && target != user
                && target.IsAlive
                && !target.IsScp;
        }

        private void SwapInventories(Player player1, Player player2)
        {
            var p1Items = CaptureInventory(player1);
            var p2Items = CaptureInventory(player2);

            ClearInventory(player1);
            ClearInventory(player2);

            RestoreInventory(player1, p2Items);
            RestoreInventory(player2, p1Items);

            NotifyPlayers(player1, player2);
        }

        private List<ItemType> CaptureInventory(Player player)
        {
            return player.Items.Select(i => i.Type).ToList();
        }

        private void ClearInventory(Player player)
        {
            var items = player.Items.ToList();
            foreach (var item in items)
                item?.Destroy();
        }

        private void RestoreInventory(Player player, List<ItemType> itemTypes)
        {
            foreach (var itemType in itemTypes)
                player.AddItem(itemType);
        }

        private void NotifyPlayers(Player player1, Player player2)
        {
            player1.ShowHint(string.Format(SwapMessageFormat, player2.Nickname));
            player2.ShowHint(string.Format(SwapMessageFormat, player1.Nickname));
        }
    }
}