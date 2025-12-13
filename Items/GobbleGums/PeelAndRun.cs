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
    public class PeelAndRun : FortunaFizzItem
    {
        private const float USE_DELAY = 2f;
        private const float DEATH_CHANCE = 0.25f;

        private const byte DEATH_EFFECT_INTENSITY = 1;
        private const float DEATH_EFFECT_DURATION = 100f;
        
        private const byte ESCAPE_INVISIBLE_INTENSITY = 1;
        private const float ESCAPE_EFFECT_DURATION = 15f;
        private const byte ESCAPE_MOVEMENT_BOOST = 50;
        private const byte ESCAPE_FLASH_INTENSITY = 1;
        private const float ESCAPE_FLASH_DURATION = 1f;
        private const byte ESCAPE_SILENT_WALK_INTENSITY = 10;
        private const float ESCAPE_LOW_HEALTH = 25f;
        
        private const string RAGDOLL_DEATH_REASON = "Es sieht aus wie eine leblose Hülle.";

        public override uint Id { get; set; } = 807;
        public override string Name { get; set; } = "Peel And Run";
        public override string Description { get; set; } = "Drop your skin and run. Has a chance to kill you.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        public PeelAndRun()
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

            Timing.CallDelayed(USE_DELAY, () => ExecutePeelAndRun(ev));
        }

        private static void ExecutePeelAndRun(UsingItemEventArgs ev)
        {
            if (ev.Player == null || !ev.Player.IsAlive)
                return;

            var roll = Random.value;

            if (roll <= DEATH_CHANCE)
                ApplyDeathEffects(ev.Player);
            else
                ApplyEscapeEffects(ev.Player);

            ev.Item?.Destroy();
        }

        private static void ApplyDeathEffects(Player player)
        {
            player.EnableEffect(EffectType.CardiacArrest, DEATH_EFFECT_INTENSITY, DEATH_EFFECT_DURATION);
            player.EnableEffect(EffectType.AmnesiaItems, DEATH_EFFECT_INTENSITY, DEATH_EFFECT_DURATION);
            player.EnableEffect(EffectType.Bleeding, DEATH_EFFECT_INTENSITY, DEATH_EFFECT_DURATION);
            player.EnableEffect(EffectType.Poisoned, DEATH_EFFECT_INTENSITY, DEATH_EFFECT_DURATION);
            player.EnableEffect(EffectType.Hemorrhage, DEATH_EFFECT_INTENSITY, DEATH_EFFECT_DURATION);
            player.EnableEffect(EffectType.SeveredHands, DEATH_EFFECT_INTENSITY, DEATH_EFFECT_DURATION);
            
            Log.Debug($"[PeelAndRun] {player.Nickname} failed - death effects applied");
        }

        private static void ApplyEscapeEffects(Player player)
        {
            player.EnableEffect(EffectType.Invisible, ESCAPE_INVISIBLE_INTENSITY, ESCAPE_EFFECT_DURATION);
            player.EnableEffect(EffectType.MovementBoost, ESCAPE_MOVEMENT_BOOST, ESCAPE_EFFECT_DURATION);
            player.EnableEffect(EffectType.Flashed, ESCAPE_FLASH_INTENSITY, ESCAPE_FLASH_DURATION);
            player.EnableEffect(EffectType.SilentWalk, ESCAPE_SILENT_WALK_INTENSITY, ESCAPE_EFFECT_DURATION);
            
            player.Health = ESCAPE_LOW_HEALTH;
            player.MaxHealth = ESCAPE_LOW_HEALTH;
            
            SpawnDecoyRagdoll(player);
            
            Log.Debug($"[PeelAndRun] {player.Nickname} succeeded - escape effects applied");
        }

        private static void SpawnDecoyRagdoll(Player player)
        {
            Ragdoll.CreateAndSpawn(
                player.Role,
                player.DisplayNickname,
                RAGDOLL_DEATH_REASON,
                player.Position,
                player.ReferenceHub.PlayerCameraReference.rotation
            );
        }
    }
}
