using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GockelsAIO_exiled.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Selector : ICommand
    {
        public string Command => "listplayers";
        public string[] Aliases => new[] { "lp" };
        public string Description => "Lists all living players with an ID.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerSender)
            {
                response = "This command can only be run by a player.";
                return false;
            }

            var players = Player.List.Where(p => p.IsAlive).ToList();

            if (!players.Any())
            {
                response = "No players are currently alive.";
                return true;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Alive Players:");
            for (int i = 0; i < players.Count; i++)
            {
                sb.AppendLine($"{i}. {players[i].Nickname} ({players[i].Role.Team})");
            }

            response = sb.ToString();
            return true;
        }
    }

    [CommandHandler(typeof(ClientCommandHandler))]
    public class SelectPlayer : ICommand
    {
        public string Command => "select";
        public string[] Aliases => new[] { "sp" };
        public string Description => "Selects a living player by ID (from listplayers).";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerSender)
            {
                response = "This command can only be run by a player.";
                return false;
            }

            var executor = Player.Get(playerSender);

            if (arguments.Count == 0)
            {
                response = "Usage: select <ID>";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out int id))
            {
                response = "ID must be a number.";
                return false;
            }

            var players = Player.List.Where(p => p.IsAlive).ToList();

            if (id < 0 || id >= players.Count)
            {
                response = "Invalid ID. Use listplayers to see valid IDs.";
                return false;
            }

            var selected = players[id];
            Storage.SelectedPlayers[executor] = selected;

            response = $"You have selected {selected.Nickname} ({selected.Role.Name}).";
            return true;
        }
    }
}
