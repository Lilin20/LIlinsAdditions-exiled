using System.Collections.Generic;
using System.Linq;
using AdminToys;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using ProjectMER.Features;
using UnityEngine;

namespace GockelsAIO_exiled
{
    public class SchematicSpawner
    {
        public static readonly Dictionary<RoomType, (Vector3 pos, Vector3 rot)> RoomSpawnData = new()
        {
            { RoomType.Lcz330,              (new Vector3(-5.84f, 0.13f, 3.014f),         new Vector3(0, -90, 0))         },
            { RoomType.LczGlassBox,         (new Vector3(4.503f, 0.13f, 4.947f),         new Vector3(0, -90, 0))         },
            { RoomType.LczAirlock,          (new Vector3(0f, 0.13f, 1f),                 new Vector3(0, -90, 0))         },
            { RoomType.LczCafe,             (new Vector3(-5.878f, 0.13f, 4.542f),        new Vector3(0, -90, 0))         },
            { RoomType.LczClassDSpawn,      (new Vector3(-24.711f, 0.13f, 0f),           new Vector3(0, -180f, 0))       },
            { RoomType.LczCrossing,         (new Vector3(2.343f, 0.13f, -2.31f),         new Vector3(0, 45, 0))          },
            { RoomType.LczPlants,           (new Vector3(0f, 0.13f, 1.474f),             new Vector3(0, -90f, 0))        },
            { RoomType.LczStraight,         (new Vector3(0f, 0.13f, -1.144f),            new Vector3(0, 90f, 0))         },
            { RoomType.LczTCross,           (new Vector3(1.158f, 0.13f, 0f),             new Vector3(0, 0f, 0))          },
            { RoomType.HczIntersection,     (new Vector3(-5.133f, 0.13f, -1.647f),       new Vector3(0, 90, 0))          },
            { RoomType.Hcz096,              (new Vector3(-1.781f, 0.13f, 1.285f),        new Vector3(0, -90f, 0))        },
            { RoomType.Hcz127,              (new Vector3(-3.769f, 0.13f, -5.085f),       new Vector3(0, 135f, 0))        },
            { RoomType.HczIntersectionJunk, (new Vector3(-1.614f, 0.13f, 0f),            new Vector3(0, -180f, 0))       },
            { RoomType.HczHid,              (new Vector3(2.247f, 0.13f, -1.868f),        new Vector3(0, 90f, 0))         },
            { RoomType.HczStraightPipeRoom, (new Vector3(-6.326f, 5.204f, 5.375f),       new Vector3(0, 180f, 0))        },
            { RoomType.HczArmory,           (new Vector3(2.065f, 0.13f, 5.214f),         new Vector3(0, 0f, 0))          },
        };

        public static readonly Dictionary<RoomType, (Vector3 pos, Vector3 rot)> GobblegumRoomSpawnData = new()
        {
            { RoomType.LczGlassBox,         (new Vector3(8.842f, 0.332f, -2.923f),       new Vector3(0, 0, 0))           },
            { RoomType.LczCafe,             (new Vector3(-4.339f, 0.332f, -4.614f),      new Vector3(0, 90, 0))          },
            { RoomType.Lcz914,              (new Vector3(0f, 0.332f, 6.879f),            new Vector3(0, -90, 0))         },
            { RoomType.HczHid,              (new Vector3(1.037f, 0.332f, 4.992f),        new Vector3(0, -90f, 0))        },
            { RoomType.HczArmory,           (new Vector3(2.11f, 0.332f, -5.15f),         new Vector3(0, 0f, 0))          },
        };

        public static readonly Dictionary<RoomType, (Vector3 pos, Vector3 rot)> CoinRoomSpawnDate = new()
        {
            { RoomType.LczAirlock,          (new Vector3(2.855f, 0.0178f, -4.25f),       new Vector3(0, 0, 0))           },
            { RoomType.Lcz173,              (new Vector3(12.099f, 11.479f, 3.962f),      new Vector3(0, 0, 0))           },
            { RoomType.LczArmory,           (new Vector3(0.2342f, 0.524f, 2.0502f),      new Vector3(0, 0, 0))           },
            { RoomType.HczHid,              (new Vector3(-6.05f, 4.4983f, -4.744f),      new Vector3(0, 0, 0))           },
            { RoomType.HczNuke,             (new Vector3(1.288f, -72.417f, -0.423f),     new Vector3(0, 0, 0))           },
        };

        public void SpawnCoins(int amount)
        {
            Dictionary<RoomType, (Vector3 pos, Vector3 rot)> spawnData = CoinRoomSpawnDate;

            for (int i = 0; i < amount; i++)
            {
                

                if (spawnData.Count == 0)
                {
                    break;
                }

                if (UnityEngine.Random.value > 0.25f)
                    continue;

                var randomEntry = spawnData.ElementAt(UnityEngine.Random.Range(0, spawnData.Count));
                RoomType selectedType = randomEntry.Key;
                var data = randomEntry.Value;

                var rooms = Exiled.API.Features.Room.List
                    .Where(r => r.Type == selectedType && r.Zone != ZoneType.Surface && r.Type != RoomType.Pocket)
                    .ToList();

                if (rooms.Count == 0)
                {
                    spawnData.Remove(selectedType);
                    continue;
                }

                var room = rooms[UnityEngine.Random.Range(0, rooms.Count)];

                Log.Info($"Coin spawned in: {room.Name}");

                Vector3 globalPosition = room.transform.localToWorldMatrix * new Vector4(data.pos.x, data.pos.y, data.pos.z, 1);
                Quaternion globalRotation = room.transform.rotation * Quaternion.Euler(data.rot);

                ObjectSpawner.TrySpawnSchematic("Coin", globalPosition, globalRotation, out var schematic);

                EventHandlers.TrackedCoins.Add(schematic);

                spawnData.Remove(selectedType);
            }
        }

        public void SpawnMysteryBoxes(int amount)
        {
            // Lokale Kopie des Dictionaries, damit wir RoomTypes entfernen können
            Dictionary<RoomType, (Vector3 pos, Vector3 rot)> spawnData = new(RoomSpawnData);

            for (int i = 0; i < amount; i++)
            {
                if (spawnData.Count == 0)
                    break; // Keine RoomTypes mehr übrig

                if (UnityEngine.Random.value < 0.1f)
                {
                    continue;
                }

                // Wähle zufälligen RoomType aus dem Dictionary
                var randomEntry = spawnData.ElementAt(UnityEngine.Random.Range(0, spawnData.Count));
                RoomType selectedType = randomEntry.Key;
                var data = randomEntry.Value;

                // Räume vom Typ finden (außer Pocket & Surface)
                var rooms = Exiled.API.Features.Room.List
                    .Where(r => r.Type == selectedType && r.Zone != ZoneType.Surface && r.Type != RoomType.Pocket)
                    .ToList();

                if (rooms.Count == 0)
                {
                    spawnData.Remove(selectedType); // Diesen RoomType trotzdem entfernen
                    continue;
                }

                var room = rooms[UnityEngine.Random.Range(0, rooms.Count)];

                Log.Info(room.Type);

                Vector3 globalPosition = room.transform.localToWorldMatrix * new Vector4(data.pos.x, data.pos.y, data.pos.z, 1);
                Quaternion globalRotation = room.transform.rotation * Quaternion.Euler(data.rot);

                if (ObjectSpawner.TrySpawnSchematic("MysteryBox", globalPosition, globalRotation, out var schematic))
                {
                    EventHandlers.TrackedSchematics.Add(schematic);
                }

                // Benutzten RoomType entfernen
                spawnData.Remove(selectedType);
            }
        }

        public void SpawnTrapTest()
        {
            var room = Exiled.API.Features.Room.Get(RoomType.LczCrossing);

            Vector3 yourLocalPositionInTheRoom = new Vector3(0, 0, 0);
            Vector3 yourLocalRotationInTheRoom = new Vector3(0, 0, 0);

            Vector3 globalPosition = room.transform.localToWorldMatrix * new Vector4(yourLocalPositionInTheRoom.x, yourLocalPositionInTheRoom.y, yourLocalPositionInTheRoom.z, 1);
            Quaternion globalRotation = room.transform.rotation * Quaternion.Euler(yourLocalRotationInTheRoom);

            ObjectSpawner.TrySpawnSchematic("Trap1", globalPosition, globalRotation, out var trapSchematic);

            foreach (var block in trapSchematic.AttachedBlocks)
            {
                if (block.name == "TrapBase")
                {
                    Log.Info("TrapBase gefunden");
                    GameObject trapBase = block.gameObject;

                    BoxCollider collider = trapBase.AddComponent<BoxCollider>();
                    collider.size = new Vector3(1f, 7f, 1f);
                    collider.isTrigger = true;
                    trapBase.AddComponent<DeathBox>();
                }
            }
        }

        public void SpawnGobblegumMachines(int maxCount)
        {
            int spawned = 0;

            foreach (var kvp in GobblegumRoomSpawnData)
            {
                if (spawned >= maxCount)
                    break;

                // 50 % Chance
                if (UnityEngine.Random.value > 0.5f)
                    continue;

                var room = Exiled.API.Features.Room.Get(kvp.Key);
                if (room == null)
                    continue;

                Vector3 localPos = kvp.Value.pos;
                Vector3 localRot = kvp.Value.rot;

                Vector3 globalPosition = room.transform.localToWorldMatrix * new Vector4(localPos.x, localPos.y, localPos.z, 1);
                Quaternion globalRotation = room.transform.rotation * Quaternion.Euler(localRot);

                if (ObjectSpawner.TrySpawnSchematic("GobblegumMachine", globalPosition, globalRotation, out var schematic))
                {
                    spawned++;
                    EventHandlers.TrackedGobblegumMachines.Add(schematic);

                    foreach (var block in schematic.AttachedBlocks)
                    {
                        if (block.name == "FizzSpeaker")
                        {
                            GameObject go = block.gameObject;

                            AudioPlayer audioPlayer = AudioPlayer.CreateOrGet($"GobblegumSound{UnityEngine.Random.Range(1, 10000)}", onIntialCreation: (p) =>
                            {
                                Speaker speaker = p.AddSpeaker($"Main{UnityEngine.Random.Range(1, 10000)}", isSpatial: true, minDistance: 1f, maxDistance: 15f);
                                speaker.transform.parent = go.transform;
                                speaker.transform.localPosition = Vector3.zero;
                            });

                            audioPlayer.AddClip("gobblegum", loop: true, volume: 2, destroyOnEnd: false);
                        }
                    }
                }
            }
        }
    }
}
