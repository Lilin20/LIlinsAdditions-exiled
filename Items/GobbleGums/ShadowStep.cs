using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class ShadowStep : FortunaFizzItem
    {
        private const float USE_DELAY = 2f;
        private const float EFFECT_DURATION = 15f;
        private const byte SILENT_WALK_INTENSITY = 100;
        private const byte FOG_CONTROL_INTENSITY = 0;

        public override uint Id { get; set; } = 805;
        public override string Name { get; set; } = "Shadow Step";
        public override string Description { get; set; } = "Walk freely and see whats to come.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        public ShadowStep()
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

            Timing.CallDelayed(USE_DELAY, () => ActivateShadowStep(ev));
        }

        private static void ActivateShadowStep(UsingItemEventArgs ev)
        {
            if (ev.Player == null || !ev.Player.IsAlive)
                return;

            var originalPosition = ev.Player.Position;

            ApplyShadowEffects(ev.Player);
            ev.Item?.Destroy();

            Log.Debug($"[ShadowStep] {ev.Player.Nickname} entered shadow realm at {originalPosition}");

            Timing.CallDelayed(EFFECT_DURATION, () => ExitShadowStep(ev.Player, originalPosition));
        }

        private static void ApplyShadowEffects(Player player)
        {
            player.EnableEffect(EffectType.Ghostly);
            player.EnableEffect(EffectType.SilentWalk, SILENT_WALK_INTENSITY);
            player.EnableEffect(EffectType.FogControl, FOG_CONTROL_INTENSITY);
            player.EnableEffect(EffectType.Invisible);
            player.Handcuff();
        }

        private static void ExitShadowStep(Player player, Vector3 returnPosition)
        {
            if (player == null || !player.IsAlive)
            {
                Log.Debug($"[ShadowStep] Player no longer alive, skipping exit");
                return;
            }

            player.Teleport(returnPosition);
            player.DisableAllEffects();
            player.RemoveHandcuffs();

            Log.Debug($"[ShadowStep] {player.Nickname} exited shadow realm");
        }
    }
}
