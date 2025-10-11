using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace GockelsAIO_exiled.Abilities.Active
{
    [CustomAbility]
    public class DoorPicking : ActiveAbility
    {
        public override string Name { get; set; } = "Door Picking Ability";
        public override string Description { get; set; } = "Allows you to open any door for a short period of time.";
        public override float Duration { get; set; } = 15f;
        public override float Cooldown { get; set; } = 90f;

        public float TimeMin { get; set; } = 6f;
        public float TimeMax { get; set; } = 12f;
        public float TimeForDoorToBeOpen { get; set; } = 5f;

        public string BeforePickingDoorText { get; set; } = "Interact with a door to start to pick it";
        public string PickingDoorText { get; set; } = "Picking door...";

        public Dictionary<EffectType, byte> Effects { get; set; } = new()
        {
            { EffectType.Ensnared, 1 },
            { EffectType.Slowness, 255 }
        };

        protected override void AbilityAdded(Player player)
        {
            SelectAbility(player);
            base.AbilityAdded(player);
        }

        protected override void AbilityUsed(Player player)
        {
            Exiled.Events.Handlers.Player.InteractingDoor += OnInteractingDoor;
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.InteractingDoor -= OnInteractingDoor;
            base.UnsubscribeEvents();
        }

        private void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (ev.Door.IsOpen || ev.Player.CurrentItem != null)
                return;

            ev.IsAllowed = false;

            int randomTime = Random.Range((int)TimeMin, (int)TimeMax);

            ev.Player.ShowHint(PickingDoorText, randomTime);

            foreach (var effect in Effects)
                ev.Player.EnableEffect(effect.Key, effect.Value, randomTime);

            Timing.CallDelayed(randomTime, () =>
            {
                if (ev.Door.Type == DoorType.Scp173Gate)
                {
                    ev.Player.ShowHint("This door can't physically be opened.");
                    return;
                }

                ev.Door.IsOpen = true;
                Timing.CallDelayed(TimeForDoorToBeOpen, () => ev.Door.IsOpen = false);
            });
        }
    }
}