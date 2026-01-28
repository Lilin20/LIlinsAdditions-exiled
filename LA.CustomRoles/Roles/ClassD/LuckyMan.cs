using Exiled.API.Enums;
using Exiled.API.Features.Spawn;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using System.Collections.Generic;
using VVUP.CustomRoles.API;
using YamlDotNet.Serialization;

namespace LilinsAdditions.CustomRoles.Roles.ClassD
{
    public class LuckyMan : CustomRole, ICustomRole
    {
        public override uint Id { get; set; } = 201;
        public StartTeam StartTeam { get; set; } = StartTeam.ClassD;
        public int Chance { get; set; } = 15;
        public override int MaxHealth { get; set; } = 100;
        public override string Name { get; set; } = "SCP-181 - Lucky Man";
        public override string Description { get; set; } = "You can open all doors for 3 minutes. And also dodge damage.";
        public override string CustomInfo { get; set; } = "SCP-181 - Lucky Man";
        public override RoleTypeId Role { get; set; } = RoleTypeId.ClassD;
        public List<EffectType> GoodEffects { get; set; } = new List<EffectType> { EffectType.MovementBoost, EffectType.Vitality, EffectType.Invigorated };
        public string OpenAllDoorsHint { get; set; } = "You can now open every door.";
        public override SpawnProperties SpawnProperties { get; set; } = new()
        {
            Limit = 1,
        };

        public bool _canOpenWithoutPerms = true;
        
        public override List<CustomAbility>? CustomAbilities { get; set; } = new()
        {
            new RestrictedEscape
            {
                Name = "Restricted Escape [Passive]",
                Description = "Restricts Escaping as SCP-181.",
                EscapeTextUncuffed = "As SCP-181, you cannot escape.",
                EscapeTextCuffed = "As SCP-181, you cannot escape.",
            },
        };
        
        [YamlIgnore] 
        public override float SpawnChance { get; set; } = 0;
        [YamlIgnore] 
        public override bool IgnoreSpawnSystem { get; set; } = true;

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
