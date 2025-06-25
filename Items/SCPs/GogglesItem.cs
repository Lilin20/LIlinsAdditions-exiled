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
        public static Dictionary<int, GogglesItem> equippedGoggles = new Dictionary<int, GogglesItem>();
        protected bool PlayerHasGoggles(Player player)
        {
            if (equippedGoggles.TryGetValue(player.Id, out GogglesItem gogglesItem))
            {
                return this == gogglesItem;
            }
            return false;
        }
        protected virtual void RemoveGoggles(Player player, bool showMessage = true)
        {
            if (!equippedGoggles.TryGetValue(player.Id, out GogglesItem item)) return;
            if (item != this) return;
            equippedGoggles.Remove(player.Id);
            if (showMessage)
            {
                Player.Get(player.Id).ShowHint($"You remove the {Name}");
            }

        }
        protected virtual void EquipGoggles(Player player, bool showMessage = true)
        {
            if (equippedGoggles.TryGetValue(player.Id, out GogglesItem item)) return;
            equippedGoggles.Add(player.Id, this);


            if (showMessage)
            {
                Player.Get(player.Id).ShowHint($"You put on the {Name}");
            }
        }
        protected override void OnOwnerChangingRole(OwnerChangingRoleEventArgs e)
        {
            base.OnOwnerChangingRole(e);
            RemoveGoggles(e.Player, false);
        }
        protected override void SubscribeEvents()
        {
            base.SubscribeEvents();
            PlayerHandler.UsingItemCompleted += OnUsingCompleted;
            PlayerHandler.UsingItem += OnUsing;
        }
        protected override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();
            PlayerHandler.UsingItem -= OnUsing;
            PlayerHandler.UsingItemCompleted -= OnUsingCompleted;
        }
        protected override void OnWaitingForPlayers()
        {
            base.OnWaitingForPlayers();
            equippedGoggles.Clear();
        }
        protected override void OnOwnerDying(OwnerDyingEventArgs e)
        {
            base.OnOwnerDying(e);
            RemoveGoggles(e.Player, false);
        }

        protected override void OnDroppingItem(DroppingItemEventArgs e)
        {
            base.OnDroppingItem(e);
            if (Check(e.Item))
            {
                equippedGoggles.TryGetValue(e.Player.Id, out GogglesItem item);
                e.IsAllowed = false;
                if (item != this)
                {
                    e.IsAllowed = true;
                    return;
                }
                RemoveGoggles(e.Player);
            }
        }
        protected void OnUsing(UsingItemEventArgs e)
        {
            if (!Check(e.Item)) return;
            if (equippedGoggles.ContainsKey(e.Player.Id))
            {
                e.IsAllowed = false;
                e.Player.ShowHint("You are already wearing something!");
            }
        }
        protected void OnUsingCompleted(UsingItemCompletedEventArgs e)
        {
            if (!Check(e.Item)) return;
            e.IsAllowed = false;
            Timing.CallDelayed(0.01f, () => { e.Player.CurrentItem = null; });
            if (!equippedGoggles.ContainsKey(e.Player.Id))
            {
                EquipGoggles(e.Player);
            }
        }
    }
}
