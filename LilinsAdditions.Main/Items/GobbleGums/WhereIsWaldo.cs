using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Pickups.Projectiles;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace LilinsAdditions.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class WhereIsWaldo : FortunaFizzItem
    {
        private const float USE_DELAY = 2f;
        private const float EXPLOSION_DELAY = 2f;
        private const float EXPLOSION_CHANCE = 0.25f;
        private const float DEFAULT_DAMAGE_MULTIPLIER = 1.0f;
        private const string DEFAULT_SUCCESS_HINT = "Bye Bye!";
        private const string DEFAULT_FAILURE_HINT = "Hm...";
        private const float GRENADE_FUSE_TIME = 0.1f;

        public override uint Id { get; set; } = 808;
        public override string Name { get; set; } = "Bye Bye Buddy";
        public override string Description { get; set; } = "Fate draws only one number today (or is it?)";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        public float DamageMultiplier { get; set; } = DEFAULT_DAMAGE_MULTIPLIER;
        public string ByeByeHint { get; set; } = DEFAULT_SUCCESS_HINT;
        public string HmHint { get; set; } = DEFAULT_FAILURE_HINT;

        public WhereIsWaldo()
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

            ExecuteRandomFate(ev);
        }

        private void ExecuteRandomFate(UsingItemEventArgs ev)
        {
            if (ev.Player == null || !ev.Player.IsAlive)
                return;

            var alivePlayers = Player.List.Where(p => p.IsAlive).ToList();
            if (alivePlayers.Count == 0)
            {
                Log.Warn($"[WhereIsWaldo] No alive players found for {ev.Player.Nickname}");
                ev.Item?.Destroy();
                return;
            }

            var targetPlayer = alivePlayers.GetRandomValue();
            var willExplode = Random.value <= EXPLOSION_CHANCE;

            if (willExplode)
            {
                targetPlayer.ShowHint(ByeByeHint);
                Timing.CallDelayed(EXPLOSION_DELAY, () => SpawnExplosionGrenade(targetPlayer, ev.Player));
            }
            else
            {
                ev.Player.ShowHint(HmHint);
                Log.Debug($"[WhereIsWaldo] {ev.Player.Nickname} failed - no explosion");
            }

            ev.Item?.Destroy();
        }

        private void SpawnExplosionGrenade(Player target, Player user)
        {
            if (target == null || !target.IsAlive)
            {
                Log.Debug($"[WhereIsWaldo] Target no longer alive, skipping grenade spawn");
                return;
            }
            
            var grenade = Projectile.CreateAndSpawn(
                ProjectileType.FragGrenade,
                target.Position,
                rotation: Quaternion.identity,
                shouldBeActive: true,
                previousOwner: user
            ) as ExplosionGrenadeProjectile;
            
            if (grenade == null)
            {
                Log.Error($"[WhereIsWaldo] Failed to create explosion grenade projectile");
                return;
            }
            
            grenade.Base._playerDamageOverDistance =
                grenade.Base._playerDamageOverDistance.Multiply(DamageMultiplier);
            grenade.ScpDamageMultiplier = DamageMultiplier;
            grenade.FuseTime = GRENADE_FUSE_TIME;
            
            Log.Debug($"[WhereIsWaldo] {user.Nickname} spawned grenade at {target.Nickname}'s position " +
                      $"(damage multiplier: {DamageMultiplier:F2}x for all targets)");
        }
    }
}
