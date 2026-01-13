using Exiled.API.Interfaces;
using LilinsAdditions.CustomRoles.Roles.ClassD;
using LilinsAdditions.CustomRoles.Roles.NTF;
using LilinsAdditions.CustomRoles.Roles.Zombies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LilinsAdditions.CustomRoles
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public string BurstSoundPath { get; set; } = string.Empty;
        public List<LuckyMan> luckyMan { get; set; } = new()
        {
            new LuckyMan()
        };
        public List<Thief> thief { get; set; } = new()
        {
            new Thief()
        };
        public List<RiotOperator> riotOperator { get; set; } = new()
        {
            new RiotOperator()
        };
        public List<KamikazeZombie> kamikazeZombie { get; set; } = new()
        {
            new KamikazeZombie()
        };
    }
}
