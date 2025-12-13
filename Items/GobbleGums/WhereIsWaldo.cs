using System.Linq;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace GockelsAIO_exiled.Items.GobbleGums
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

            Timing.CallDelayed(USE_DELAY, () => ExecuteRandomFate(ev));
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

            if (Item.Create(ItemType.GrenadeHE, user) is not ExplosiveGrenade grenade)
            {
                Log.Error($"[WhereIsWaldo] Failed to create explosive grenade item");
                return;
            }

            if (grenade.Projectile?.Base == null)
            {
                Log.Error($"[WhereIsWaldo] Grenade projectile or base is null");
                return;
            }
            
            grenade.Projectile.Base._playerDamageOverDistance = 
                grenade.Projectile.Base._playerDamageOverDistance.Multiply(DamageMultiplier);
            grenade.Projectile.ScpDamageMultiplier = DamageMultiplier;

            grenade.FuseTime = GRENADE_FUSE_TIME;
            
            grenade.Projectile.Spawn(target.Position, shouldBeActive: true, previousOwner: user);

            Log.Debug($"[WhereIsWaldo] {user.Nickname} spawned grenade at {target.Nickname}'s position " +
                     $"(damage multiplier: {DamageMultiplier:F2}x for all targets)");
        }
    }
}
