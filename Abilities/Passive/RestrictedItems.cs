using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;

namespace GockelsAIO_exiled.Abilities.Passive
{
    public class RestrictedItems : PassiveAbility
    {
        public override string Name { get; set; } = "Restricted Items";
        public override string Description { get; set; } = "Handles restricted items";

        public List<ItemType> RestrictedItemList { get; set; } = new List<ItemType>();
        public List<Player> PlayersWithRestrictedItemsEffect { get; set; } = new List<Player>();
        public bool RestrictUsingItems { get; set; } = true;
        public bool RestrictPickingUpItems { get; set; } = true;
        public bool RestrictDroppingItems { get; set; } = false;

        protected override void AbilityAdded(Player player)
        {
            PlayersWithRestrictedItemsEffect.Add(player);
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
            Exiled.Events.Handlers.Player.PickingUpItem += OnPickingUpItem;
            Exiled.Events.Handlers.Player.DroppingItem += OnDroppingItem;
        }

        protected override void AbilityRemoved(Player player)
        {
            PlayersWithRestrictedItemsEffect.Remove(player);
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
            Exiled.Events.Handlers.Player.PickingUpItem -= OnPickingUpItem;
            Exiled.Events.Handlers.Player.DroppingItem -= OnDroppingItem;
            player.DisableAllEffects();
        }

        private void OnUsingItem(UsingItemEventArgs ev)
        {
            if (!RestrictUsingItems)
            {
                return;
            }
            if (PlayersWithRestrictedItemsEffect.Contains(ev.Player) && RestrictedItemList != null && RestrictedItemList.Contains(ev.Item.Type))
            {
                ev.IsAllowed = false;
            }
        }

        private void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (!RestrictPickingUpItems)
            {
                return;
            }
            if (PlayersWithRestrictedItemsEffect.Contains(ev.Player) && RestrictedItemList != null && RestrictedItemList.Contains(ev.Pickup.Type))
            {
                ev.IsAllowed = false;
            }
        }

        private void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (!RestrictDroppingItems)
            {
                return;
            }
            if (PlayersWithRestrictedItemsEffect.Contains(ev.Player) && RestrictedItemList != null && RestrictedItemList.Contains(ev.Item.Type))
            {
                ev.IsAllowed = false;
            }
        }
    }
}
