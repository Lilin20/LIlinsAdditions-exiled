
using System;
using System.Linq;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using GockelsAIO_exiled.Handlers;
using GockelsAIO_exiled.Items.Keycards;
using UnityEngine;
using Player = Exiled.API.Features.Player;
using TextToy = LabApi.Features.Wrappers.TextToy;

namespace GockelsAIO_exiled.Features
{
    public class PointSystem
    {
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
            Quaternion rotation = CalculateCreditCardRotation(attacker);

            Exiled.API.Features.Pickups.Pickup creditCardPickup = SpawnCreditCardPickup(spawnPosition, rotation);
            StoreCreditCardPoints(creditCardPickup, points);
            CreatePointsDisplay(creditCardPickup, points);
        }

        private static Vector3 CalculateSpawnPosition(Player player)
        {
            return player.Position + Vector3.up * CreditCardSpawnHeight;
        }

        private static Quaternion CalculateCreditCardRotation(Player attacker)
        {
            Quaternion cameraRotation = attacker?.CameraTransform.rotation ?? Quaternion.identity;
            return cameraRotation * Quaternion.Euler(-90, 0, 0);
        }

        private static Exiled.API.Features.Pickups.Pickup SpawnCreditCardPickup(Vector3 position, Quaternion rotation)
        {
            CustomItem creditCard = CustomItem.Get(typeof(CreditCard)).Single();
            Exiled.API.Features.Pickups.Pickup pickup = creditCard.Spawn(position);

            if (pickup.Rigidbody != null)
            {
                DisablePhysics(pickup);
            }

            pickup.Rotation = rotation;
            return pickup;
        }

        private static void DisablePhysics(Exiled.API.Features.Pickups.Pickup pickup)
        {
            pickup.Rigidbody.isKinematic = true;
            pickup.Rigidbody.useGravity = false;
            pickup.PhysicsModule.ServerSendRpc(pickup.PhysicsModule.ServerWriteRigidbody);
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