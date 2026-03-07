using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using LilinsAdditions.Main.Handlers;
using ProjectMER.Features;
using UnityEngine;

namespace LilinsAdditions.Main.Features;

public class SchematicSpawner
{
    public void SpawnCoins(int amount)
    {
        Dictionary<RoomType, SpawnData> spawnData = LilinsAdditions.Instance.Config.CoinSpawnPoints;

        for (var i = 0; i < amount; i++)
        {
            if (spawnData.Count == 0) break;

            if (Random.value > 0.25f)
                continue;

            var randomEntry = spawnData.ElementAt(Random.Range(0, spawnData.Count));
            var selectedType = randomEntry.Key;
            var data = randomEntry.Value;

            var rooms = Room.List
                .Where(r => r.Type == selectedType && r.Zone != ZoneType.Surface && r.Type != RoomType.Pocket)
                .ToList();

            if (rooms.Count == 0)
            {
                spawnData.Remove(selectedType);
                continue;
            }

            var room = rooms[Random.Range(0, rooms.Count)];

            Log.Debug($"Coin spawned in: {room.Name}");

            Vector3 globalPosition = room.transform.localToWorldMatrix *
                                     new Vector4(data.Position.x, data.Position.y, data.Position.z, 1);
            var globalRotation = room.transform.rotation * Quaternion.Euler(data.Rotation);

            ObjectSpawner.TrySpawnSchematic("Coin", globalPosition, globalRotation, out var schematic);

            PMERHandler.TrackedCoins.Add(schematic);

            spawnData.Remove(selectedType);
        }
    }

    public void SpawnMysteryBoxes(int amount)
    {
        Dictionary<RoomType, SpawnData> spawnData = new(LilinsAdditions.Instance.Config.MysteryBoxSpawnPoints);

        for (var i = 0; i < amount; i++)
        {
            if (spawnData.Count == 0)
                break;

            if (Random.value < 0.1f) continue;

            var randomEntry = spawnData.ElementAt(Random.Range(0, spawnData.Count));
            var selectedType = randomEntry.Key;
            var data = randomEntry.Value;

            var rooms = Room.List
                .Where(r => r.Type == selectedType && r.Zone != ZoneType.Surface && r.Type != RoomType.Pocket)
                .ToList();

            if (rooms.Count == 0)
            {
                spawnData.Remove(selectedType);
                continue;
            }

            var room = rooms[Random.Range(0, rooms.Count)];

            Log.Debug(room.Type);

            Vector3 globalPosition = room.transform.localToWorldMatrix *
                                     new Vector4(data.Position.x, data.Position.y, data.Position.z, 1);
            var globalRotation = room.transform.rotation * Quaternion.Euler(data.Rotation);

            if (ObjectSpawner.TrySpawnSchematic("MysteryBox", globalPosition, globalRotation, out var schematic))
                PMERHandler.TrackedSchematics.Add(schematic);

            spawnData.Remove(selectedType);
        }
    }

    public void SpawnGobblegumMachines(int maxCount)
    {
        var spawned = 0;

        foreach (var kvp in LilinsAdditions.Instance.Config.VendingMachineSpawnPoints)
        {
            if (spawned >= maxCount)
                break;

            if (Random.value > 0.5f)
                continue;

            var room = Room.Get(kvp.Key);
            if (room == null)
                continue;

            Vector3 localPos = kvp.Value.Position;
            Vector3 localRot = kvp.Value.Rotation;

            Vector3 globalPosition =
                room.transform.localToWorldMatrix * new Vector4(localPos.x, localPos.y, localPos.z, 1);
            var globalRotation = room.transform.rotation * Quaternion.Euler(localRot);

            if (ObjectSpawner.TrySpawnSchematic("GobblegumMachine", globalPosition, globalRotation, out var schematic))
            {
                spawned++;
                PMERHandler.TrackedGobblegumMachines.Add(schematic);

                PMERHandler.VendingMachineUsage[schematic] = 0;

                foreach (var block in schematic.AttachedBlocks)
                    if (block.name == "FizzSpeaker")
                    {
                        var go = block.gameObject;

                        var audioPlayer = AudioPlayer.CreateOrGet($"GobblegumSound{Random.Range(1, 10000)}",
                            onIntialCreation: p =>
                            {
                                var speaker = p.AddSpeaker($"Main{Random.Range(1, 10000)}", isSpatial: true,
                                    minDistance: 1f, maxDistance: 15f);
                                speaker.transform.parent = go.transform;
                                speaker.transform.localPosition = Vector3.zero;
                            });

                        audioPlayer.AddClip("gobblegum", loop: true,
                            volume: LilinsAdditions.Instance.Config.VendingMachineMusicVolume, destroyOnEnd: false);
                    }
            }
        }
    }

    public class SpawnData
    {
        public SpawnData()
        {
        }

        public SpawnData(Vector3 pos, Vector3 rot)
        {
            Position = pos;
            Rotation = rot;
        }

        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
    }
}