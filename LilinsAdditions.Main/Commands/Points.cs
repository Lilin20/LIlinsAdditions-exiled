using System;
using CommandSystem;
using Exiled.API.Features;
using LilinsAdditions.Main.Features;

namespace LilinsAdditions.Main.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class Points : ICommand
{
    private const int MinimumArgumentCount = 2;
    private const int ArgumentCountWithAmount = 3;
    private const string UsageMessage = "Usage: points (get|add|set|remove) <player> [amount]";
    private const string InvalidActionMessage = "Not a valid action. Use: get, add, set, remove";
    private const string InvalidAmountMessage = "Please enter a valid amount.";
    public string Command => "points";
    public string[] Aliases => new[] { "pts" };
    public string Description => "Manages points for a selected player (add/set/remove/get)";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count < MinimumArgumentCount)
        {
            response = UsageMessage;
            return false;
        }

        var action = arguments.At(0).ToLower();
        var playerName = arguments.At(1);

        if (!TryGetPlayer(playerName, out var player))
        {
            response = $"Player '{playerName}' not found.";
            return false;
        }

        return ExecuteAction(action, player, arguments, out response);
    }

    private bool TryGetPlayer(string playerName, out Player player)
    {
        player = Player.Get(playerName);
        return player != null;
    }

    private bool ExecuteAction(string action, Player player, ArraySegment<string> arguments, out string response)
    {
        switch (action)
        {
            case "get":
                return HandleGetPoints(player, out response);

            case "add":
            case "set":
            case "remove":
                return HandlePointsModification(action, player, arguments, out response);

            default:
                response = InvalidActionMessage;
                return false;
        }
    }

    private bool HandleGetPoints(Player player, out string response)
    {
        var currentPoints = PointSystem.GetPoints(player);
        response = $"Player {player.Nickname} has {currentPoints} points.";
        return true;
    }

    private bool HandlePointsModification(string action, Player player, ArraySegment<string> arguments,
        out string response)
    {
        if (!TryParseAmount(arguments, out var amount))
        {
            response = InvalidAmountMessage;
            return false;
        }

        switch (action)
        {
            case "add":
                PointSystem.AddPoints(player, amount);
                response = $"Added {amount} points to {player.Nickname}.";
                break;

            case "set":
                PointSystem.SetPoints(player, amount);
                response = $"Set {player.Nickname}'s points to {amount}.";
                break;

            case "remove":
                PointSystem.RemovePoints(player, amount);
                response = $"Removed {amount} points from {player.Nickname}.";
                break;

            default:
                response = InvalidActionMessage;
                return false;
        }

        return true;
    }

    private bool TryParseAmount(ArraySegment<string> arguments, out int amount)
    {
        amount = 0;
        return arguments.Count >= ArgumentCountWithAmount && int.TryParse(arguments.At(2), out amount);
    }
}