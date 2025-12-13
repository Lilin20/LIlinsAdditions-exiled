using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class Switcheroo : FortunaFizzItem
    {
        private const float USE_DELAY = 2f;
        
        private static readonly HashSet<RoleTypeId> SwapIgnoredRoles = new()
        {
            RoleTypeId.Spectator,
            RoleTypeId.Filmmaker,
            RoleTypeId.Overwatch,
            RoleTypeId.Scp079,
            RoleTypeId.Tutorial,
        };

        public override uint Id { get; set; } = 813;
        public override string Name { get; set; } = "Switcherooo";
        public override string Description { get; set; } = "Swap places – the chaos decides.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        public Switcheroo()
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

            Timing.CallDelayed(USE_DELAY, () => ExecuteSwap(ev));
        }

        private static void ExecuteSwap(UsingItemEventArgs ev)
        {
            if (ev.Player == null || !ev.Player.IsAlive)
                return;

            var targetPlayer = GetRandomSwapTarget(ev.Player);
            if (targetPlayer == null)
                return;

            SwapPlayerPositions(ev.Player, targetPlayer);
            ev.Item?.Destroy();
        }

        private static Player GetRandomSwapTarget(Player excludePlayer)
        {
            var eligiblePlayers = Player.List
                .Where(p => p != excludePlayer 
                         && p.IsAlive 
                         && !SwapIgnoredRoles.Contains(p.Role.Type))
                .ToList();

            return eligiblePlayers.Count > 0 ? eligiblePlayers.RandomItem() : null;
        }

        private static void SwapPlayerPositions(Player player1, Player player2)
        {
            var tempPosition = player2.Position;
            player2.Teleport(player1.Position);
            player1.Teleport(tempPosition);
        }
    }
}
