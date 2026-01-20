using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Roles;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace LilinsAdditions.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class LightHeaded : FortunaFizzItem
    {
        private const float USE_DELAY = 2f;
        private const float EFFECT_DURATION = 15f;
        private const float REDUCED_GRAVITY_Y = -3.8f;

        public override uint Id { get; set; } = 815;
        public override string Name { get; set; } = "Light Headed";
        public override string Description { get; set; } = "Everything feels so light?";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        public LightHeaded()
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

            ev.IsAllowed = false;
            
            if (ev.Player.Role is not FpcRole fpcRole)
                return;

            var originalGravity = fpcRole.Gravity;
            ApplyReducedGravity(ev, fpcRole, originalGravity);
        }

        private static void ApplyReducedGravity(UsingItemEventArgs ev, FpcRole fpcRole, Vector3 originalGravity)
        {
            if (ev.Player == null || !ev.Player.IsAlive || ev.Player.Role is not FpcRole)
                return;

            fpcRole.Gravity = new Vector3(0, REDUCED_GRAVITY_Y, 0);
            ev.Item?.Destroy();

            Log.Debug($"[LightHeaded] {ev.Player.Nickname} gravity reduced for {EFFECT_DURATION}s");

            Timing.CallDelayed(EFFECT_DURATION, () => RestoreGravity(ev.Player, fpcRole, originalGravity));
        }

        private static void RestoreGravity(Player player, FpcRole fpcRole, Vector3 originalGravity)
        {
            if (player == null || player.Role is not FpcRole)
                return;
            
            fpcRole.Gravity = originalGravity;
            Log.Debug($"[LightHeaded] {player.Nickname} gravity restored");
        }
    }
}
