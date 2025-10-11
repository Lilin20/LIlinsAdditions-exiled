using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GockelsAIO_exiled
{
    public static class Storage
    {
        public static Dictionary<Player, Player> SelectedPlayers { get; } = new();
    }
}
