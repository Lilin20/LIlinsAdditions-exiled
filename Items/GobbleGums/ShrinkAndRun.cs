using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class ShrinkAndRun : FortunaFizzItem
    {
        private const float USE_DELAY = 2f;
        private const float SHRINK_DURATION = 0.5f;
        private const float EFFECT_DURATION = 5f;
        private const float SHRINK_SCALE = 0.2f;
        private const byte MOVEMENT_BOOST_INTENSITY = 50;

        private static readonly Vector3 ShrinkSize = new(SHRINK_SCALE, SHRINK_SCALE, SHRINK_SCALE);
        private static readonly Vector3 NormalSize = Vector3.one;

        public override uint Id { get; set; } = 816;
        public override string Name { get; set; } = "Shrink and Run";
        public override string Description { get; set; } = "Tiny people run faster!";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        public ShrinkAndRun()
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

            Timing.CallDelayed(USE_DELAY, () => ActivateShrinkAndRun(ev));
        }

        private static void ActivateShrinkAndRun(UsingItemEventArgs ev)
        {
            if (ev.Player == null || !ev.Player.IsAlive)
                return;

            ev.Item?.Destroy();

            Timing.RunCoroutine(SmoothScaleTransition(ev.Player, ShrinkSize, SHRINK_DURATION));
            ev.Player.EnableEffect(EffectType.MovementBoost, MOVEMENT_BOOST_INTENSITY, EFFECT_DURATION);

            Log.Debug($"[ShrinkAndRun] {ev.Player.Nickname} shrunk for {EFFECT_DURATION}s");

            Timing.CallDelayed(EFFECT_DURATION, () => RestoreNormalSize(ev.Player));
        }

        private static void RestoreNormalSize(Player player)
        {
            if (player == null || !player.IsAlive)
                return;

            Timing.RunCoroutine(SmoothScaleTransition(player, NormalSize, SHRINK_DURATION));
            Log.Debug($"[ShrinkAndRun] {player.Nickname} returned to normal size");
        }

        private static IEnumerator<float> SmoothScaleTransition(Player player, Vector3 targetScale, float duration)
        {
            if (player == null || !player.IsAlive)
                yield break;

            var initialScale = player.Scale;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                if (player == null || !player.IsAlive)
                    yield break;

                var t = elapsed / duration;
                player.Scale = Vector3.Lerp(initialScale, targetScale, t);
                
                elapsed += Time.deltaTime;
                yield return Timing.WaitForOneFrame;
            }

            if (player != null && player.IsAlive)
                player.Scale = targetScale;
        }
    }
}
