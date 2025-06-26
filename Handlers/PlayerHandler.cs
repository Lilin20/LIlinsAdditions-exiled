using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups.Projectiles;
using Exiled.CustomRoles.API;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp914;
using GockelsAIO_exiled.Abilities.Active;
using GockelsAIO_exiled.Features;
using HintServiceMeow.Core.Utilities;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hint = HintServiceMeow.Core.Models.Hints.Hint;

namespace GockelsAIO_exiled.Handlers
{
    public class PlayerHandler
    {
        public static Dictionary<Player, int> PlayerPoints = new();

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
                        AutoText = display => $"{GetContent(ev.Player)}",
                    };

                    hint.Alignment = HintServiceMeow.Core.Enum.HintAlignment.Left;
                    hint.FontSize = 32;
                    hint.XCoordinate = Helper.HelperScripts.GetLeftXPosition(ev.Player.ReferenceHub.aspectRatioSync.AspectRatio);
                    hint.YCoordinate = 500;

                    PlayerDisplay playerDisplay = PlayerDisplay.Get(ev.Player);
                    playerDisplay.AddHint(hint);
                }
            });
        }

        public void OnInteractLocker(InteractingLockerEventArgs ev)
        {

        }

        public void OnThrowingGrenade(ThrownProjectileEventArgs ev)
        {
            if (ev.Projectile is ExplosionGrenadeProjectile grenade)
            {
                grenade.Base._playerDamageOverDistance = grenade.Base._playerDamageOverDistance.Multiply(0f);
            }
        }

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
                PointSystem.AddPoints(killer, 200); // z. B. 200 Punkte für Kills
                Log.Debug($"[PointSystem] {killer.Nickname} hat einen Gegner ({victim.Nickname}) getötet und 200 Punkte erhalten.");
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

        public void OnPlayerIn914(UpgradingPlayerEventArgs ev)
        {
            if (ev.KnobSetting == Scp914.Scp914KnobSetting.Rough)
            {
                if (UnityEngine.Random.value <= 0.9f)
                {
                    // 50% seiner aktuellen HP abziehen
                    float damage = ev.Player.Health / 2f;
                    ev.Player.Hurt(damage);

                    // Teleportiere Spieler an zufällige Position (z.B. in eine zufällige Room-Position)
                    var randomRoom = Room.Random(ZoneType.LightContainment);
                    Log.Info(randomRoom);
                    if (randomRoom != null)
                    {
                        Log.Info(randomRoom.Name);
                        Timing.CallDelayed(0.15f, () =>
                        {
                            ev.Player.Teleport(randomRoom.Position + UnityEngine.Vector3.up);
                        });
                    }
                }
            }
        }

        //Quest-System relevant - erstmal Disabled!
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

        public static IEnumerator<float> AddPointsOverTime()
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

        public static void TeleportPlayerToRoom(Player player, Room room, Vector3 localPos, Vector3 localRot)
        {
            // Lokale Position in globale Weltposition umwandeln
            Vector3 globalPosition = room.transform.localToWorldMatrix * new Vector4(localPos.x, localPos.y, localPos.z, 1);
            Quaternion globalRotation = room.transform.rotation * Quaternion.Euler(localRot);

            // Spieler teleportieren
            player.Position = globalPosition;
            player.Rotation = globalRotation;
        }

        public static string GetContent(Player player)
        {
            if (!PlayerPoints.ContainsKey(player))
                return $"💰: -";

            return $"💰: {PointSystem.GetPoints(player)}";
        }
    }
}
