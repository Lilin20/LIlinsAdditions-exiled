using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.CustomItems.API.Features;
using Exiled.CustomRoles.API;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp914;
using GockelsAIO_exiled.Abilities.Active;
using GockelsAIO_exiled.Features;
using MEC;
using PlayerRoles;
using PlayerRoles.Voice;
using RueI.API;
using RueI.API.Elements;
using RueI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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
                    RueDisplay display = RueDisplay.Get(ev.Player);

                    DynamicElement de = new DynamicElement(position: 500f, _ => GetContent(ev.Player))
                    {
                        UpdateInterval = TimeSpan.FromSeconds(1),
                        VerticalAlign = RueI.API.Elements.Enums.VerticalAlign.Center,
                        ResolutionBasedAlign = true,
                        ZIndex = 100,
                    };

                    display.Show(new Tag(), de);
                    


                    //Hint hint = new Hint
                    //{
                    //    AutoText = display => $"{GetContent(ev.Player)}",
                    //};

                    //hint.Alignment = HintServiceMeow.Core.Enum.HintAlignment.Left;
                    //hint.FontSize = 32;
                    //hint.XCoordinate = Helper.HelperScripts.GetLeftXPosition(ev.Player.ReferenceHub.aspectRatioSync.AspectRatio);
                    //hint.YCoordinate = 500;

                    //PlayerDisplay playerDisplay = PlayerDisplay.Get(ev.Player);
                    //playerDisplay.AddHint(hint);
                }
            });
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
                    Log.Debug($"[PointSystem] {ev.Player.Nickname} was added with 0 Points.");
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
                Log.Debug($"[PointSystem] {killer.Nickname} has killed an enemy ({victim.Nickname}) and received 200 points.");
            }
        }

        public void OnLeft(LeftEventArgs ev)
        {
            if (PlayerPoints.ContainsKey(ev.Player))
            {
                PlayerPoints.Remove(ev.Player);
                Log.Debug($"Player {ev.Player.Nickname} was removed from the dictionary.");
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
                        RoomType.Hcz096,
                        RoomType.HczIncineratorWayside,
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
            if (ev.Player.GetCustomRoles().All(r => r.Name != "MTF Nu-7")) return;

            if (!RiotShield.activeShields.TryGetValue(ev.Player, out var shield)) return;

            Transform shieldTransform = shield.transform;

            if (ev.Item is Firearm)
            {
                // Spieler hat Waffe -> Schild zur Seite drehen
                shieldTransform.localPosition = new Vector3(-0.4f, -1f, 0.4f); // leicht links
                shieldTransform.localRotation = Quaternion.Euler(0f, -70f, 0f); // seitlich
                Log.Debug($"[{ev.Player.Nickname}] weapon detected – shield rotated to the left.");
            }
            else
            {
                // Standardausrichtung
                shieldTransform.localPosition = new Vector3(0f, -1f, 0.5f);
                shieldTransform.localRotation = Quaternion.identity;
                Log.Debug($"[{ev.Player.Nickname}] no weapon detected. – shield centered.");
            }
        }

        public void OnPlayerIn914(UpgradingPlayerEventArgs ev)
        {
            if (LilinsAdditions.Instance.Config.Enable914Teleport)
            {
                if (ev.KnobSetting == Scp914.Scp914KnobSetting.Rough)
                {
                    if (UnityEngine.Random.value <= 0.9f)
                    {
                        float damage = ev.Player.Health / 2f;
                        ev.Player.Hurt(damage);

                        var randomRoom = Room.Random(ZoneType.LightContainment);
                        Log.Debug(randomRoom);
                        if (randomRoom != null)
                        {
                            Log.Debug(randomRoom.Name);
                            Timing.CallDelayed(0.15f, () =>
                            {
                                ev.Player.Teleport(randomRoom.Position + UnityEngine.Vector3.up);
                            });
                        }
                    }
                }
            }
        }

        public void OnCraftingTrackingAccess(Exiled.Events.EventArgs.Scp914.UpgradingPickupEventArgs ev)
        {
            if (ev.KnobSetting != Scp914.Scp914KnobSetting.Fine) return;

            if (ev.Pickup.Type == ItemType.KeycardO5)
            {
                ev.IsAllowed = false;

                var customOutput = CustomItem.TrySpawn(4444, ev.OutputPosition, out Pickup customPickuop);
                ev.Pickup.Destroy();
            }
        }

        private bool _isRunning = false; // debounce flag

        public void OnUsingIntercomWithCard(IntercomSpeakingEventArgs args)
        {
            if (_isRunning) // if already running, ignore new activations
            {
                args.IsAllowed = false;
                return;
            }

            if (args.Player.CurrentItem == null)
                return;

            if (CustomItem.TryGet(args.Player.CurrentItem, out CustomItem customItem))
            {
                if (customItem is CustomKeycard customKeycard)
                {
                    if (customKeycard.Id == 4444)
                    {
                        args.IsAllowed = false;
                        _isRunning = true; // lock sequence

                        int totalSteps = 5;
                        float duration = 10f;
                        float stepTime = duration / totalSteps;

                        // Progress bar display
                        for (int i = 1; i <= totalSteps; i++)
                        {
                            int progress = i;
                            Timing.CallDelayed(progress * stepTime, () =>
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.Append("Loading...\n");
                                sb.Append('[');
                                sb.Append(new string('█', progress));
                                sb.Append(new string('-', totalSteps - progress));
                                sb.Append(']');

                                IntercomDisplay._singleton.Network_overrideText = sb.ToString();
                            });
                        }

                        // After progress finishes
                        Timing.CallDelayed(duration + 0.5f, () =>
                        {
                            Player executor = args.Player;
                            string message;

                            // Check if this player has a selected target
                            if (Storage.SelectedPlayers.TryGetValue(executor, out Player selected) && selected.IsAlive)
                            {
                                string zone = selected.Zone.ToString();
                                string room = selected.CurrentRoom?.Name ?? "Unknown Room";

                                message = $"{selected.Nickname} detected\nZone: {zone}\nRoom: {room}";

                                // Play tracking sound ONCE
                                var audioPlayer = AudioPlayer.CreateOrGet(
                                    $"TrackingSound_{UnityEngine.Random.Range(1, 10000)}",
                                    onIntialCreation: p =>
                                    {
                                        var speaker = p.AddSpeaker(
                                            $"Main_{UnityEngine.Random.Range(1, 10000)}",
                                            isSpatial: true,
                                            minDistance: 20f,
                                            maxDistance: 30f);
                                        speaker.transform.SetParent(selected.Transform, false);
                                    });

                                audioPlayer.AddClip("trackingsound", loop: false, volume: 1.25f, destroyOnEnd: true);
                                Timing.CallDelayed(2.5f, () =>
                                {
                                    selected.EnableEffect(EffectType.Blurred, 1, 2);
                                    selected.EnableEffect(EffectType.Flashed, 1, 1);

                                    selected.ShowHint("You're being tracked.", 5f);
                                });
                            }
                            else
                            {
                                message = "No player selected.";
                            }

                            IntercomDisplay._singleton.Network_overrideText = message;

                            // Reset intercom text and unlock debounce after 5 seconds
                            Timing.CallDelayed(5f, () =>
                            {
                                IntercomDisplay._singleton.Network_overrideText = null;
                                _isRunning = false; // unlock for next use
                            });
                        });
                    }
                }
            }
        }

        public static IEnumerator<float> AddPointsOverTime()
        {
            Log.Debug("[AddPointsOverTime] Coroutine started.");

            while (true)
            {
                foreach (var kvp in PlayerPoints.ToList())
                {
                    Player player = kvp.Key;

                    if (player != null && player.IsAlive)
                    {
                        PlayerPoints[player] += 100;
                        Log.Debug($"[AddPointsOverTime] {player.Nickname} +100 Points => {PlayerPoints[player]}");
                    }
                    else
                    {
                        Log.Debug($"[AddPointsOverTime] {player?.Nickname ?? "???"} is not alive or null.");
                    }
                }

                yield return Timing.WaitForSeconds(LilinsAdditions.Instance.Config.PointsOverTimeDelay);
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
            StringBuilder sb = new();
            sb.SetAlignment(RueI.Utils.Enums.AlignStyle.Left);

            if (!PlayerPoints.ContainsKey(player))
            {
                sb.Append($"💰: -");
                return sb.ToString();
            }

            sb.Append($"💰: {PointSystem.GetPoints(player)}");

            return sb.ToString();
        }
    }
}
