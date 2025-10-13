using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.ThrowableProjectiles;
using Mirror;
using System.ComponentModel;
using UnityEngine;

namespace GockelsAIO_exiled.Items.Weapons.Grenade
{
    [CustomItem(ItemType.GrenadeHE)]
    public class StickyGrenade : Exiled.CustomItems.API.Features.CustomWeapon
    {
        public override uint Id { get; set; } = 1001;
        public override string Name { get; set; } = "Sticky Grenade";
        public override string Description { get; set; } = "A grenade that sticks to the first surface it touches";
        public override float Weight { get; set; } = 1.15f;
        public override ItemType Type { get; set; } = ItemType.GrenadeHE;
        public override SpawnProperties? SpawnProperties { get; set; } = null;

        [Description("How long the grenade waits before exploding after sticking")]
        public float StickyFuseTime { get; set; } = 5.0f;

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

            // Add our custom collision handler to make it sticky  
            var stickyHandler = ev.Projectile.GameObject.AddComponent<StickyCollisionHandler>();
            stickyHandler.Init(ev.Player.GameObject, ev.Projectile.Base, StickyFuseTime);
        }
    }

    public class StickyCollisionHandler : MonoBehaviour
    {
        private bool initialized;
        private bool hasStuck;
        private GameObject owner;
        private ThrownProjectile projectile;
        private float stickyFuseTime;

        public void Init(GameObject owner, ThrownProjectile projectile, float fuseTime)
        {
            this.owner = owner;
            this.projectile = projectile;
            this.stickyFuseTime = fuseTime;
            this.initialized = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            try
            {
                if (!initialized || hasStuck)
                    return;

                // Skip collision with owner and other grenades (same logic as CollisionHandler)  
                if (collision.collider.gameObject == owner ||
                    collision.collider.gameObject.TryGetComponent<EffectGrenade>(out _))
                    return;

                // Make it stick by stopping all movement  
                var rigidbody = projectile.GetComponent<Rigidbody>();
                if (rigidbody != null)
                {
                    rigidbody.velocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;
                    rigidbody.isKinematic = true;
                }

                // Set custom fuse time instead of immediate explosion  
                if (projectile is EffectGrenade effectGrenade)
                {
                    effectGrenade.TargetTime = NetworkTime.time + stickyFuseTime;
                }

                hasStuck = true;
            }
            catch (System.Exception ex)
            {
                Log.Error($"StickyCollisionHandler error: {ex}");
                Destroy(this);
            }
        }
    }
}
