using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class NeverSeen : CustomItem
    {
        public override uint Id { get; set; } = 800;
        public override string Name { get; set; } = "Forget Me Not";
        public override string Description { get; set; } = "Du vergisst sämtliche Gesichter die du je gesehen hast.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingSilentStep;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingSilentStep;
            base.UnsubscribeEvents();
        }

        private void OnUsingSilentStep(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem)) return;

            Timing.CallDelayed(2f, () =>
            {
                foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List)
                {
                    if (player.Role == RoleTypeId.Scp096)
                    {
                        if (player.Role is not Exiled.API.Features.Roles.Scp096Role scp096)
                        {
                            continue;
                        }

                        if (!scp096.HasTarget(ev.Player))
                        {
                            continue;
                        }

                        scp096.RemoveTarget(ev.Player);
                        ev.Item.Destroy();
                        return;
                    }
                }
            });
        }
    }
}
