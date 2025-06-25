using CommandSystem;
using Exiled.API.Features;
using GockelsAIO_exiled.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GockelsAIO_exiled.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Points : ICommand
    {
        public string Command => "points";
        public string[] Aliases => new[] { "pts" };
        public string Description => "Verwalte die Punkte eines Spielers (add/set/remove/get)";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 2)
            {
                response = "Usage: points (get|add|set|remove) <player> [amount]";
                return false;
            }

            string action = arguments.At(0).ToLower();
            string playerName = arguments.At(1);
            Player player = Player.Get(playerName);

            if (player == null)
            {
                response = $"Spieler '{playerName}' nicht gefunden.";
                return false;
            }

            switch (action)
            {
                case "get":
                    int currentPoints = PointSystem.GetPoints(player);
                    response = $"Spieler {player.Nickname} hat {currentPoints} Punkte.";
                    return true;

                case "add":
                case "set":
                case "remove":
                    if (arguments.Count < 3 || !int.TryParse(arguments.At(2), out int amount))
                    {
                        response = "Bitte gib eine gültige Punktzahl an.";
                        return false;
                    }

                    if (action == "add")
                    {
                        PointSystem.AddPoints(player, amount);
                        response = $"Spieler {player.Nickname} wurden {amount} Punkte hinzugefügt.";
                    }
                    else if (action == "set")
                    {
                        PointSystem.SetPoints(player, amount);
                        response = $"Spieler {player.Nickname} hat nun {amount} Punkte.";
                    }
                    else // remove
                    {
                        PointSystem.RemovePoints(player, amount);
                        response = $"{amount} Punkte von {player.Nickname} entfernt.";
                    }

                    return true;

                default:
                    response = "Ungültige Aktion. Verwende: get, add, set, remove";
                    return false;
            }
        }
    }
}
