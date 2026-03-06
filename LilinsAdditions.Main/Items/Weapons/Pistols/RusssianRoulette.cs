using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;
using Player = Exiled.Events.Handlers.Player;

namespace LilinsAdditions.Items.Weapons.Pistols
{
    [CustomItem(ItemType.GunRevolver)]
    public class RussianRoulette : CustomWeapon
    {
        private const float ROULETTE_DELAY = 0.5f;
        private const float HUMAN_SUCCESS_CHANCE = 0.65f;
        private const float SCP_SUCCESS_CHANCE = 0.05f;

        public override uint Id { get; set; } = 202;
        public override string Name { get; set; } = "Russian Roulette";
        public override string Description { get; set; } = "Let the fate decide.";
        public override byte ClipSize { get; set; } = 6;
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Player.Shot += OnShotPlayer;
            Player.Hurting += OnHurtingPlayer;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Player.Shot -= OnShotPlayer;
            Player.Hurting -= OnHurtingPlayer;
            base.UnsubscribeEvents();
        }

        private void OnHurtingPlayer(HurtingEventArgs ev)
        {
            if (ev.Player == null || ev.Attacker == null)
                return;

            if (!Check(ev.Attacker.CurrentItem))
                return;
            
            ev.Amount = 0f;
            ev.IsAllowed = false;

            base.OnHurting(ev);
        }

        private void OnShotPlayer(ShotEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            if (ev.Target == null)
            {
                Log.Debug($"[RussianRoulette] {ev.Player.Nickname} shot missed - no target");
                return;
            }

            ev.CanHurt = false;

            var roll = Random.value;
            Timing.CallDelayed(ROULETTE_DELAY, () => ResolveRoulette(ev.Player, ev.Target, roll));
        }

        private static void ResolveRoulette(Exiled.API.Features.Player shooter, Exiled.API.Features.Player target, float roll)
        {
            if (!ValidatePlayersForRoulette(shooter, target))
                return;

            var isScp = target.Role.Team == Team.SCPs;
            var successChance = isScp ? SCP_SUCCESS_CHANCE : HUMAN_SUCCESS_CHANCE;
            var isSuccess = roll <= successChance;

            if (isSuccess)
            {
                KillTarget(target, shooter);
                RewardShooter(shooter);
            }
            else
            {
                KillShooter(shooter, target);
            }
        }

        private static bool ValidatePlayersForRoulette(Exiled.API.Features.Player shooter, Exiled.API.Features.Player target)
        {
            if (shooter == null || !shooter.IsAlive)
            {
                Log.Debug($"[RussianRoulette] Shooter no longer valid for roulette");
                return false;
            }

            if (target == null || !target.IsAlive)
            {
                Log.Debug($"[RussianRoulette] Target no longer valid for roulette");
                return false;
            }

            return true;
        }

        private static void KillTarget(Exiled.API.Features.Player target, Exiled.API.Features.Player shooter)
        {
            target.Hurt(new UniversalDamageHandler(-1f, DeathTranslations.Unknown));
            
            var targetType = target.Role.Team == Team.SCPs ? "SCP" : "Human";
            Log.Debug($"[RussianRoulette] {shooter.Nickname} won roulette against {targetType} {target.Nickname}");
        }

        private static void RewardShooter(Exiled.API.Features.Player shooter)
        {
            shooter.AddItem(ItemType.Coin);
            Log.Debug($"[RussianRoulette] {shooter.Nickname} received coin reward");
        }

        private static void KillShooter(Exiled.API.Features.Player shooter, Exiled.API.Features.Player target)
        {
            shooter.Hurt(new UniversalDamageHandler(-1f, DeathTranslations.Unknown));
            
            var targetType = target.Role.Team == Team.SCPs ? "SCP" : "Human";
            Log.Debug($"[RussianRoulette] {shooter.Nickname} lost roulette against {targetType} {target.Nickname}");
        }
    }
}