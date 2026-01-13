using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
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
        public override string Description { get; set; } = "Kill your targets fast and easy.";
        public override float Damage { get; set; } = 95;
        public override byte ClipSize { get; set; } = 1;
        public override float Weight { get; set; } = 1.5f;
        public override SpawnProperties SpawnProperties { get; set; }
        public override AttachmentName[] Attachments { get; set; } = new[] { AttachmentName.ScopeSight };
        
        private const float CloseRangeThreshold = 8f;
        private const float LongRangeBodyDamage = 55f;
        private const float ScpDamage = 200f;

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ActivatingWorkstation += OnActivatingWorkstation;
            Exiled.Events.Handlers.Item.ChangingAttachments += OnChangingAttachments;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ActivatingWorkstation -= OnActivatingWorkstation;
            Exiled.Events.Handlers.Item.ChangingAttachments -= OnChangingAttachments;
            base.UnsubscribeEvents();
        }

        private void OnChangingAttachments(ChangingAttachmentsEventArgs ev)
        {
            if (Check(ev.Player.CurrentItem))
            {
                ev.IsAllowed = false;
            }
        }

        private void OnActivatingWorkstation(ActivatingWorkstationEventArgs ev)
        {
            if (Check(ev.Player))
            {
                ev.IsAllowed = false;
            }
        }

        protected override void OnShot(ShotEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem) || ev.Target == null)
                return;

            ev.CanHurt = false;
            ev.Player.ShowHitMarker();

            ApplyDamage(ev);
        }

        private void ApplyDamage(ShotEventArgs ev)
        {
            if (ev.Target.Role.Team == PlayerRoles.Team.SCPs)
            {
                ApplyScpDamage(ev.Target);
            }
            else
            {
                ApplyHumanDamage(ev);
            }
        }

        private void ApplyHumanDamage(ShotEventArgs ev)
        {
            if (IsHeadshot(ev))
            {
                KillTarget(ev.Target);
                return;
            }

            if (IsCloseRange(ev.Distance))
            {
                KillTarget(ev.Target);
            }
            else
            {
                ev.Target.Hurt(LongRangeBodyDamage);
            }
        }

        private void ApplyScpDamage(Exiled.API.Features.Player target)
        {
            target.Hurt(ScpDamage);
        }

        private void KillTarget(Exiled.API.Features.Player target)
        {
            target.Kill(new UniversalDamageHandler(-1f, DeathTranslations.BulletWounds));
        }

        private bool IsHeadshot(ShotEventArgs ev)
        {
            return ev.Hitbox.HitboxType == HitboxType.Headshot;
        }

        private bool IsCloseRange(float distance)
        {
            return distance < CloseRangeThreshold;
        }
    }
}