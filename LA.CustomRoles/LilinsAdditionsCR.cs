using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles;
using Exiled.CustomRoles.API;
using Exiled.CustomRoles.API.Features;
using LilinsAdditions.CustomRoles.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using VVUP.CustomRoles.API;

namespace LilinsAdditions.CustomRoles
{
    public class LilinsAdditionsCR : Plugin<Config>
    {
        public override string Name => "Lilin's Additions - Custom Roles";
        public override string Author => "Lilin";
        public override Version Version => new Version(0, 1);
        public override PluginPriority Priority => PluginPriority.Lowest;
        public static LilinsAdditionsCR Instance;
        public static PlayerHandlers PlayerHandlers;
        public Dictionary<StartTeam, List<ICustomRole>> Roles { get; } = new();

        public override void OnEnabled()
        {
            Instance = this;
            PlayerHandlers = new PlayerHandlers();

            RegisterPlayerHandlers();
            LoadAudioClips();

            HashSet<CustomRole> existingRoles = new HashSet<CustomRole>(CustomRole.Registered);
            HashSet<CustomAbility> existingAbilities = new HashSet<CustomAbility>(CustomAbility.Registered);
            Config.riotOperator.Register();
            Config.kamikazeZombie.Register();
            Config.luckyMan.Register();
            Config.thief.Register();
            foreach (CustomRole role in CustomRole.Registered)
            {
                if (role.CustomAbilities is not null)
                {
                    foreach (var ability in role.CustomAbilities.Where(ability => !existingAbilities.Contains(ability)))
                    {
                        Log.Debug($"LA CR: Registering ability {ability.Name}");
                        ability.Register();
                    }
                }
                if (!existingRoles.Contains(role) && role is ICustomRole custom)
                {
                    Log.Debug($"LA CR: Adding {role.Name} to dictionary..");
                    StartTeam team = custom.StartTeam switch
                    {
                        var t when t.HasFlag(StartTeam.Chaos) => StartTeam.Chaos,
                        var t when t.HasFlag(StartTeam.Guard) => StartTeam.Guard,
                        var t when t.HasFlag(StartTeam.Ntf) => StartTeam.Ntf,
                        var t when t.HasFlag(StartTeam.Scientist) => StartTeam.Scientist,
                        var t when t.HasFlag(StartTeam.ClassD) => StartTeam.ClassD,
                        var t when t.HasFlag(StartTeam.Scp) => StartTeam.Scp,
                        var t when t.HasFlag(StartTeam.Scp049) => StartTeam.Scp049,
                        var t when t.HasFlag(StartTeam.Scp079) => StartTeam.Scp079,
                        var t when t.HasFlag(StartTeam.Scp096) => StartTeam.Scp096,
                        var t when t.HasFlag(StartTeam.Scp106) => StartTeam.Scp106,
                        var t when t.HasFlag(StartTeam.Scp173) => StartTeam.Scp173,
                        var t when t.HasFlag(StartTeam.Scp939) => StartTeam.Scp939,
                        var t when t.HasFlag(StartTeam.Scp3114) => StartTeam.Scp3114,
                        _ => StartTeam.Other
                    };

                    if (!VVUP.CustomRoles.Plugin.Instance.Roles.ContainsKey(team))
                        VVUP.CustomRoles.Plugin.Instance.Roles.Add(team, new());

                    for (int i = 0; i < role.SpawnProperties.Limit; i++)
                        VVUP.CustomRoles.Plugin.Instance.Roles[team].Add(custom);
                    Log.Debug($"LA CR: Roles {team} now has {VVUP.CustomRoles.Plugin.Instance.Roles[team].Count} elements.");
                }
            }

            existingRoles.Clear();
            existingAbilities.Clear();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            CustomRole.UnregisterRoles();
            UnregisterPlayerHandlers();

            PlayerHandlers = null;
            Instance = null;
            base.OnDisabled();
        }

        public void LoadAudioClips()
        {
            bool bombsoundLoaded = AudioClipStorage.LoadClip(Instance.Config.BurstSoundPath, "bombsound");
        }

        public void RegisterPlayerHandlers()
        {
            Exiled.Events.Handlers.Player.ChangingItem += PlayerHandlers.OnChangingItem;
        }

        public void UnregisterPlayerHandlers()
        {
            Exiled.Events.Handlers.Player.ChangingItem -= PlayerHandlers.OnChangingItem;
        }
    }
}
