using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace GockelsAIO_exiled.Items.Weapons.Pistols
{
    [CustomItem(ItemType.GunRevolver)]
    public class Behemoth : CustomWeapon
    {
        public override uint Id { get; set; } = 203;
        public override string Name { get; set; } = "Behemoth 50.cal";
        public override string Description { get; set; } = "A very deadly revolver. Can shoot through walls.";
        public override byte ClipSize { get; set; } = 6;
        public override float Weight { get; set; } = 0.5f;
        public float DamagingDistance { get; set; } = 100f;
        public float DamageOverride { get; set; } = 80f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shooting += OnFiring;
            Exiled.Events.Handlers.Player.Hurting += PlayerHurting;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Shooting -= OnFiring;
            Exiled.Events.Handlers.Player.Hurting -= PlayerHurting;
            base.UnsubscribeEvents();
        }

        public void PlayerHurting(HurtingEventArgs ev)
        {
            if (ev.Player is null) return;
            if (ev.Attacker is null) return;
            if (!Check(ev.Attacker.CurrentItem)) return;

            ev.Amount = 0f;
            ev.IsAllowed = false;

            base.OnHurting(ev);
        }

        private void OnFiring(ShootingEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            Vector3 origin = ev.Player.CameraTransform.position;
            Vector3 direction = ev.Player.CameraTransform.forward;

            // RaycastAll holt alle Treffer entlang der Strecke, auch hinter Wänden
            int layer13Mask = 1 << 13;
            RaycastHit[] hits = Physics.RaycastAll(origin, direction, DamagingDistance, layer13Mask);

            foreach (RaycastHit hit in hits)
            {
                try
                {
                    var target = Exiled.API.Features.Player.Get(hit.collider);
                    if (target != null && target.IsAlive && target != ev.Player)
                    {
                        ev.Player.ShowHitMarker();
                        Timing.CallDelayed(0.25f, () =>
                        {
                            target.Hurt(DamageOverride);
                        });
                        break;
                    }
                }
                catch
                {
                    // Optional: Log oder ignorieren
                }
            }
        }
    }
}
