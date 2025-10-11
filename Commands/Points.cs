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
        public string Description => "Manages points for a selected player (add/set/remove/get)";

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
                response = $"Player '{playerName}' not found.";
                return false;
            }

            switch (action)
            {
                case "get":
                    int currentPoints = PointSystem.GetPoints(player);
                    response = $"Player {player.Nickname} has {currentPoints} Points.";
                    return true;

                case "add":
                case "set":
                case "remove":
                    if (arguments.Count < 3 || !int.TryParse(arguments.At(2), out int amount))
                    {
                        response = "Please enter a valid value.";
                        return false;
                    }

                    if (action == "add")
                    {
                        PointSystem.AddPoints(player, amount);
                        response = $"Player {player.Nickname} got {amount} Points.";
                    }
                    else if (action == "set")
                    {
                        PointSystem.SetPoints(player, amount);
                        response = $"Player {player.Nickname} now has {amount} Points.";
                    }
                    else // remove
                    {
                        PointSystem.RemovePoints(player, amount);
                        response = $"Player's {player.Nickname} points where removed.";
                    }

                    return true;

                default:
                    response = "Not a valid aciton. Use: get, add, set, remove";
                    return false;
            }
        }
    }
}
