using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using PlayerHandlers = Exiled.Events.Handlers.Player;

namespace LilinsAdditions.Items.Weapons.SMGs
{
    [CustomItem(ItemType.GunCrossvec)]
    public class MedicGun : CustomWeapon
    {
        public override uint Id { get; set; } = 500;
        public override string Name { get; set; } = "MS9K - MedShot 9000";
        public override string Description { get; set; } = "A tool to heal entities.";
        public override byte ClipSize { get; set; } = 60;
        public override float Weight { get; set; } = 0.5f;
        public override float Damage { get; set; } = 0f;
        public override SpawnProperties SpawnProperties { get; set; }
        
        private const float HealAmount = 5f;

        protected override void SubscribeEvents()
        {
            PlayerHandlers.Shot += OnShotPlayer;
            PlayerHandlers.Hurting += OnHurtingPlayer;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            PlayerHandlers.Shot -= OnShotPlayer;
            PlayerHandlers.Hurting -= OnHurtingPlayer;
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
        }

        private void OnShotPlayer(ShotEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem) || ev.Target == null)
                return;

            ev.CanHurt = false;

            if (!IsScpShooter(ev.Player))
            {
                HealTarget(ev.Target);
            }
        }

        private bool IsScpShooter(Exiled.API.Features.Player player)
        {
            return player.Role.Team == Team.SCPs;
        }

        private void HealTarget(Exiled.API.Features.Player target)
        {
            target.Heal(HealAmount);
        }
    }
}