using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.CustomItems.API.Features;
using Exiled.CustomRoles.API;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp914;
using Exiled.Events.EventArgs.Server;
using GockelsAIO_exiled.Abilities.Active;
using GockelsAIO_exiled.Features;
using HintServiceMeow.Core.Utilities;
using MEC;
using Mirror;
using PlayerRoles;
using ProjectMER.Features.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hint = HintServiceMeow.Core.Models.Hints.Hint;

namespace GockelsAIO_exiled
{
    public class EventHandlers
    {
        public static readonly List<SchematicObject> TrackedSchematics = new();
        public static readonly List<SchematicObject> TrackedGobblegumMachines= new();
        public static readonly List<SchematicObject> TrackedCoins = new();
        public SchematicSpawner ss = new();
        public static CoroutineHandle tickCoroutine;
        private CoroutineHandle _followToyCoroutine;

        public static readonly List<uint> GobblegumIDs = new()
        {
            800,
            801,
            802,
            803,
            804,
            805,
            806,
            807,
            808,
            809,
            810,
            811,
            812,
            813,
            814
        };

        public void TeleportPlayerToRoom(Player player, Room room, Vector3 localPos, Vector3 localRot)
        {
            // Lokale Position in globale Weltposition umwandeln
            Vector3 globalPosition = room.transform.localToWorldMatrix * new Vector4(localPos.x, localPos.y, localPos.z, 1);
            Quaternion globalRotation = room.transform.rotation * Quaternion.Euler(localRot);

            // Spieler teleportieren
            player.Position = globalPosition;
            player.Rotation = globalRotation;
        }

        public static Dictionary<Player, int> PlayerPoints = new();

        public void OnChangingRolePoints(ChangingRoleEventArgs ev)
        {
            if (ev.Player == null)
                return;

            // Entfernen, wenn Spieler zu totem Zustand wechselt
            if (ev.NewRole == RoleTypeId.Spectator ||
                ev.NewRole == RoleTypeId.Overwatch ||
                ev.NewRole == RoleTypeId.Destroyed ||
                ev.NewRole == RoleTypeId.None ||
                ev.NewRole == RoleTypeId.Filmmaker ||
                ev.NewRole == RoleTypeId.Tutorial)
            {
                if (PlayerPoints.ContainsKey(ev.Player))
                {
                    PlayerPoints.Remove(ev.Player);
                    Log.Debug($"[PointSystem] {ev.Player.Nickname} wurde wegen Tod entfernt.");
                }

                return;
            }

            // Nur hinzufügen, wenn Team gültig ist
            var team = Exiled.API.Extensions.RoleExtensions.GetTeam(ev.NewRole);
            if (team == Team.FoundationForces ||
                team == Team.ChaosInsurgency ||
                team == Team.Scientists ||
                team == Team.ClassD)
            {
                if (!PlayerPoints.ContainsKey(ev.Player))
                {
                    PlayerPoints[ev.Player] = 400;
                    Log.Debug($"[PointSystem] {ev.Player.Nickname} wurde mit 0 Punkten eingetragen.");

                    //QuestManager.Instance.AssignQuest(ev.Player, new Quest
                    //{
                    //    Id = "Kill Scientists",
                    //    Type = QuestType.KillEnemy,
                    //    Target = "Scientists",
                    //    RequiredAmount = 3,
                    //    RewardPoints = 20
                    //});

                    //QuestManager.Instance.ShowActiveQuests(ev.Player);
                }
            }
        }

        public void OnPlayerDied(DyingEventArgs ev)
        {
            if (ev.Attacker == null || ev.Attacker == ev.Player)
            {
                Log.Info("Todes-Event ignoriert: Kein Angreifer oder Selbstmord.");
                return;
            }

            var killer = ev.Attacker;

            if (ev.Player.Role.Team == killer.Role.Team)
            {
                Log.Info($"Todes-Event ignoriert: {killer.Nickname} und {ev.Player.Nickname} sind im selben Team ({killer.Role.Team}).");
                return; // Nur gegnerische Teams
            }

            Log.Info($"Todes-Event: {killer.Nickname} ({killer.Role.Team}) hat {ev.Player.Nickname} ({ev.Player.Role.Team}) getötet.");

            var quests = QuestManager.Instance.GetPlayerProgress(killer);

            foreach (var progress in quests.Where(q => q.Quest.Type == QuestType.KillEnemy))
            {
                Log.Info($"Prüfe Quest: {progress.Quest.Id} – Zielteam: {progress.Quest.Target}, Benötigt: {progress.Quest.RequiredAmount}, Aktuell: {progress.Progress}");

                if (progress.Quest.Target == ev.Player.Role.Team.ToString())
                {
                    if (!progress.IsCompleted)
                    {
                        progress.AddProgress();
                        Log.Info($"Fortschritt für {killer.Nickname} in Quest '{progress.Quest.Id}': {progress.Progress}/{progress.Quest.RequiredAmount}");

                        if (progress.IsCompleted)
                        {
                            Log.Info($"Quest '{progress.Quest.Id}' abgeschlossen! Belohnung wird vergeben.");
                            QuestManager.Instance.GrantReward(killer, progress);
                        }
                    }
                    else
                    {
                        Log.Info($"Quest '{progress.Quest.Id}' ist bereits abgeschlossen. Kein weiterer Fortschritt.");
                    }
                    Log.Info($"Fortschritt für {killer.Nickname} in Quest '{progress.Quest.Id}': {progress.Progress}/{progress.Quest.RequiredAmount}");

                    if (progress.IsCompleted)
                    {
                        Log.Info($"Quest '{progress.Quest.Id}' abgeschlossen! Belohnung wird vergeben.");
                        QuestManager.Instance.GrantReward(killer, progress);
                    }
                }
                else
                {
                    Log.Info($"Zielteam passt nicht: Erwartet '{progress.Quest.Target}', aber getötet wurde '{ev.Player.Role.Team}'.");
                }
            }
        }

        public void OnKillGivePoints(DyingEventArgs ev)
        {
            if (ev.Attacker == null || ev.Player == null || ev.Attacker == ev.Player)
                return;

            Player killer = ev.Attacker;
            Player victim = ev.Player;

            var killerTeam = killer.Role.Team;
            var victimTeam = victim.Role.Team;

            bool isEnemy = false;

            // Gegenseitige Feindlogik
            switch (killerTeam)
            {
                case Team.ClassD:
                    isEnemy = victimTeam == Team.FoundationForces || victimTeam == Team.Scientists || victimTeam == Team.SCPs;
                    break;
                case Team.FoundationForces:
                    isEnemy = victimTeam == Team.ClassD || victimTeam == Team.ChaosInsurgency || victimTeam == Team.SCPs;
                    break;
                case Team.ChaosInsurgency:
                    isEnemy = victimTeam == Team.FoundationForces || victimTeam == Team.Scientists || victimTeam == Team.SCPs;
                    break;
                case Team.Scientists:
                    isEnemy = victimTeam == Team.ClassD || victimTeam == Team.ChaosInsurgency || victimTeam == Team.SCPs;
                    break;
            }

            if (isEnemy && PlayerPoints.ContainsKey(killer))
            {
                AddPoints(killer, 200); // z. B. 200 Punkte für Kills
                Log.Debug($"[PointSystem] {killer.Nickname} hat einen Gegner ({victim.Nickname}) getötet und 200 Punkte erhalten.");
            }
        }

        public void SpawnSetPoints(SpawnedEventArgs ev)
        {
            Timing.CallDelayed(1, () =>
            {
                if (ev.Player.Role.Team == Team.ClassD ||
                ev.Player.Role.Team == Team.FoundationForces ||
                ev.Player.Role.Team == Team.ChaosInsurgency ||
                ev.Player.Role.Team == Team.Scientists)
                {
                    if (!PlayerPoints.ContainsKey(ev.Player))
                    {
                        PlayerPoints.Add(ev.Player, 800);
                    }

                    Hint hint = new Hint
                    {
                        AutoText = display => $"{GockelsAIO.GetContent(ev.Player)}",
                    };

                    hint.Alignment = HintServiceMeow.Core.Enum.HintAlignment.Left;
                    hint.FontSize = 32;
                    hint.XCoordinate = GetLeftXPosition(ev.Player.ReferenceHub.aspectRatioSync.AspectRatio);
                    hint.YCoordinate = 500;

                    PlayerDisplay playerDisplay = PlayerDisplay.Get(ev.Player);
                    playerDisplay.AddHint(hint);
                }
            });
        }

        public static void AddPoints(Player player, int points)
        {
            if (PlayerPoints.ContainsKey(player))
            {
                PlayerPoints[player] += points;
                Log.Debug($"Spieler {player.Nickname} hat jetzt {PlayerPoints[player]} Punkte.");
            }
        }

        public void OnLeft(LeftEventArgs ev)
        {
            if (PlayerPoints.ContainsKey(ev.Player))
            {
                PlayerPoints.Remove(ev.Player);
                Log.Debug($"Spieler {ev.Player.Nickname} wurde aus dem Dictionary entfernt.");
            }
        }

        public static int GetPoints(Player player)
        {
            if (PlayerPoints.TryGetValue(player, out int points))
                return points;

            return 0; // Falls Spieler nicht gefunden wird, z. B. wenn er noch nicht gespawnt ist
        }

        public static void SetPoints(Player player, int points)
        {
            if (PlayerPoints.ContainsKey(player))
            {
                PlayerPoints[player] = points;
                Log.Debug($"Spieler {player.Nickname} hat jetzt {PlayerPoints[player]} Punkte.");
            }
        }

        public static void RemovePoints(Player player, int pointsToRemove)
        {
            if (PlayerPoints.TryGetValue(player, out int currentPoints))
            {
                int newPoints = Math.Max(0, currentPoints - pointsToRemove);
                PlayerPoints[player] = newPoints;
            }
        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            Timing.KillCoroutines(tickCoroutine);
        }

        public void OnPickingUp(PickingUpItemEventArgs ev)
        {

        }

        public void DEBUGGrenadeThrow(ExplodingGrenadeEventArgs ev)
        {
            Log.Info(ev.Projectile.GameObject.layer);
        }

        public void OnStart()
        {
            ss.SpawnMysteryBoxes(8);
            ss.SpawnTrapTest();
            ss.SpawnGobblegumMachines(5);
            ss.SpawnCoins(1);

            foreach (var player in Player.List)
            {
                if (player.Role.Team is Team.FoundationForces or Team.ChaosInsurgency or Team.Scientists or Team.ClassD)
                {
                    if (!PlayerPoints.ContainsKey(player))
                    {
                        PlayerPoints[player] = 10000;
                        Log.Debug($"[OnStart] {player.Nickname} startet mit 800 Punkten.");
                    }
                }
            }

            ADSMonitor.Start();

            foreach (var player in Player.List)
            {
                Log.Debug($"[Check] {player.Nickname} | Alive: {player.IsAlive} | InPoints: {PlayerPoints.ContainsKey(player)}");
            }

            Timing.CallDelayed(120, () =>
            {
                tickCoroutine = Timing.RunCoroutine(AddPointsOverTime());
            });
        }

        public void OnButtonInteract(ProjectMER.Events.Arguments.ButtonInteractedEventArgs ev)
        {
            var schematic = TrackedSchematics.FirstOrDefault(s => s == ev.Schematic);
            if (schematic == null) return;

            int playerPoints = GetPoints(ev.Player);
            if (playerPoints < 800)
            {
                ev.Player.SendBroadcast("<color=red>Du brauchst mindestens 800 Punkte!</color>", 5);
                Log.Debug($"Spieler {ev.Player.Nickname} hatte nur {playerPoints} Punkte.");
                return;
            }

            RemovePoints(ev.Player, 800);
            Log.Info($"[Debug] Button-Interaktion in bekanntem Schematic '{schematic.name}' erkannt.");

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

                            audioPlayer.AddClip("mysterybox", loop: false, volume: 1.5f, destroyOnEnd: true);
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
                    if (GetPoints(ev.Player) >= 200)
                    {
                        RemovePoints(ev.Player, 200);
                        Log.Info($"[Debug] Button-Interaktion in bekanntem Schematic '{schematic.name}' erkannt.");

                        uint randomGobblegum = GobblegumIDs[UnityEngine.Random.Range(0, GobblegumIDs.Count())];

                        CustomItem.TryGive(ev.Player, randomGobblegum);
                    }
                    else
                    {
                        ev.Player.SendBroadcast("<color=red>Du brauchst mindestens 200 Punkte!</color>", 5);
                        Log.Debug($"Spieler {ev.Player.Nickname} hatte nur {GetPoints(ev.Player)} Punkte.");
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
                    AddPoints(ev.Player, 1500);
                    TrackedCoins.Remove(schematic);
                    schematic.Destroy();
                }
            }
        }

        public void OnSchematicSpawn(ProjectMER.Events.Arguments.SchematicSpawnedEventArgs ev)
        {
            if (ev.Schematic == null)
            {
                Log.Warn("[Debug] SchematicSpawned-Event hat ein NULL-Schematic geliefert.");
                return;
            }

            Player player = Player.Get(3);

            ev.Schematic.transform.parent = player.Transform;

            Vector3 relativeOffset = new Vector3(0f, -1f, 1f);
            ev.Schematic.transform.localPosition = relativeOffset;
            ev.Schematic.transform.localRotation = Quaternion.identity;

        }

        public void OnChangingItem(ChangingItemEventArgs ev)
        {
            if (!ev.Player.GetCustomRoles().Any(r => r.Name == "MTF Nu-7")) return;

            if (!RiotShield.activeShields.TryGetValue(ev.Player, out var shield)) return;
            
            Transform shieldTransform = shield.transform;

            if (ev.Item is Firearm)
            {
                // Spieler hat Waffe -> Schild zur Seite drehen
                shieldTransform.localPosition = new Vector3(-0.4f, -1f, 0.4f); // leicht links
                shieldTransform.localRotation = Quaternion.Euler(0f, -70f, 0f); // seitlich
                Log.Debug($"[{ev.Player.Nickname}] Waffe erkannt – Schild nach links gedreht.");
            }
            else
            {
                // Standardausrichtung
                shieldTransform.localPosition = new Vector3(0f, -1f, 0.5f);
                shieldTransform.localRotation = Quaternion.identity;
                Log.Debug($"[{ev.Player.Nickname}] Keine Waffe – Schild zentriert.");
            }
        }

        public void OnSpawningGuards()
        {
            Room[] allRooms = Room.List.ToArray();
            List<RoomType> forbiddenRoomTypes = new List<RoomType>
            {
                RoomType.Hcz079,
                RoomType.Hcz106,
                RoomType.HczHid,
                RoomType.Hcz096,
                RoomType.Hcz939,
                RoomType.HczTestRoom,
                RoomType.Hcz049,
                RoomType.EzCollapsedTunnel,
                RoomType.EzGateA,
                RoomType.EzGateB,
                RoomType.Lcz173,
                RoomType.HczTesla,
                RoomType.EzShelter,
                RoomType.Pocket,
                RoomType.HczCrossRoomWater,
                RoomType.Surface,
            };

            foreach (Player player in Player.List)
            {
                if (player.Role == RoleTypeId.FacilityGuard)
                {
                    Room randomRoom = allRooms[UnityEngine.Random.Range(0, allRooms.Length)];
                    while (forbiddenRoomTypes.Contains(randomRoom.Type))
                    {
                        randomRoom = allRooms[UnityEngine.Random.Range(0, allRooms.Length)];
                    }
                    Log.Info($"Guard {player.CustomName} spawned in {randomRoom.Type}");
                    player.Teleport(randomRoom.Position + Vector3.up);
                }
            }
        }

        public void OnSCPVoidJump(HurtingEventArgs ev)
        {
            if (ev.Player == null) return;

            if (ev.Player.Role.Team == Team.SCPs)
            {
                if (ev.DamageHandler.Type == DamageType.Crushed)
                {
                    List<RoomType> forbiddenRoomTypes = new List<RoomType>
                    {
                        RoomType.HczHid,
                        RoomType.HczTestRoom,
                        RoomType.EzCollapsedTunnel,
                        RoomType.EzGateA,
                        RoomType.EzGateB,
                        RoomType.Lcz173,
                        RoomType.HczTesla,
                        RoomType.EzShelter,
                        RoomType.Pocket,
                        RoomType.HczCrossRoomWater,
                    };

                    Room[] allRooms = Room.List
                        .Where(r => !forbiddenRoomTypes.Contains(r.Type) && r.Zone == ZoneType.HeavyContainment)
                        .ToArray();

                    Room randomRoom = allRooms[UnityEngine.Random.Range(0, allRooms.Length)];
                    while (forbiddenRoomTypes.Contains(randomRoom.Type) || randomRoom.Zone != ZoneType.HeavyContainment)
                    {
                        randomRoom = allRooms[UnityEngine.Random.Range(0, allRooms.Length)];
                    }

                    ev.IsAllowed = false;
                    ev.Player.Teleport(randomRoom);
                    ev.Player.IsGodModeEnabled = true;
                    Timing.CallDelayed(1f, () =>
                    {
                        ev.Player.IsGodModeEnabled = false;
                    });
                }
            }
        }

        public void OnPlayerIn914(UpgradingPlayerEventArgs ev)
        {
            if (ev.KnobSetting == Scp914.Scp914KnobSetting.Rough)
            {
                if (UnityEngine.Random.value <= 0.5f)
                {
                    // 50% seiner aktuellen HP abziehen
                    float damage = ev.Player.Health / 2f;
                    ev.Player.Hurt(damage);

                    // Teleportiere Spieler an zufällige Position (z.B. in eine zufällige Room-Position)
                    var randomRoom = Exiled.API.Features.Room.List
                        .Where(r => r.Zone == ZoneType.LightContainment && r.Type != RoomType.Pocket)
                        .ToList()
                        .GetRandomValue();

                    if (randomRoom != null)
                        ev.Player.Position = randomRoom.Position + UnityEngine.Vector3.up;
                }
            }
        }

        public static float GetLeftXPosition(float _aspectRatio)
        {
            return (622.27444f * Mathf.Pow(_aspectRatio, 3f)) + (-2869.08991f * Mathf.Pow(_aspectRatio, 2f)) + (3827.03102f * _aspectRatio) - 1580.21554f;
        }

        private static IEnumerator<float> AddPointsOverTime()
        {
            Log.Info("[AddPointsOverTime] Coroutine gestartet.");

            while (true)
            {
                foreach (var kvp in PlayerPoints.ToList())
                {
                    Player player = kvp.Key;

                    if (player != null && player.IsAlive)
                    {
                        PlayerPoints[player] += 100;
                        Log.Debug($"[AddPointsOverTime] {player.Nickname} +50 Punkte => {PlayerPoints[player]}");
                    }
                    else
                    {
                        Log.Debug($"[AddPointsOverTime] {player?.Nickname ?? "???"} ist nicht lebendig oder null.");
                    }
                }

                yield return Timing.WaitForSeconds(120);
            }
        }
    }
}
