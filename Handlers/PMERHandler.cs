using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using GockelsAIO_exiled.Features;
using ProjectMER.Features.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GockelsAIO_exiled.Handlers
{
    public class PMERHandler
    {
        public static readonly List<SchematicObject> TrackedSchematics = new();
        public static readonly List<SchematicObject> TrackedGobblegumMachines = new();
        public static readonly List<SchematicObject> TrackedCoins = new();
        public static readonly List<uint> GobblegumIDs = new()
        {
            800,
            801,
            802,
            803,
            805,
            806,
            807,
            808,
            809,
            810,
            811,
            812,
            813,
            814,
            815,
            816,
        };

        public void OnButtonInteract(ProjectMER.Events.Arguments.ButtonInteractedEventArgs ev)
        {
            var schematic = TrackedSchematics.FirstOrDefault(s => s == ev.Schematic);
            if (schematic == null) return;

            int playerPoints = PointSystem.GetPoints(ev.Player);
            if (playerPoints < 800)
            {
                ev.Player.SendBroadcast("<color=red>You need 800 Points to open the box!</color>", 5);
                return;
            }

            PointSystem.RemovePoints(ev.Player, 800);
            Log.Debug($"[Debug] Button-Interaction from Schematic '{schematic.name}' detected.");

            foreach (var block in schematic.AttachedBlocks)
            {
                switch (block.name)
                {
                    case "ChestOpenTop":
                        block.gameObject.GetComponent<Animator>()?.SetTrigger("StartAnim");
                        break;

                    case "WeaponSpawn":
                        WeaponSelector.StartMysteryBox(block.transform.position);

                        var go = block.gameObject;
                        if (go != null)
                        {
                            var audioPlayer = AudioPlayer.CreateOrGet($"MysteryBoxSound{UnityEngine.Random.Range(1, 10000)}", onIntialCreation: p =>
                            {
                                var speaker = p.AddSpeaker($"Main{UnityEngine.Random.Range(1, 10000)}", isSpatial: true, minDistance: 1f, maxDistance: 15f);
                                speaker.transform.SetParent(go.transform, false);
                            });

                            audioPlayer.AddClip("mysterybox", loop: false, volume: GockelsAIO.Instance.Config.MysteryBoxMusicVolume, destroyOnEnd: true);
                        }
                        break;
                }
            }

            TrackedSchematics.Remove(schematic);
        }

        public void OnButtonInteractGobblegum(ProjectMER.Events.Arguments.ButtonInteractedEventArgs ev)
        {
            foreach (var schematic in TrackedGobblegumMachines.ToList())
            {
                if (schematic == ev.Schematic)
                {
                    if (PointSystem.GetPoints(ev.Player) >= 200)
                    {
                        PointSystem.RemovePoints(ev.Player, 200);
                        Log.Debug($"[Debug] Button-Interaction from Schematic '{schematic.name}' detected.");

                        uint randomGobblegum = GobblegumIDs[UnityEngine.Random.Range(0, GobblegumIDs.Count())];

                        CustomItem.TryGive(ev.Player, randomGobblegum);
                    }
                    else
                    {
                        ev.Player.SendBroadcast("<color=red>You need 200 points!</color>", 5);
                        break;
                    }
                }
            }
        }

        public void OnButtonInteractCoin(ProjectMER.Events.Arguments.ButtonInteractedEventArgs ev)
        {
            foreach (var schematic in TrackedCoins.ToList())
            {
                if (schematic == ev.Schematic)
                {
                    PointSystem.AddPoints(ev.Player, 1500);
                    TrackedCoins.Remove(schematic);
                    schematic.Destroy();
                }
            }
        }

        public void OnSchematicSpawn(ProjectMER.Events.Arguments.SchematicSpawnedEventArgs ev)
        {
            if (ev.Schematic == null)
            {
                Log.Warn("[Debug] SchematicSpawned-Event delivered a NULL-schematic.");
                return;
            }

            Player player = Player.Get(3);

            ev.Schematic.transform.parent = player.Transform;

            Vector3 relativeOffset = new Vector3(0f, -1f, 1f);
            ev.Schematic.transform.localPosition = relativeOffset;
            ev.Schematic.transform.localRotation = Quaternion.identity;

        }
    }
}
