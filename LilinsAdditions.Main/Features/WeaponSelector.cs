using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.CustomItems.API.Features;
using MEC;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LilinsAdditions.Main.Features;

public class WeaponSelector
{
    // Animation Configuration
    private const float AnimationDuration = 5f;
    private const float AnimationInterval = 0.2f;
    private const float YOffsetIncrement = 0.035f;

    public static List<WeightedCustomItem> WeightedCustomWeapons { get; } = new();

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
        var elapsed = 0f;
        var yOffset = 0f;
        var iterationCount = 0;

        while (elapsed < AnimationDuration)
        {
            iterationCount++;
            Log.Debug($"Animation iteration {iterationCount}: elapsed={elapsed:F2}, yOffset={yOffset:F3}");

            currentPickup = SpawnTemporaryPickup(position, yOffset, currentPickup);

            if (currentPickup == null)
                Log.Error($"Failed to spawn pickup in iteration {iterationCount}");
            else
                Log.Debug($"Successfully spawned pickup {currentPickup.Serial} in iteration {iterationCount}");

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

        var itemName = GetWeightedRandomItem();
        var spawnPosition = basePosition + Vector3.up * yOffset;

        if (CustomItem.TrySpawn(itemName, spawnPosition, out var pickup))
        {
            ConfigurePickupPhysics(pickup, false);
            return pickup;
        }

        return null;
    }

    private static void SpawnFinalPickup(Vector3 basePosition, float yOffset, Pickup previousPickup)
    {
        previousPickup?.Destroy();

        var itemName = GetWeightedRandomItem();
        var spawnPosition = basePosition + Vector3.up * yOffset;
        
        if (WeightedCustomWeapons.Any(w => w.Name == itemName) && CustomItem.TrySpawn(itemName, spawnPosition, out var finalPickup))
        {
            ConfigurePickupPhysics(finalPickup, false);
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

        var totalWeight = WeightedCustomWeapons.Sum(item => item.Weight);
        if (totalWeight <= 0)
            return WeightedCustomWeapons[0].Name;

        var randomWeight = Random.Range(0, totalWeight);
        var currentWeight = 0;

        foreach (var item in WeightedCustomWeapons)
        {
            currentWeight += item.Weight;
            if (randomWeight < currentWeight) return item.Name;
        }

        return WeightedCustomWeapons[0].Name;
    }

    public class WeightedCustomItem
    {
        public WeightedCustomItem(string name, int weight)
        {
            Name = name;
            Weight = weight;
        }

        public string Name { get; }
        public int Weight { get; }
    }
}