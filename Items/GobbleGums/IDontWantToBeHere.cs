using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class IDontWantToBeHere : FortunaFizzItem
    {
        private const float USE_DELAY = 2f;
        private const float INVISIBLE_DURATION = 15f;
        private const byte POISONED_INTENSITY = 1;
        private const float POISONED_DURATION = 10f;
        private const byte SILENT_WALK_INTENSITY = 255;
        private const float SILENT_WALK_DURATION = 15f;

        public override uint Id { get; set; } = 809;
        public override string Name { get; set; } = "I Dont Want To Be Here";
        public override string Description { get; set; } = "Gets you out of certain dimensions.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        public IDontWantToBeHere()
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

            Timing.CallDelayed(USE_DELAY, () => ExecutePocketEscape(ev));
        }

        private static void ExecutePocketEscape(UsingItemEventArgs ev)
        {
            if (ev.Player == null || !ev.Player.IsAlive)
                return;

            if (IsPlayerInPocketDimension(ev.Player))
            {
                ApplyEscapeEffects(ev.Player);
                TeleportToScp106(ev.Player);
            }

            ev.Item?.Destroy();
        }

        private static bool IsPlayerInPocketDimension(Player player)
        {
            var pocketRoom = Room.Get(RoomType.Pocket);
            return pocketRoom != null && pocketRoom.Players.Contains(player);
        }

        private static void ApplyEscapeEffects(Player player)
        {
            player.EnableEffect(EffectType.Invisible, INVISIBLE_DURATION, addDurationIfActive: false);
            player.EnableEffect(EffectType.Poisoned, POISONED_INTENSITY, POISONED_DURATION, addDurationIfActive: false);
            player.EnableEffect(EffectType.SilentWalk, SILENT_WALK_INTENSITY, SILENT_WALK_DURATION, addDurationIfActive: false);
        }

        private static void TeleportToScp106(Player player)
        {
            var scp106 = Player.List.FirstOrDefault(p => p.Role.Type == RoleTypeId.Scp106);
            
            if (scp106 != null)
            {
                player.Teleport(scp106);
                Log.Debug($"[IDontWantToBeHere] {player.Nickname} escaped Pocket Dimension to SCP-106");
            }
            else
            {
                Log.Debug($"[IDontWantToBeHere] {player.Nickname} escaped Pocket Dimension but no SCP-106 found");
            }
        }
    }
}
