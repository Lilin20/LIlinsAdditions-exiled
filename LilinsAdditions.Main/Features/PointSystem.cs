using System;
using System.Linq;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using LilinsAdditions.Handlers;
using LilinsAdditions.Items.Keycards;
using UnityEngine;
using Player = Exiled.API.Features.Player;
using TextToy = LabApi.Features.Wrappers.TextToy;

namespace LilinsAdditions.Features
{
    public class PointSystem
    {
        // Configuration
        private const float CreditCardSpawnHeight = 0.4f;
        private const float TextToyOffset = 0.2f;
        private const string SessionVariablePrefix = "CreditCard_Points_";

        public static void AddPoints(Player player, int points)
        {
            if (!PlayerHandler.PlayerPoints.ContainsKey(player))
                return;

            PlayerHandler.PlayerPoints[player] += points;
            Log.Debug($"Player {player.Nickname} now has {PlayerHandler.PlayerPoints[player]} points.");
        }

        public static int GetPoints(Player player)
        {
            return PlayerHandler.PlayerPoints.TryGetValue(player, out int points) ? points : 0;
        }

        public static void SetPoints(Player player, int points)
        {
            if (!PlayerHandler.PlayerPoints.ContainsKey(player))
                return;

            PlayerHandler.PlayerPoints[player] = points;
            Log.Debug($"Player {player.Nickname} now has {PlayerHandler.PlayerPoints[player]} points.");
        }

        public static void RemovePoints(Player player, int pointsToRemove)
        {
            if (!PlayerHandler.PlayerPoints.TryGetValue(player, out int currentPoints))
                return;

            PlayerHandler.PlayerPoints[player] = Math.Max(0, currentPoints - pointsToRemove);
            Log.Debug($"Player {player.Nickname} now has {PlayerHandler.PlayerPoints[player]} points after removing {pointsToRemove}.");
        }

        public static void SpawnCreditCard(Player dyingPlayer, Player attacker)
        {
            int points = GetPoints(dyingPlayer);
            Vector3 spawnPosition = CalculateSpawnPosition(dyingPlayer);
            Quaternion cameraRotation = CalculateCreditCardRotation(attacker);

            Exiled.API.Features.Pickups.Pickup creditCardPickup = SpawnCreditCardPickup(spawnPosition, cameraRotation);
            StoreCreditCardPoints(creditCardPickup, points);
            CreatePointsDisplay(creditCardPickup, points);
        }

        private static Vector3 CalculateSpawnPosition(Player player)
        {
            return player.Position + Vector3.up * CreditCardSpawnHeight;
        }

        private static Quaternion CalculateCreditCardRotation(Player attacker)
        {
            return attacker?.CameraTransform.rotation ?? Quaternion.identity;
        }

        private static Exiled.API.Features.Pickups.Pickup SpawnCreditCardPickup(Vector3 position, Quaternion cameraRotation)  
        {  
            CustomItem creditCard = CustomItem.Get(typeof(CreditCard)).Single();
            Exiled.API.Features.Pickups.Pickup pickup = creditCard.Spawn(position);
            
            pickup.Rotation = cameraRotation * Quaternion.Euler(-90, 0, 0);
            
            if (pickup.Rigidbody != null)
            {
                pickup.Rigidbody.isKinematic = true;
                pickup.PhysicsModule.ServerSendRpc(pickup.PhysicsModule.ServerWriteRigidbody);
                pickup.Rigidbody.useGravity = false;
            }
            
            return pickup;
        }

        private static void StoreCreditCardPoints(Exiled.API.Features.Pickups.Pickup pickup, int points)
        {
            string key = $"{SessionVariablePrefix}{pickup.Serial}";
            Server.SessionVariables[key] = points;
        }

        private static void CreatePointsDisplay(Exiled.API.Features.Pickups.Pickup pickup, int points)
        {
            TextToy textDisplay = TextToy.Create(pickup.Transform);
            textDisplay.Transform.localPosition += Vector3.forward * TextToyOffset;
            textDisplay.Rotation = Quaternion.Euler(90, 0, 0);
            textDisplay.TextFormat = $"<size=2>{points}</size>";
        }
    }
}