using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using MEC;
using System.Collections.Generic;
using Exiled.API.Features.Items;
using PlayerRoles.FirstPersonControl;
using UnityEngine;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class CardiacOverdrive : FortunaFizzItem
    {
        private const float USE_DELAY = 2f;
        private const float EFFECT_DURATION = 60f;
        private const float HP_DRAIN_PER_REGEN = 2.5f;
        private const float DRAIN_CHECK_INTERVAL = 0.5f;
        private const float MIN_HP_THRESHOLD = 15f;
        private const float STAMINA_CRITICAL_THRESHOLD = 0.1f;
        private const float STAMINA_RESTORE_AMOUNT = 0.2f;
        private const float HINT_UPDATE_INTERVAL = 2f;

        private static readonly Dictionary<Player, CoroutineHandle> ActiveEffects = new();

        public override uint Id { get; set; } = 819;
        public override string Name { get; set; } = "Cardiac Overdrive";
        public override string Description { get; set; } = "Sacrifice your life force to run indefinitely (or until your heart stops)!";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        public CardiacOverdrive()
        {
            Buyable = false;
        }

        #region Event Registration

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
            Exiled.Events.Handlers.Player.Died += OnPlayerDied;
            Exiled.Events.Handlers.Player.Left += OnPlayerLeft;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
            Exiled.Events.Handlers.Player.Died -= OnPlayerDied;
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeft;
            base.UnsubscribeEvents();
        }

        #endregion

        #region Event Handlers

        private void OnUsingItem(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            float cooldownEndTime = ev.Player.GetCooldownItem(ItemType.AntiSCP207);
            if (cooldownEndTime > Time.timeSinceLevelLoad)
            {
                ev.IsAllowed = false;
                return;
            }
            
            ev.Player.SetCooldownItem(USE_DELAY, ItemType.AntiSCP207);
            
            if (IsEffectActive(ev.Player))
            {
                ShowEffectAlreadyActiveHint(ev.Player);
                ev.IsAllowed = false;
                return;
            }

            Timing.CallDelayed(USE_DELAY, () => ActivateEffect(ev.Player, ev.Item));
        }

        private void OnPlayerDied(DiedEventArgs ev)
        {
            if (IsEffectActive(ev.Player))
                DeactivateEffect(ev.Player);
        }

        private void OnPlayerLeft(LeftEventArgs ev)
        {
            if (IsEffectActive(ev.Player))
                DeactivateEffect(ev.Player);
        }

        #endregion

        #region Effect Management

        private void ActivateEffect(Player player, Item item)
        {
            if (!IsPlayerValid(player))
                return;

            item?.Destroy();

            var coroutine = Timing.RunCoroutine(CardiacOverdriveEffect(player));
            ActiveEffects[player] = coroutine;

            ShowActivationHint(player);
            Log.Debug($"[CardiacOverdrive] {player.Nickname} activated effect");
        }

        private IEnumerator<float> CardiacOverdriveEffect(Player player)
        {
            var elapsed = 0f;
            var lastHintTime = 0f;

            while (elapsed < EFFECT_DURATION && IsPlayerValid(player))
            {
                if (player.Stamina < STAMINA_CRITICAL_THRESHOLD)
                {
                    if (player.Health > MIN_HP_THRESHOLD)
                    {
                        RegenerateStaminaWithHealthCost(player, ref lastHintTime, elapsed);
                    }
                    else
                    {
                        ShowInsufficientHealthHint(player);
                        break;
                    }
                }

                elapsed += DRAIN_CHECK_INTERVAL;
                yield return Timing.WaitForSeconds(DRAIN_CHECK_INTERVAL);
            }

            DeactivateEffect(player);
        }

        private void DeactivateEffect(Player player)
        {
            if (player == null)
                return;

            if (ActiveEffects.TryGetValue(player, out var coroutine))
            {
                Timing.KillCoroutines(coroutine);
                ActiveEffects.Remove(player);
            }

            if (player.IsAlive)
                ShowDeactivationHint(player);

            Log.Debug($"[CardiacOverdrive] {player.Nickname} effect ended");
        }

        #endregion

        #region Helper Methods

        private static bool IsEffectActive(Player player)
        {
            return ActiveEffects.ContainsKey(player);
        }

        private static bool IsPlayerValid(Player player)
        {
            return player != null && player.IsAlive;
        }

        private void RegenerateStaminaWithHealthCost(Player player, ref float lastHintTime, float elapsed)
        {
            player.Stamina = STAMINA_RESTORE_AMOUNT;
            
            player.Health -= HP_DRAIN_PER_REGEN;
            
            if (ShouldUpdateHint(lastHintTime))
            {
                ShowStaminaRegenerationHint(player, elapsed);
                lastHintTime = Time.time;
            }

            Log.Debug($"[CardiacOverdrive] {player.Nickname} stamina regenerated using {HP_DRAIN_PER_REGEN} HP");
        }

        private static bool ShouldUpdateHint(float lastHintTime)
        {
            return Time.time - lastHintTime > HINT_UPDATE_INTERVAL;
        }

        #endregion

        #region UI Hints

        private static void ShowEffectAlreadyActiveHint(Player player)
        {
            player.ShowHint("<color=red>Cardiac Overdrive is already active!</color>", 2);
        }

        private static void ShowActivationHint(Player player)
        {
            player.ShowHint($"<color=red>Cardiac Overdrive activated for {EFFECT_DURATION}s!</color>", 3);
        }

        private void ShowStaminaRegenerationHint(Player player, float elapsed)
        {
            var remainingTime = EFFECT_DURATION - elapsed;
            player.ShowHint($"<color=red>💔 Using life force! HP: {player.Health:F0} | {remainingTime:F0}s left</color>", 2);
        }

        private static void ShowInsufficientHealthHint(Player player)
        {
            player.ShowHint("<color=red>Your heart gave out! Effect ended.</color>", 3);
        }

        private static void ShowDeactivationHint(Player player)
        {
            player.ShowHint("<color=yellow>Cardiac Overdrive effect ended.</color>", 2);
        }

        #endregion
    }
}