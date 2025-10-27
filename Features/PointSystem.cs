using AdminToys;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using Exiled.CustomItems.API.Features;
using GockelsAIO_exiled.Handlers;
using GockelsAIO_exiled.Items.Keycards;
using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Player = Exiled.API.Features.Player;
using TextToy = LabApi.Features.Wrappers.TextToy;

namespace GockelsAIO_exiled.Features
{
    public class PointSystem
    {
        public static void AddPoints(Player player, int points)
        {
            if (PlayerHandler.PlayerPoints.ContainsKey(player))
            {
                PlayerHandler.PlayerPoints[player] += points;
                Log.Debug($"Player {player.Nickname} now has {PlayerHandler.PlayerPoints[player]} Points.");
            }
        }

        public static int GetPoints(Player player)
        {
            if (PlayerHandler.PlayerPoints.TryGetValue(player, out int points))
                return points;

            return 0;
        }

        public static void SetPoints(Player player, int points)
        {
            if (PlayerHandler.PlayerPoints.ContainsKey(player))
            {
                PlayerHandler.PlayerPoints[player] = points;
                Log.Debug($"Player {player.Nickname} now has {PlayerHandler.PlayerPoints[player]} Points.");
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

        public static void SpawnCreditCard(Player dyingPlayer, Player attacker)
        {
            Vector3 position = dyingPlayer.Position + new Vector3(0, 0.4f, 0);

            // Get the killer's camera rotation instead of the dying player's  
            Quaternion cameraRotation = attacker?.CameraTransform.rotation ?? Quaternion.identity;

            CustomItem creditCard = CustomItem.Get(typeof(CreditCard)).Single();
            Exiled.API.Features.Pickups.Pickup? ccPickup = creditCard.Spawn(position);

            if (ccPickup.Rigidbody != null)
            {
                ccPickup.Rigidbody.isKinematic = true;
                ccPickup.PhysicsModule.ServerSendRpc(ccPickup.PhysicsModule.ServerWriteRigidbody);
                ccPickup.Rigidbody.useGravity = false;
            }

            // Apply the killer's camera rotation to the pickup  
            ccPickup.Rotation = cameraRotation * Quaternion.Euler(-90, 0, 0);

            string key = $"CreditCard_Points_{ccPickup.Serial}";
            Exiled.API.Features.Server.SessionVariables[key] = GetPoints(dyingPlayer);

            TextToy tt = TextToy.Create(ccPickup.Transform);
            tt.Transform.localPosition += new Vector3(0f, 0f, 0.2f);
            tt.Rotation = Quaternion.Euler(90, 0, 0);
            tt.TextFormat = $"<size=2>{GetPoints(dyingPlayer)}</size>";
        }
    }
}
