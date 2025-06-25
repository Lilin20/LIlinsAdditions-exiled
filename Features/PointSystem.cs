using Exiled.API.Features;
using GockelsAIO_exiled.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GockelsAIO_exiled.Features
{
    public class PointSystem
    {
        public static void AddPoints(Player player, int points)
        {
            if (PlayerHandler.PlayerPoints.ContainsKey(player))
            {
                PlayerHandler.PlayerPoints[player] += points;
                Log.Debug($"Spieler {player.Nickname} hat jetzt {PlayerHandler.PlayerPoints[player]} Punkte.");
            }
        }

        public static int GetPoints(Player player)
        {
            if (PlayerHandler.PlayerPoints.TryGetValue(player, out int points))
                return points;

            return 0; // Falls Spieler nicht gefunden wird, z. B. wenn er noch nicht gespawnt ist
        }

        public static void SetPoints(Player player, int points)
        {
            if (PlayerHandler.PlayerPoints.ContainsKey(player))
            {
                PlayerHandler.PlayerPoints[player] = points;
                Log.Debug($"Spieler {player.Nickname} hat jetzt {PlayerHandler.PlayerPoints[player]} Punkte.");
            }
        }

        public static void RemovePoints(Player player, int pointsToRemove)
        {
            if (PlayerHandler.PlayerPoints.TryGetValue(player, out int currentPoints))
            {
                int newPoints = Math.Max(0, currentPoints - pointsToRemove);
                PlayerHandler.PlayerPoints[player] = newPoints;
            }
        }
    }
}
