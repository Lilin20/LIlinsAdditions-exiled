using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Roles;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class DeathCheat : CustomItem
    {
        public override uint Id { get; set; } = 804;
        public override string Name { get; set; } = "Death Cheat";
        public override string Description { get; set; } = "Belebt dich nach dem Tot wieder (vielleicht).";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        public Player oldPlayer = null;

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingDeathCheat;
            Exiled.Events.Handlers.Player.Dying += OnPlayerDeath;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingDeathCheat;
            Exiled.Events.Handlers.Player.Dying -= OnPlayerDeath;
            base.UnsubscribeEvents();
        }

        private void OnUsingDeathCheat(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            Timing.CallDelayed(2f, () =>
            {
                oldPlayer = ev.Player;

                ev.Item.Destroy();
            });
        }

        private void OnPlayerDeath(DyingEventArgs ev)
        {
            float random = UnityEngine.Random.value;
            if (random <= 0.8f)
            {
                if (oldPlayer != null)
                {
                    oldPlayer = ev.Player;
                    Role oldRole = oldPlayer.Role;

                    System.Random randomTime = new System.Random();
                    float randomValue = randomTime.Next(30, 121);

                    oldPlayer.ShowHint("Etwas greift nach deiner Seele...");

                    Timing.CallDelayed(randomValue, () =>
                    {
                        if (oldPlayer.Role is Exiled.API.Features.Roles.SpectatorRole)
                        {
                            oldPlayer.Role.Set(oldRole, RoleSpawnFlags.None);
                            oldPlayer.ShowHint("Wake the fuck up Samurai - We got SCPs to kill...");
                        }
                    });
                }
                oldPlayer = null;
            }
        }
    }
}
