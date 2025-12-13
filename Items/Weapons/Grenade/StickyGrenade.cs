using System.ComponentModel;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.ThrowableProjectiles;
using Mirror;
using UnityEngine;

namespace GockelsAIO_exiled.Items.Weapons.Grenade
{
    [CustomItem(ItemType.GrenadeHE)]
    public class StickyGrenade : Exiled.CustomItems.API.Features.CustomWeapon
    {
        private const float DEFAULT_STICKY_FUSE_TIME = 5.0f;

        public override uint Id { get; set; } = 1001;
        public override string Name { get; set; } = "Sticky Grenade";
        public override string Description { get; set; } = "A grenade that sticks to the first surface it touches";
        public override float Weight { get; set; } = 1.15f;
        public override ItemType Type { get; set; } = ItemType.GrenadeHE;
        public override SpawnProperties? SpawnProperties { get; set; } = null;

        [Description("How long the grenade waits before exploding after sticking")]
        public float StickyFuseTime { get; set; } = DEFAULT_STICKY_FUSE_TIME;

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ThrownProjectile += OnThrownProjectile;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ThrownProjectile -= OnThrownProjectile;
            base.UnsubscribeEvents();
        }

        private void OnThrownProjectile(ThrownProjectileEventArgs ev)
        {
            if (!Check(ev.Item))
                return;

            if (ev.Projectile?.GameObject == null)
            {
                Log.Warn($"[StickyGrenade] Projectile or GameObject is null for {ev.Player.Nickname}");
                return;
            }

            var stickyHandler = ev.Projectile.GameObject.AddComponent<StickyCollisionHandler>();
            stickyHandler.Init(ev.Player.GameObject, ev.Projectile.Base, StickyFuseTime);

            Log.Debug($"[StickyGrenade] {ev.Player.Nickname} threw sticky grenade (fuse: {StickyFuseTime}s)");
        }
    }

    public class StickyCollisionHandler : MonoBehaviour
    {
        private bool _initialized;
        private bool _hasStuck;
        private GameObject _ownerObject;
        private ThrownProjectile _projectile;
        private float _stickyFuseTime;

        public void Init(GameObject ownerObject, ThrownProjectile projectile, float fuseTime)
        {
            _ownerObject = ownerObject;
            _projectile = projectile;
            _stickyFuseTime = fuseTime;
            _initialized = true;

            Log.Debug($"[StickyCollisionHandler] Initialized with fuse time: {fuseTime}s");
        }

        private void OnCollisionEnter(Collision collision)
        {
            try
            {
                if (!_initialized || _hasStuck)
                    return;

                if (!ShouldStickToCollision(collision))
                    return;

                MakeProjectileStick();
                SetCustomFuseTime();

                _hasStuck = true;

                Log.Debug($"[StickyCollisionHandler] Grenade stuck to {collision.collider.gameObject.name}");
            }
            catch (System.Exception ex)
            {
                Log.Error($"[StickyCollisionHandler] Error in OnCollisionEnter: {ex}");
                Destroy(this);
            }
        }

        private bool ShouldStickToCollision(Collision collision)
        {
            if (collision?.collider?.gameObject == null)
                return false;

            // Don't stick to owner
            if (collision.collider.gameObject == _ownerObject)
                return false;

            // Don't stick to other grenades
            if (collision.collider.gameObject.TryGetComponent<EffectGrenade>(out _))
                return false;

            return true;
        }

        private void MakeProjectileStick()
        {
            if (_projectile == null)
                return;

            var rigidbody = _projectile.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                Log.Warn("[StickyCollisionHandler] Rigidbody component not found");
                return;
            }

            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.isKinematic = true;
        }

        private void SetCustomFuseTime()
        {
            if (_projectile is not EffectGrenade effectGrenade)
            {
                Log.Warn("[StickyCollisionHandler] Projectile is not an EffectGrenade");
                return;
            }

            effectGrenade.TargetTime = NetworkTime.time + _stickyFuseTime;
        }

        private void OnDestroy()
        {
            Log.Debug("[StickyCollisionHandler] Component destroyed");
        }
    }
}