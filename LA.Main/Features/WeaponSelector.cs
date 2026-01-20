using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.CustomItems.API.Features;
using MEC;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LilinsAdditions.Features
{
    public class WeaponSelector
    {
        // Animation Configuration
        private const float AnimationDuration = 5f;
        private const float AnimationInterval = 0.2f;
        private const float YOffsetIncrement = 0.035f;

        public static List<WeightedCustomItem> WeightedCustomWeapons { get; } = new();

        public class WeightedCustomItem
        {
            public string Name { get; }
            public int Weight { get; }

            public WeightedCustomItem(string name, int weight)
            {
                Name = name;
                Weight = weight;
            }
        }

        public static void WeightedCustomWeaponsWithConfig()
        {
            var weightedItems = LilinsAdditions.Instance.Config.MysteryBoxItemPool
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                .Select(p => new WeightedCustomItem(p.Name, p.Weight))
                .ToList();
            
            WeightedCustomWeapons.AddRange(weightedItems);
        }

        public static void StartMysteryBox(Vector3 position)
        {
            Timing.RunCoroutine(RunMysteryBoxAnimation(position));
        }

        private static IEnumerator<float> RunMysteryBoxAnimation(Vector3 position)
        {
            Log.Debug($"Starting mystery box animation at position {position}"); 
            
            Pickup currentPickup = null;
            float elapsed = 0f;
            float yOffset = 0f;
            int iterationCount = 0;

            while (elapsed < AnimationDuration)
            {
                iterationCount++;  
                Log.Debug($"Animation iteration {iterationCount}: elapsed={elapsed:F2}, yOffset={yOffset:F3}");
                
                currentPickup = SpawnTemporaryPickup(position, yOffset, currentPickup);
                
                if (currentPickup == null)
                {
                    Log.Error($"Failed to spawn pickup in iteration {iterationCount}");
                }
                else
                {
                    Log.Debug($"Successfully spawned pickup {currentPickup.Serial} in iteration {iterationCount}");
                }
                
                yOffset += YOffsetIncrement;

                yield return Timing.WaitForSeconds(AnimationInterval);
                elapsed += AnimationInterval;
                Log.Debug($"After wait: elapsed={elapsed:F2}, remaining={AnimationDuration - elapsed:F2}");
            }
            
            Log.Debug($"Animation completed after {iterationCount} iterations, spawning final pickup");
            SpawnFinalPickup(position, yOffset, currentPickup);
            Log.Debug("Mystery box animation finished");
        }

        private static Pickup SpawnTemporaryPickup(Vector3 basePosition, float yOffset, Pickup previousPickup)
        {
            previousPickup?.Destroy();

            string itemName = GetWeightedRandomItem();
            Vector3 spawnPosition = basePosition + Vector3.up * yOffset;

            if (CustomItem.TrySpawn(itemName, spawnPosition, out Pickup pickup))
            {
                ConfigurePickupPhysics(pickup, enableGravity: false);
                return pickup;
            }

            return null;
        }

        private static void SpawnFinalPickup(Vector3 basePosition, float yOffset, Pickup previousPickup)
        {
            previousPickup?.Destroy();

            string itemName = GetWeightedRandomItem();
            Vector3 spawnPosition = basePosition + Vector3.up * yOffset;

            if (CustomItem.TrySpawn(itemName, spawnPosition, out Pickup finalPickup))
            {
                ConfigurePickupPhysics(finalPickup, enableGravity: false);
            }
        }

        private static void ConfigurePickupPhysics(Pickup pickup, bool enableGravity)
        {
            if (pickup.Rigidbody == null)
                return;

            pickup.Rotation = Quaternion.identity;
            pickup.Rigidbody.isKinematic = true;
            pickup.Rigidbody.useGravity = enableGravity;
            pickup.Rigidbody.detectCollisions = false;

            
        }

        private static string GetWeightedRandomItem()
        {
            if (WeightedCustomWeapons.Count == 0)
                return null;

            int totalWeight = WeightedCustomWeapons.Sum(item => item.Weight);
            if (totalWeight <= 0)
                return WeightedCustomWeapons[0].Name;

            int randomWeight = Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var item in WeightedCustomWeapons)
            {
                currentWeight += item.Weight;
                if (randomWeight < currentWeight)
                {
                    return item.Name;
                }
            }

            return WeightedCustomWeapons[0].Name;
        }
    }
}