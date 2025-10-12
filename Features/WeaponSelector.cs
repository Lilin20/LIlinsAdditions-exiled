using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MapGeneration;
using MEC;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using System.Collections.Generic;
using UnityEngine;

namespace GockelsAIO_exiled
{
    public class WeaponSelector
    {
        private static readonly List<WeightedCustomItem> WeightedCustomWeapons = new()
        {
            new WeightedCustomItem(100, 2),
            new WeightedCustomItem(101, 3),
            new WeightedCustomItem(200, 10),
            new WeightedCustomItem(201, 10),
            new WeightedCustomItem(202, 10),
            new WeightedCustomItem(203, 3),
            new WeightedCustomItem(300, 10),
            new WeightedCustomItem(400, 2),
            new WeightedCustomItem(500, 6),
            new WeightedCustomItem(401, 1),
        };
        
        private static readonly float Duration = 5f;
        private static readonly float Interval = 0.2f;

        private class WeightedCustomItem
        {
            public uint Id;
            public int Weight;

            public WeightedCustomItem(uint id, int weight)
            {
                Id = id;
                Weight = weight;
            }
        }

        private static uint GetWeightedCustomItem()
        {
            int totalWeight = 0;
            foreach (var item in WeightedCustomWeapons)
            {
                totalWeight += item.Weight;
            }

            int randomWeight = UnityEngine.Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var item in WeightedCustomWeapons)
            {
                currentWeight += item.Weight;
                if (randomWeight < currentWeight)
                {
                    return item.Id;
                }
            }

            return WeightedCustomWeapons[0].Id;
        }

        public static void StartMysteryBox(Vector3 position)
        {
            Timing.RunCoroutine(RunCustomMysteryBox(position));
        }

        private static IEnumerator<float> RunCustomMysteryBox(Vector3 position)
        {
            Pickup currentPickup = null;
            Pickup finalPickup = null;
            float elapsed = 0f;
            float yOffset = 0f;

            while (elapsed < Duration)
            {
                currentPickup?.Destroy();

                uint randomWeapon = GetWeightedCustomItem(); //CustomWeaponPool[UnityEngine.Random.Range(0, CustomWeaponPool.Count)];

                Vector3 currentPosition = position + new Vector3(0f, yOffset, 0f);

                CustomItem.TrySpawn(randomWeapon, currentPosition, out currentPickup);
                currentPickup.Rotation = Quaternion.identity;
                currentPickup.Rigidbody.isKinematic = true;
                currentPickup.PhysicsModule.ServerSendRpc(currentPickup.PhysicsModule.ServerWriteRigidbody);
                currentPickup.Rigidbody.useGravity = false;
                currentPickup.Rigidbody.detectCollisions = false;

                yOffset += 0.035f;

                yield return Timing.WaitForSeconds(Interval);
                elapsed += Interval;
            }

            currentPickup?.Destroy();
            uint finalCustom = GetWeightedCustomItem(); //CustomWeaponPool[UnityEngine.Random.Range(0, CustomWeaponPool.Count)];
            CustomItem.TrySpawn(finalCustom, position + new Vector3(0f, yOffset, 0f), out finalPickup);
            finalPickup.Rigidbody.isKinematic = true;
            finalPickup.PhysicsModule.ServerSendRpc(finalPickup.PhysicsModule.ServerWriteRigidbody);
            finalPickup.Rigidbody.useGravity = false;
            finalPickup.Rigidbody.detectCollisions = false;
        }
    }
}
