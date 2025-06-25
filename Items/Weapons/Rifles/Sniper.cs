using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Item;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.Firearms.Attachments;
using PlayerStatsSystem;

namespace GockelsAIO_exiled.Items.Weapons.Rifles
{
    [CustomItem(ItemType.GunE11SR)]
    public class Sniper : CustomWeapon
    {
        public override uint Id { get; set; } = 101;
        public override string Name { get; set; } = "RangeTec - .308 Lapua";
        public override string Description { get; set; } = "Schaltet Ziele sauber und schnell aus.";
        public override float Damage { get; set; } = 95;
        public override byte ClipSize { get; set; } = 1;
        public override float Weight { get; set; } = 1.5f;

        public override SpawnProperties SpawnProperties { get; set; }
        public override AttachmentName[] Attachments { get; set; } = new[]
        {
            AttachmentName.ScopeSight,
        };
        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ActivatingWorkstation += OnModify;
            Exiled.Events.Handlers.Item.ChangingAttachments += OnAttachmentChange;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ActivatingWorkstation -= OnModify;
            Exiled.Events.Handlers.Item.ChangingAttachments -= OnAttachmentChange;
            base.UnsubscribeEvents();
        }

        protected override void OnShooting(ShootingEventArgs ev)
        {
            ev.Firearm.Recoil = new CameraShaking.RecoilSettings(2, 5, 10, 10, 0);
        }

        public void OnAttachmentChange(ChangingAttachmentsEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
            {
                return;
            }

            ev.IsAllowed = false;
        }

        public void OnModify(ActivatingWorkstationEventArgs ev)
        {
            if (!Check(ev.Player))
            {
                return;
            }

            ev.IsAllowed = false;
        }

        protected override void OnPickingUp(PickingUpItemEventArgs ev)
        {
            if (!CustomRole.Get(65).Check(ev.Player))
            {
                ev.IsAllowed = false;
                ev.Player.Hurt(1);

                ev.Player.ShowHint("Ein Sicherheitsmechanismus greift ein und gibt dir einen Stromschlag.");
            }
        }

        protected override void OnShot(ShotEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            if (ev.Target == null) return;

            ev.CanHurt = false;
            ev.Player.ShowHitMarker();

            if (ev.Target.Role.Team != PlayerRoles.Team.SCPs)
            {
                if (ev.Distance < 8f && ev.Hitbox.HitboxType != HitboxType.Headshot)
                {
                    ev.Target.Kill(new UniversalDamageHandler(-1f, DeathTranslations.BulletWounds));
                }
                else if (ev.Distance >= 8f && ev.Hitbox.HitboxType != HitboxType.Headshot)
                {
                    ev.Target.Hurt(55);
                }
                if (ev.Hitbox.HitboxType == HitboxType.Headshot)
                {
                    ev.Target.Kill(new UniversalDamageHandler(-1f, DeathTranslations.BulletWounds));
                }
            }
            else if (ev.Target.Role.Team == PlayerRoles.Team.SCPs)
            {
                ev.Target.Hurt(200);
            }

        }
    }
}
