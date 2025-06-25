using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class Switcheroo : CustomItem
    {
        public override uint Id { get; set; } = 813;
        public override string Name { get; set; } = "Switcherooo";
        public override string Description { get; set; } = "Orte tauschen, Wege kreuzen – das Chaos entscheidet.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }
        public HashSet<RoleTypeId> PlayerSwapIgnoredRoles { get; set; } = new()
        {
            RoleTypeId.Spectator,
            RoleTypeId.Filmmaker,
            RoleTypeId.Overwatch,
            RoleTypeId.Scp079,
            RoleTypeId.Tutorial,
        };

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingSwitcheroo;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingSwitcheroo;
            base.UnsubscribeEvents();
        }

        private void OnUsingSwitcheroo(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem)) return;

            Timing.CallDelayed(2f, () =>
            {
                var playerList = Player.List.Where(x => x.IsAlive && !PlayerSwapIgnoredRoles.Contains(x.Role.Type)).ToList();
                playerList.Remove(ev.Player);

                if (playerList.IsEmpty())
                {
                    return;
                }

                var targetPlayer = playerList.RandomItem();
                var pos = targetPlayer.Position;

                targetPlayer.Teleport(ev.Player.Position);
                ev.Player.Teleport(pos);

                ev.Item.Destroy();
            });
        }
    }
}
