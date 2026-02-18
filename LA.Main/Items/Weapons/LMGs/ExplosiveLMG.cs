using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using HarmonyLib;
using InventorySystem.Items.ThrowableProjectiles;
using UnityEngine;

namespace LilinsAdditions.Items.Weapons.LMGs
{
    [CustomItem(ItemType.GunLogicer)]
    public class ExplosiveLMG : CustomWeapon
    {
        private const float PLAYER_DAMAGE_MULTIPLIER = 0.01f;
        private const float DISABLE_EFFECT_MULTIPLIER = 0f;
        private const float NANO_ROCKET_FUSE_TIME = 0.01f;
        private const float SPAWN_HEIGHT_OFFSET = 0.1f;

        public override uint Id { get; set; } = 401;
        public override float Damage { get; set; } = 0.1f;
        public override string Name { get; set; } = "Prototype LMG - Nano Rockets";
        public override string Description { get; set; } = "Shoots nano rockets.";
        public override byte ClipSize { get; set; } = 200;
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shot += OnShotPlayer;
            Exiled.Events.Handlers.Player.Hurting += OnHurtingPlayer;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shot -= OnShotPlayer;
            Exiled.Events.Handlers.Player.Hurting -= OnHurtingPlayer;
            base.UnsubscribeEvents();
        }

        protected override void OnReloading(ReloadingWeaponEventArgs ev)
        {
            ev.IsAllowed = false;
            base.OnReloading(ev);
        }

        private void OnHurtingPlayer(HurtingEventArgs ev)
        {
            if (ev.Player == null || ev.Attacker == null)
                return;

            if (!Check(ev.Attacker.CurrentItem))
                return;

            if (ev.DamageHandler.Type == DamageType.Logicer)
            {
                ev.Player.ShowHitMarker();
                ev.Amount = 0f;
                ev.IsAllowed = false;
            }

            base.OnHurting(ev);
        }

        private void OnShotPlayer(ShotEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            SpawnNanoRocket(ev.Player, ev.Position);
        }

        private static void SpawnNanoRocket(Player shooter, Vector3 position)
        {
            if (Item.Create(ItemType.GrenadeHE, shooter) is not ExplosiveGrenade grenade)
            {
                Log.Error($"[ExplosiveLMG] Failed to create nano rocket for {shooter.Nickname}");
                return;
            }

            if (grenade.Projectile?.Base == null)
            {
                Log.Error($"[ExplosiveLMG] Grenade projectile or base is null");
                return;
            }

            ConfigureNanoRocket(grenade);
            
            var spawnPosition = position + (Vector3.up * SPAWN_HEIGHT_OFFSET);
            grenade.SpawnActive(spawnPosition);

            Log.Debug($"[ExplosiveLMG] {shooter.Nickname} fired nano rocket at {spawnPosition}");
        }

        private static void ConfigureNanoRocket(ExplosiveGrenade grenade)
        {
            var projectileBase = grenade.Projectile.Base;
            var type = typeof(ExplosionGrenade);
            
            var playerDamageField = AccessTools.Field(type, nameof(ExplosionGrenade._playerDamageOverDistance));
            var burnDurationField = AccessTools.Field(type, nameof(ExplosionGrenade._burnedDuration));
            var concussedDurationField = AccessTools.Field(type, nameof(ExplosionGrenade._concussedDuration));
            var deafenedDurationField = AccessTools.Field(type, nameof(ExplosionGrenade._deafenedDuration));
            var shakeField = AccessTools.Field(type, nameof(ExplosionGrenade._shakeOverDistance));
            var doorDamageField = AccessTools.Field(type, nameof(ExplosionGrenade._doorDamageOverDistance));
            var effectDurationField = AccessTools.Field(type, nameof(ExplosionGrenade._effectDurationOverDistance));
            
            var currentDamage = (float)playerDamageField.GetValue(projectileBase);
            playerDamageField.SetValue(projectileBase, currentDamage * PLAYER_DAMAGE_MULTIPLIER);
            
            burnDurationField.SetValue(projectileBase, 0f);
            concussedDurationField.SetValue(projectileBase, 0f);
            deafenedDurationField.SetValue(projectileBase, 0f);
            
            var currentShake = (float)shakeField.GetValue(projectileBase);
            shakeField.SetValue(projectileBase, currentShake * DISABLE_EFFECT_MULTIPLIER);
            
            var currentDoorDamage = (float)doorDamageField.GetValue(projectileBase);
            doorDamageField.SetValue(projectileBase, currentDoorDamage * DISABLE_EFFECT_MULTIPLIER);
            
            var currentEffectDuration = (float)effectDurationField.GetValue(projectileBase);
            effectDurationField.SetValue(projectileBase, currentEffectDuration * DISABLE_EFFECT_MULTIPLIER);
            
            grenade.FuseTime = NANO_ROCKET_FUSE_TIME;
        }
    }
}