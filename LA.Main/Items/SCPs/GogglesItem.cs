using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.CustomItems.API.EventArgs;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerHandler = Exiled.Events.Handlers.Player;

namespace GockelsAIO_exiled.Items.SCPs
{
    public abstract class GogglesItem : CustomItem
    {
        private const float ITEM_CLEAR_DELAY = 0.01f;
        private const string ALREADY_WEARING_MESSAGE = "You are already wearing something!";
        
        private static readonly Dictionary<int, GogglesItem> EquippedGoggles = new();

        protected bool PlayerHasGoggles(Player player)
        {
            return EquippedGoggles.TryGetValue(player.Id, out var gogglesItem) && this == gogglesItem;
        }

        protected virtual void RemoveGoggles(Player player, bool showMessage = true)
        {
            if (!EquippedGoggles.TryGetValue(player.Id, out var item) || item != this)
                return;

            EquippedGoggles.Remove(player.Id);

            if (showMessage)
                player.ShowHint($"You remove the {Name}");

            Log.Debug($"[GogglesItem] {player.Nickname} removed {Name}");
        }

        protected virtual void EquipGoggles(Player player, bool showMessage = true)
        {
            if (EquippedGoggles.ContainsKey(player.Id))
                return;

            EquippedGoggles[player.Id] = this;

            if (showMessage)
                player.ShowHint($"You put on the {Name}");

            Log.Debug($"[GogglesItem] {player.Nickname} equipped {Name}");
        }

        protected override void OnOwnerChangingRole(OwnerChangingRoleEventArgs e)
        {
            base.OnOwnerChangingRole(e);
            RemoveGoggles(e.Player, showMessage: false);
        }

        protected override void OnOwnerDying(OwnerDyingEventArgs e)
        {
            base.OnOwnerDying(e);
            RemoveGoggles(e.Player, showMessage: false);
        }

        protected override void OnWaitingForPlayers()
        {
            base.OnWaitingForPlayers();
            EquippedGoggles.Clear();
            Log.Debug("[GogglesItem] Cleared all equipped goggles");
        }

        protected override void SubscribeEvents()
        {
            base.SubscribeEvents();
            PlayerHandler.UsingItem += OnUsingItem;
            PlayerHandler.UsingItemCompleted += OnUsingItemCompleted;
            PlayerHandler.Left += OnPlayerLeft;
        }

        protected override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();
            PlayerHandler.UsingItem -= OnUsingItem;
            PlayerHandler.UsingItemCompleted -= OnUsingItemCompleted;
            PlayerHandler.Left -= OnPlayerLeft;
        }

        protected override void OnDroppingItem(DroppingItemEventArgs e)
        {
            base.OnDroppingItem(e);

            if (!Check(e.Item))
                return;

            if (!EquippedGoggles.TryGetValue(e.Player.Id, out var equippedItem))
            {
                e.IsAllowed = true;
                return;
            }

            if (equippedItem != this)
            {
                e.IsAllowed = true;
                return;
            }

            e.IsAllowed = false;
            RemoveGoggles(e.Player);
        }

        private void OnUsingItem(UsingItemEventArgs e)
        {
            if (!Check(e.Item))
                return;

            if (EquippedGoggles.ContainsKey(e.Player.Id))
            {
                e.IsAllowed = false;
                e.Player.ShowHint(ALREADY_WEARING_MESSAGE);
            }
        }

        private void OnUsingItemCompleted(UsingItemCompletedEventArgs e)
        {
            if (!Check(e.Item))
                return;

            e.IsAllowed = false;
            Timing.CallDelayed(ITEM_CLEAR_DELAY, () => ClearCurrentItem(e.Player));

            if (!EquippedGoggles.ContainsKey(e.Player.Id))
                EquipGoggles(e.Player);
        }

        private static void ClearCurrentItem(Player player)
        {
            if (player != null && player.IsConnected)
                player.CurrentItem = null;
        }

        private void OnPlayerLeft(LeftEventArgs e)
        {
            if (EquippedGoggles.Remove(e.Player.Id))
                Log.Debug($"[GogglesItem] Cleaned up goggles for disconnected player {e.Player.Nickname}");
        }
    }
}