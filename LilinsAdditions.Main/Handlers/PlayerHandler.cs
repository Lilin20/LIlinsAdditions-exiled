using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp914;
using LilinsAdditions.Features;
using LilinsAdditions.Items.Keycards;
using MEC;
using PlayerRoles;
using RueI.API;
using RueI.API.Elements;
using RueI.Utils;
using UnityEngine;

namespace LilinsAdditions.Handlers
{
    public class PlayerHandler
    {
        private const float GOD_MODE_DURATION = 1f;
        private const float TELEPORT_DELAY = 0.15f;
        private const float ROUGH_914_DAMAGE_MULTIPLIER = 0.5f;
        private const float ROUGH_914_TRIGGER_CHANCE = 0.9f;

        private static readonly HashSet<RoomType> ForbiddenScpRoomTypes = new()
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
        
        public static Dictionary<Player, int> PlayerPoints = new();
        
        #region Spawn & Display
        
        public void SpawnSetPoints(SpawnedEventArgs ev)
        {
            Timing.CallDelayed(1f, () => InitializePlayerDisplay(ev.Player));
        }

        private void InitializePlayerDisplay(Player player)
        {
            if (!IsEligibleForPointSystem(player))
                return;

            var display = RueDisplay.Get(player);
            var element = CreatePointDisplayElement(player);
            display.Show(new Tag(), element);
        }
        
        private static bool IsEligibleForPointSystem(Player player)
        {
            return player.Role.Team switch
            {
                Team.ClassD or Team.FoundationForces or Team.ChaosInsurgency or Team.Scientists => true,
                _ => false
            };
        }
        
        private static DynamicElement CreatePointDisplayElement(Player player)
        {
            return new DynamicElement(position: 500f, _ => GetContent(player))
            {
                UpdateInterval = TimeSpan.FromSeconds(1),
                VerticalAlign = RueI.API.Elements.Enums.VerticalAlign.Center,
                ResolutionBasedAlign = true,
                ZIndex = 100,
            };
        }
        
        #endregion
        
        #region Point System Events
        
        public void OnKillGivePoints(DyingEventArgs ev)
        {
            if (!IsValidKill(ev.Attacker, ev.Player))
                return;

            var killer = ev.Attacker;
            var victim = ev.Player;

            if (AreEnemies(killer.Role.Team, victim.Role.Team) && PlayerPoints.ContainsKey(killer))
            {
                var pointsToAdd = LilinsAdditions.Instance.Config.PointsForKillingEnemy;
                PointSystem.AddPoints(killer, pointsToAdd);
                
                Log.Debug($"[PointSystem] {killer.Nickname} killed {victim.Nickname} and received {pointsToAdd} points.");
            }
        }

        private static bool IsValidKill(Player attacker, Player victim)
        {
            return attacker != null && victim != null && attacker != victim;
        }

        private static bool AreEnemies(Team attackerTeam, Team victimTeam)
        {
            return attackerTeam switch
            {
                Team.ClassD => victimTeam is Team.FoundationForces or Team.Scientists or Team.SCPs,
                Team.FoundationForces => victimTeam is Team.ClassD or Team.ChaosInsurgency or Team.SCPs,
                Team.ChaosInsurgency => victimTeam is Team.FoundationForces or Team.Scientists or Team.SCPs,
                Team.Scientists => victimTeam is Team.ClassD or Team.ChaosInsurgency or Team.SCPs,
                _ => false
            };
        }
        
        public void OnLeft(LeftEventArgs ev)
        {
            RemovePlayerFromPointSystem(ev.Player, "Player left");
        }
        
        private static void RemovePlayerFromPointSystem(Player player, string reason)
        {
            if (PlayerPoints.Remove(player))
            {
                Log.Debug($"[PointSystem] {player.Nickname} removed. Reason: {reason}");
            }
        }
        
        #endregion
        
        #region SCP Anti-Suicide
        
        public void OnSCPVoidJump(HurtingEventArgs ev)
        {
            if (!ShouldPreventScpSuicide(ev))
                return;

            var safeRoom = GetRandomSafeHeavyRoom();
            if (safeRoom == null)
            {
                Log.Warn("[AntiSCPSuicide] No sfe room found!");
                return;
            }

            ev.IsAllowed = false;
            TeleportScpToSafeRoom(ev.Player, safeRoom);
        }
        
        private static Room GetRandomSafeHeavyRoom()
        {
            var safeRooms = Room.List
                .Where(r => r.Zone == ZoneType.HeavyContainment && !ForbiddenScpRoomTypes.Contains(r.Type))
                .ToArray();

            return safeRooms.Length > 0 
                ? safeRooms[UnityEngine.Random.Range(0, safeRooms.Length)] 
                : null;
        }
        
        private static bool ShouldPreventScpSuicide(HurtingEventArgs ev)
        {
            return LilinsAdditions.Instance.Config.EnableAntiSCPSuicide
                   && ev.Player != null
                   && ev.Player.Role.Team == Team.SCPs
                   && ev.DamageHandler.Type == DamageType.Crushed;
        }
        
        private static void TeleportScpToSafeRoom(Player player, Room room)
        {
            player.Teleport(room);
            player.IsGodModeEnabled = true;
        
            Timing.CallDelayed(GOD_MODE_DURATION, () => player.IsGodModeEnabled = false);
        }
        
        #endregion
        
        #region SCP-914 Events
        
        public void OnPlayerIn914(UpgradingPlayerEventArgs ev)
        {
            if (!LilinsAdditions.Instance.Config.Enable914Teleport)
                return;

            if (ev.KnobSetting == Scp914.Scp914KnobSetting.Rough && 
                UnityEngine.Random.value <= ROUGH_914_TRIGGER_CHANCE)
            {
                HandleRough914Teleport(ev.Player);
            }
        }

        private static void HandleRough914Teleport(Player player)
        {
            player.Hurt(player.Health * ROUGH_914_DAMAGE_MULTIPLIER);

            var targetRoom = Room.Random(ZoneType.LightContainment);
            if (targetRoom == null)
            {
                Log.Warn("[914Teleport] No LCZ room found for teleportation!");
                return;
            }
            
            Log.Debug($"[914Teleport] Teleporting {player.Nickname} to {targetRoom.Name}");
            Timing.CallDelayed(TELEPORT_DELAY, () => 
                player.Teleport(targetRoom.Position + UnityEngine.Vector3.up));
        }
        
        #endregion
        
        #region Credit Card System
        
        public void DropCreditOnDeath(DyingEventArgs ev)
        {
            if (LilinsAdditions.Instance.Config.EnableCreditCardDrop)
            {
                PointSystem.SpawnCreditCard(ev.Player, ev.Attacker);
            }
        }
        
        public void OnPickingUpCreditCard(PickingUpItemEventArgs ev)
        {
            if (!TryGetCreditCard(ev.Pickup, out var points))
                return;
            
            ev.IsAllowed = false;
            ev.Pickup.Destroy();
            
            PointSystem.AddPoints(ev.Player, points);
            CleanupCreditCardData(ev.Pickup.Serial);
        }
        
        private static bool TryGetCreditCard(Pickup pickup, out int points)
        {
            points = 0;

            if (!CustomItem.TryGet(pickup, out var customItem) || 
                customItem.GetType() != typeof(CreditCard))
                return false;

            var key = $"CreditCard_Points_{pickup.Serial}";
            if (!Server.SessionVariables.TryGetValue(key, out var pointsObj) || 
                pointsObj is not int extractedPoints)
                return false;

            points = extractedPoints;
            return true;
        }

        private static void CleanupCreditCardData(ushort serial)
        {
            Server.SessionVariables.Remove($"CreditCard_Points_{serial}");
        }
        
        #endregion
        
        #region Coroutines
        
        public static IEnumerator<float> AddPointsOverTime()
        {
            Log.Debug("[AddPointsOverTime] Coroutine started.");

            while (true)
            {
                var config = LilinsAdditions.Instance.Config;
            
                foreach (var kvp in PlayerPoints.ToList())
                {
                    var player = kvp.Key;
                
                    if (player?.IsAlive == true)
                    {
                        PlayerPoints[player] += config.PointsOverTime;
                        Log.Debug($"[AddPointsOverTime] {player.Nickname} +{config.PointsOverTime} => {PlayerPoints[player]}");
                    }
                }

                yield return Timing.WaitForSeconds(config.PointsOverTimeDelay);
            }
        }
        
        #endregion

        #region Utility
        
        public static void TeleportPlayerToRoom(Player player, Room room, Vector3 localPos, Vector3 localRot)
        {
            var transform = room.transform;
            var globalPosition = transform.localToWorldMatrix * new Vector4(localPos.x, localPos.y, localPos.z, 1);
            var globalRotation = transform.rotation * Quaternion.Euler(localRot);

            player.Position = globalPosition;
            player.Rotation = globalRotation;
        }

        public static string GetContent(Player player)
        {
            var sb = new StringBuilder();
            sb.SetAlignment(RueI.Utils.Enums.AlignStyle.Left);

            var points = PlayerPoints.ContainsKey(player) 
                ? PointSystem.GetPoints(player).ToString() 
                : "-";

            sb.Append($"💰: {points}");
            return sb.ToString();
        }
        
        #endregion
    }
}
