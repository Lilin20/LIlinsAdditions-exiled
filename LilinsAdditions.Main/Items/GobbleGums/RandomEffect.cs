using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LilinsAdditions.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class RandomEffect : FortunaFizzItem
    {
        private const float USE_DELAY = 2f;
        private const int MIN_EFFECT_COUNT = 3;
        private const int MAX_EFFECT_COUNT = 6;
        private const byte MIN_INTENSITY = 1;
        private const byte MAX_INTENSITY = 255;
        private const float MIN_DURATION = 10f;
        private const float MAX_DURATION = 30f;

        private static readonly HashSet<EffectType> ExcludedEffects = new()
        {
            EffectType.PocketCorroding,
            EffectType.PitDeath,
            EffectType.Decontaminating,
            EffectType.FogControl,
            EffectType.None,
            EffectType.Marshmallow,
            EffectType.InsufficientLighting,
            EffectType.Scp1344,
            EffectType.Scp1344Detected,
            EffectType.Scp956Target,
            EffectType.Scp559,
            EffectType.Snowed,
            EffectType.SoundtrackMute,
            EffectType.SpawnProtected
        };

        private static readonly List<EffectType> AvailableEffects = Enum.GetValues(typeof(EffectType))
            .Cast<EffectType>()
            .Where(e => !ExcludedEffects.Contains(e))
            .ToList();

        public override uint Id { get; set; } = 814;
        public override string Name { get; set; } = "Luck of the Draw";
        public override string Description { get; set; } = "Risk it all.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        public RandomEffect()
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

            ApplyRandomEffects(ev);
        }

        private static void ApplyRandomEffects(UsingItemEventArgs ev)
        {
            if (ev.Player == null || !ev.Player.IsAlive)
                return;

            var effectCount = Random.Range(MIN_EFFECT_COUNT, MAX_EFFECT_COUNT);
            var selectedEffects = GetRandomEffects(effectCount);

            foreach (var effect in selectedEffects)
            {
                var intensity = (byte)Random.Range(MIN_INTENSITY, MAX_INTENSITY);
                var duration = Random.Range(MIN_DURATION, MAX_DURATION);

                ev.Player.EnableEffect(effect, intensity, duration);
            }

            ev.Item?.Destroy();
            
            Log.Debug($"[RandomEffect] {ev.Player.Nickname} received {selectedEffects.Count} random effects");
        }

        private static List<EffectType> GetRandomEffects(int count)
        {
            if (count >= AvailableEffects.Count)
                return new List<EffectType>(AvailableEffects);

            var shuffled = new List<EffectType>(AvailableEffects);
            
            for (var i = shuffled.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            return shuffled.Take(count).ToList();
        }
    }
}
