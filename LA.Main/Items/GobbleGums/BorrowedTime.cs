using System.Collections.Generic;
using Exiled.API.Enums;
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
    public class BorrowedTime : FortunaFizzItem
    {
        private readonly Dictionary<Player, float> _storedDamage = new();
        
        public override uint Id { get; set; } = 820;
        public override string Name { get; set; } = "Borrowed Time";
        public override string Description { get; set; } = "Store damage for 10s, then take half. Exceed 300 and explode!";
        public override float Weight { get; set; } = 0.5f;
        public float EffectDuration { get; set; } = 10f;
        public float ExplosionThreshold { get; set; } = 300f;
        public float GrenadeFuseTime { get; set; } = 0.1f;
        public override SpawnProperties SpawnProperties { get; set; }
        
        public BorrowedTime()
        {
            Buyable = true;
        }
        
        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
            Exiled.Events.Handlers.Player.Hurting += OnHurting;
            Exiled.Events.Handlers.Player.Left += OnPlayerLeft;
            base.SubscribeEvents();
        }
        
        protected override void UnsubscribeEvents()  
        {  
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
            Exiled.Events.Handlers.Player.Hurting -= OnHurting;
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeft;
            base.UnsubscribeEvents();
        }
        
        private void OnUsingItem(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;
            
            ev.IsAllowed = false;
            
            ActivateBorrowedTime(ev);
        }
        
        private void ActivateBorrowedTime(UsingItemEventArgs ev)
        {
            if (ev.Player == null || !ev.Player.IsAlive)
                return;
            
            _storedDamage[ev.Player] = 0f;
            ev.Player.ShowHint("Borrowed Time activated! Damage stored for 10s.");
            ev.Item?.Destroy();
  
            Timing.CallDelayed(EffectDuration, () => EndBorrowedTime(ev.Player));
            Log.Debug($"[BorrowedTime] {ev.Player.Nickname} activated damage storage");
        }
        
        private void OnHurting(HurtingEventArgs ev)
        {
            if (!_storedDamage.TryGetValue(ev.Player, out var currentDamage))
                return;
            
            _storedDamage[ev.Player] = currentDamage + ev.Amount;
            ev.IsAllowed = false;
            
            Log.Debug($"[BorrowedTime] {ev.Player.Nickname} stored {ev.Amount} damage (total: {_storedDamage[ev.Player]})");
        }
        
        private void EndBorrowedTime(Player player)
        {
            if (!_storedDamage.TryGetValue(player, out var totalDamage))
                return;
            
            _storedDamage.Remove(player);
            
            var finalDamage = totalDamage / 2f;
            
            if (finalDamage > ExplosionThreshold)
            {
                player.ShowHint("The borrowed time has run out... explosively!");
                SpawnExplosion(player);
            }
            else
            {
                player.ShowHint($"Borrowed Time ended! Taking {finalDamage:F0} damage.");
                player.Hurt(finalDamage);
            }
            
            Log.Debug($"[BorrowedTime] {player.Nickname} effect ended - final damage: {finalDamage}");
        }
        
        private void SpawnExplosion(Player player)
        {
            var grenade = Projectile.CreateAndSpawn(
                ProjectileType.FragGrenade,
                player.Position,
                rotation: Quaternion.identity,
                shouldBeActive: true
            ) as ExplosionGrenadeProjectile;
            
            if (grenade == null)
            {
                Log.Error($"[WhereIsWaldo] Failed to create explosion grenade projectile");
                return;
            }
            
            grenade.FuseTime = GrenadeFuseTime;
            
            Log.Debug($"[BorrowedTime] {player.Nickname} exploded from stored damage!");
        }
        
        private void OnPlayerLeft(LeftEventArgs ev)
        {
            _storedDamage.Remove(ev.Player);
        } 
    }
};