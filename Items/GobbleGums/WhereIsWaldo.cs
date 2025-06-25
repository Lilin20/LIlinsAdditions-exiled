using System.Linq;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;

namespace GockelsAIO_exiled.Items.GobbleGums
{
    [CustomItem(ItemType.AntiSCP207)]
    public class WhereIsWaldo : CustomItem
    {
        public override uint Id { get; set; } = 808;
        public override string Name { get; set; } = "Bye Bye Buddy";
        public override string Description { get; set; } = "Das Schicksal zieht heute nur eine Nummer.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; }

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem += OnUsingWhereIsWaldo;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingWhereIsWaldo;
            base.UnsubscribeEvents();
        }

        private void OnUsingWhereIsWaldo(UsingItemEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem)) return;

            Timing.CallDelayed(2f, () =>
            {
                Player randomPlayer = Player.List.Where(p => p.IsAlive).GetRandomValue();

                float random = UnityEngine.Random.value;
                if (random <= 0.25f)
                {
                    randomPlayer.ShowHint("Bye Bye!");

                    Timing.CallDelayed(2f, () =>
                    {
                        randomPlayer.Explode();
                    });

                }
                else
                {
                    ev.Player.ShowHint("Hm...");
                }

                ev.Item.Destroy();
            });
        }
    }
}
