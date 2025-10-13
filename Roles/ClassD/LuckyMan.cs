using Exiled.API.Enums;
using Exiled.API.Features.Spawn;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using System.Collections.Generic;

namespace GockelsAIO_exiled.Roles.ClassD
{
    public class LuckyMan : CustomRole
    {
        public override uint Id { get; set; } = 201;
        public override int MaxHealth { get; set; } = 100;
        public override string Name { get; set; } = "SCP-181 - Lucky Man";
        public override string Description { get; set; } = "You can open all doors for 3 minutes. And also dodge damage.";
        public override string CustomInfo { get; set; } = "SCP-181 - Lucky Man";
        public override RoleTypeId Role { get; set; } = RoleTypeId.ClassD;
        public int Chance { get; set; } = 15;
        public override List<CustomAbility> CustomAbilities { get; set; }
        public List<EffectType> GoodEffects { get; set; } = new List<EffectType> { EffectType.MovementBoost, EffectType.Vitality, EffectType.Invigorated };

        public string OpenAllDoorsHint { get; set; } = "Du kannst nun alle Türen öffnen.";
        public override SpawnProperties SpawnProperties { get; set; } = new()
        {
            Limit = 1,
            RoleSpawnPoints =
            [
                new()
                {
                    Role = RoleTypeId.ClassD,
                }
            ]
        };

        public bool _canOpenWithoutPerms = true;

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Hurting += OnHurt;
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Player.InteractingDoor += OnDoorOpening;
            Exiled.Events.Handlers.Player.TriggeringTesla += OnTeslaTrigger;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Hurting -= OnHurt;
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Exiled.Events.Handlers.Player.InteractingDoor -= OnDoorOpening;
            Exiled.Events.Handlers.Player.TriggeringTesla -= OnTeslaTrigger;
            base.UnsubscribeEvents();
        }

        public void OnEscape(EscapingEventArgs ev)
        {
            ev.IsAllowed = false;
        }

        public void OnHurt(HurtingEventArgs ev)
        {
            if (!Check(ev.Player))
                return;

            if (ev.Player == null)
                return;

            float random = UnityEngine.Random.value;
            if (random >= 0.4f)
            {
                ev.DamageHandler.Damage = 0;
                System.Random randomEffectIndex = new System.Random();

                EffectType randomEffect = GoodEffects[randomEffectIndex.Next(GoodEffects.Count)];
                ev.Player.EnableEffect(randomEffect, 20, 2f);
            }
        }

        public void OnTeslaTrigger(TriggeringTeslaEventArgs ev)
        {
            if (!Check(ev.Player))
                return;

            ev.IsAllowed = false;
        }

        public void OnSpawned(SpawnedEventArgs ev)
        {
            if (!Check(ev.Player))
                return;

            _canOpenWithoutPerms = false;

            Timing.CallDelayed(180f, () =>
            {
                _canOpenWithoutPerms = true;
                ev.Player.ShowHint(OpenAllDoorsHint);
            });
        }

        public void OnDoorOpening(InteractingDoorEventArgs ev)
        {
            if (!Check(ev.Player))
                return;

            if (_canOpenWithoutPerms && ev.Door.IsKeycardDoor)
            {
                ev.Door.IsOpen = true;
            }
        }
    }
}
