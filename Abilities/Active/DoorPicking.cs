using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;

namespace GockelsAIO_exiled.Abilities.Active
{
    [CustomAbility]
    public class DoorPicking : ActiveAbility
    {
        public override string Name { get; set; } = "Door Picking Ability";
        public override string Description { get; set; } = "Allows you to open any door for a short period of time, but limited by some external factors";
        public override float Duration { get; set; } = 15f;
        public override float Cooldown { get; set; } = 90f;
        public float TimeToDoorPickMin { get; set; } = 6f;
        public float TimeToDoorPickMax { get; set; } = 12f;
        public float TimeForDoorToBeOpen { get; set; } = 5f;
        public string BeforePickingDoorText { get; set; } = "Interact with a door to start to pick it";
        public string PickingDoorText { get; set; } = "Picking door...";
        public Dictionary<EffectType, byte> EffectsToApply { get; set; } = new Dictionary<EffectType, byte>()
        {
            {EffectType.Ensnared, 1},
            {EffectType.Slowness, 255},
        };
        public List<Player> PlayersWithPickingDoorAbility = new List<Player>();

        protected override void AbilityAdded(Player player)
        {
            SelectAbility(player);
            base.AbilityAdded(player);
        }

        protected override void AbilityRemoved(Player player)
        {
            base.AbilityRemoved(player);
        }

        protected override void AbilityUsed(Player player)
        {
            player.ShowHint(BeforePickingDoorText, 5f);
            PlayersWithPickingDoorAbility.Add(player);
            Exiled.Events.Handlers.Player.InteractingDoor += OnInteractingDoor;
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.InteractingDoor -= OnInteractingDoor;
            base.UnsubscribeEvents();
        }

        private void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (!PlayersWithPickingDoorAbility.Contains(ev.Player))
                return;

            if (ev.Door.IsOpen)
                return;

            if (ev.Player.CurrentItem != null)
                return;

            Log.Debug("VVUP Custom Abilities: Door Picking Ability, processing methods");
            ev.IsAllowed = false;
            int randomTime = new Random().Next((int)TimeToDoorPickMin, (int)TimeToDoorPickMax);
            ev.Player.ShowHint(PickingDoorText, randomTime);
            foreach (var effect in EffectsToApply)
            {
                ev.Player.EnableEffect(effect.Key, effect.Value, randomTime);
            }

            Timing.CallDelayed(randomTime, () =>
            {
                if (ev.Door.Type == DoorType.Scp173Gate)
                {
                    ev.Player.ShowHint("Diese Tür ist physisch nicht zu öffnen.");
                    PlayersWithPickingDoorAbility.Remove(ev.Player);
                    return;
                }
                else
                {
                    Log.Debug($"VVUP Custom Abilities: Opening {ev.Door.Name}");
                    ev.Door.IsOpen = true;
                    PlayersWithPickingDoorAbility.Remove(ev.Player);
                    Timing.CallDelayed(TimeForDoorToBeOpen, () =>
                    {
                        ev.Door.IsOpen = false;
                    });
                }
            });
        }
    }
}
