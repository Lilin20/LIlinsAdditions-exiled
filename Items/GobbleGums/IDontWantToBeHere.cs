using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class IDontWantToBeHere : CustomItem
    {
        public override uint Id { get; set; } = 809;
        public override string Name { get; set; } = "I Dont Want To Be Here";
        public override string Description { get; set; } = "Gets you out of certain dimensions.";
        public override float Weight { get; set; } = 0.5f;

        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingIDontWantToBeHere;

            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingIDontWantToBeHere;

            base.UnsubscribeEvents();
        }

        private void OnUsingIDontWantToBeHere(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            Timing.CallDelayed(2f, () =>
            {
                Room room = Room.Get(RoomType.Pocket);

                if (room.Players.Contains(ev.Player))
                {
                    ev.Player.EnableEffect(EffectType.Invisible, 15f, false);
                    ev.Player.EnableEffect(EffectType.Poisoned, 1, 10f, false);
                    ev.Player.EnableEffect(EffectType.SilentWalk, 255, 15f, false);

                    foreach (Exiled.API.Features.Player larry in Exiled.API.Features.Player.List)
                    {
                        if (larry.Role == RoleTypeId.Scp106)
                        {
                            ev.Player.Teleport(larry);
                        }
                    }
                }

                ev.Item.Destroy();
            });
        }
    }
}
