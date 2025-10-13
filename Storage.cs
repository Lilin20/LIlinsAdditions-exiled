using Exiled.API.Features;
using System.Collections.Generic;

namespace GockelsAIO_exiled
{
    public static class Storage
    {
        public static Dictionary<Player, Player> SelectedPlayers { get; } = new();
    }
}
