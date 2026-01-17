using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class ChemicalCocktail : FortunaFizzItem
    {
        private const float USE_DELAY = 2f;
        private const float ZOMBIE_CHANCE = 0.33f;
        private const float DEATH_CHANCE = 0.66f;
        private const float ZOMBIE_HEALTH = 100f;
        
        private const byte ZOMBIE_SLOWNESS_DURATION = 200;
        private const byte ZOMBIE_CONCUSSED_DURATION = 60;
        private const byte DEBUFF_SLOWNESS_DURATION = 200;
        private const byte DEBUFF_REDUCTION_DURATION = 150;
        private const byte DEBUFF_AMNESIA_DURATION = 100;
        private const byte DEBUFF_BLINDED_DURATION = 90;

        public override uint Id { get; set; } = 806;
        public override string Name { get; set; } = "Chemical Cocktail";
        public override string Description { get; set; } = "Well, you should never take that.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        public ChemicalCocktail()
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

            float cooldownEndTime = ev.Player.GetCooldownItem(ItemType.AntiSCP207);
            if (cooldownEndTime > Time.timeSinceLevelLoad)
            {
                ev.IsAllowed = false;
                return;
            }
            
            ev.Player.SetCooldownItem(USE_DELAY, ItemType.AntiSCP207);
            
            Timing.CallDelayed(USE_DELAY, () => ApplyRandomEffect(ev));
        }

        private static void ApplyRandomEffect(UsingItemEventArgs ev)
        {
            if (ev.Player == null || !ev.Player.IsAlive)
                return;

            var roll = Random.value;

            if (roll <= ZOMBIE_CHANCE)
                ApplyZombieTransformation(ev.Player);
            else if (roll <= DEATH_CHANCE)
                ev.Player.Vaporize();
            else
                ApplyDebuffEffects(ev.Player);

            ev.Item?.Destroy();
        }

        private static void ApplyZombieTransformation(Player player)
        {
            player.Role.Set(RoleTypeId.Scp0492, RoleSpawnFlags.None);
            player.MaxHealth = ZOMBIE_HEALTH;
            player.Health = ZOMBIE_HEALTH;
            player.EnableEffect(EffectType.Slowness, ZOMBIE_SLOWNESS_DURATION);
            player.EnableEffect(EffectType.Concussed, ZOMBIE_CONCUSSED_DURATION);
        }

        private static void ApplyDebuffEffects(Player player)
        {
            player.EnableEffect(EffectType.Slowness, DEBUFF_SLOWNESS_DURATION);
            player.EnableEffect(EffectType.DamageReduction, DEBUFF_REDUCTION_DURATION);
            player.EnableEffect(EffectType.AmnesiaItems, DEBUFF_AMNESIA_DURATION);
            player.EnableEffect(EffectType.Blinded, DEBUFF_BLINDED_DURATION);
        }
    }
}
