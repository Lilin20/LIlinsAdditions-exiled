using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Roles;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class NeverSeen : FortunaFizzItem
    {
        private const float USE_DELAY = 2f;

        public override uint Id { get; set; } = 800;
        public override string Name { get; set; } = "Forget Me Not";
        public override string Description { get; set; } = "You forget every face you saw.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        public NeverSeen()
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

            Timing.CallDelayed(USE_DELAY, () => RemoveFromScp096Targets(ev));
        }

        private static void RemoveFromScp096Targets(UsingItemEventArgs ev)
        {
            if (ev.Player == null || !ev.Player.IsAlive)
                return;

            ev.Item?.Destroy();

            var scp096 = Player.List
                .Where(p => p.Role.Type == RoleTypeId.Scp096)
                .Select(p => p.Role as Scp096Role)
                .FirstOrDefault(role => role != null && role.HasTarget(ev.Player));

            if (scp096 == null)
            {
                Log.Debug($"[NeverSeen] {ev.Player.Nickname} used item but is not targeted by SCP-096");
                return;
            }

            scp096.RemoveTarget(ev.Player);
            Log.Debug($"[NeverSeen] {ev.Player.Nickname} removed from SCP-096 targets");
        }
    }
}
