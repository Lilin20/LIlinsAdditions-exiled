using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using GockelsAIO_exiled.Features;
using ProjectMER.Features.Objects;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GockelsAIO_exiled.Items;
using UnityEngine;

namespace GockelsAIO_exiled.Handlers
{
    public class PMERHandler
    {
        private const string CHEST_OPEN_BLOCK_NAME = "ChestOpenTop";
        private const string WEAPON_SPAWN_BLOCK_NAME = "WeaponSpawn";
        private const string ANIMATION_TRIGGER = "StartAnim";
        private const float AUDIO_MIN_DISTANCE = 1f;
        private const float AUDIO_MAX_DISTANCE = 15f;
        private const int BROADCAST_DURATION = 5;
        
        public static HashSet<SchematicObject> TrackedSchematics { get; } = new();
        public static HashSet<SchematicObject> TrackedGobblegumMachines { get; } = new();
        public static HashSet<SchematicObject> TrackedCoins { get; } = new();
        
        private static readonly uint[] GobblegumIDs = new uint[]
        {
            800, 801, 802, 803, 805, 806, 807, 808,
            809, 810, 811, 812, 813, 814, 815, 816
        };
        
        private Config CachedConfig => LilinsAdditions.Instance.Config;
        
        #region Mystery Box
        
        public void OnButtonInteract(ProjectMER.Events.Arguments.ButtonInteractedEventArgs ev)
        {
            if (!TrackedSchematics.Contains(ev.Schematic))
                return;
            
            var config = CachedConfig;
            if (!TryPurchase(ev.Player, CachedConfig.PointsForMysteryBox, config.MysteryBoxMissingPointsText))
                return;
            
            Log.Debug($"[MysteryBox] Interaction from '{ev.Schematic.name}' by {ev.Player.Nickname}");
            
            ProcessMysteryBox(ev.Schematic, config);
            TrackedSchematics.Remove(ev.Schematic);
        }
        
        private void ProcessMysteryBox(SchematicObject schematic, Config config)
        {
            foreach (var block in schematic.AttachedBlocks)
            {
                switch (block.name)
                {
                    case CHEST_OPEN_BLOCK_NAME:
                        TriggerChestAnimation(block);
                        break;

                    case WEAPON_SPAWN_BLOCK_NAME:
                        SpawnWeaponWithAudio(block, config);
                        break;
                }
            }
        }
        
        private static void TriggerChestAnimation(GameObject block)
        {
            block.gameObject.GetComponent<Animator>()?.SetTrigger(ANIMATION_TRIGGER);
        }

        private static void SpawnWeaponWithAudio(GameObject block, Config config)
        {
            WeaponSelector.StartMysteryBox(block.transform.position);

            var gameObject = block.gameObject;
            if (gameObject == null)
                return;

            CreateMysteryBoxAudio(gameObject, config);
        }

        private static void CreateMysteryBoxAudio(GameObject parent, Config config)
        {
            var randomId = UnityEngine.Random.Range(1, 10000);
            var audioPlayer = AudioPlayer.CreateOrGet($"MysteryBoxSound{randomId}", onIntialCreation: player =>
            {
                var speaker = player.AddSpeaker(
                    $"Main{UnityEngine.Random.Range(1, 10000)}", 
                    isSpatial: true, 
                    minDistance: AUDIO_MIN_DISTANCE, 
                    maxDistance: AUDIO_MAX_DISTANCE
                );
                speaker.transform.SetParent(parent.transform, worldPositionStays: false);
            });

            audioPlayer.AddClip("mysterybox", loop: false, volume: config.MysteryBoxMusicVolume, destroyOnEnd: true);
        }
        
        #endregion

        #region Gobblegum Machine

        private static List<CustomItem> _cachedBuyableGobblegums;
        
        public void OnButtonInteractGobblegum(ProjectMER.Events.Arguments.ButtonInteractedEventArgs ev)
        {
            // O(1) statt O(n) Lookup
            if (!TrackedGobblegumMachines.Contains(ev.Schematic))
                return;

            var config = CachedConfig;
            if (!TryPurchase(ev.Player, config.PointsForVendingMachine, config.VendingMachineMissingPointsText))
                return;

            Log.Debug($"[Gobblegum] Interaction from '{ev.Schematic.name}' by {ev.Player.Nickname}");
            
            GiveRandomGobblegum(ev.Player);
        }

        private static void GiveRandomGobblegum(Player player)
        {
            // Lazy initialization des Caches
            if (_cachedBuyableGobblegums == null)
            {
                _cachedBuyableGobblegums = CustomItem.Registered
                    .Where(item => item is FortunaFizzItem fizz && fizz.Buyable)
                    .ToList();
            }
    
            if (_cachedBuyableGobblegums.Count == 0)
            {
                Log.Warn("[Gobblegum] No buyable gobblegums available!");
                return;
            }
    
            var randomItem = _cachedBuyableGobblegums[UnityEngine.Random.Range(0, _cachedBuyableGobblegums.Count)];
            CustomItem.TryGive(player, randomItem.Id);
        }

        #endregion

        #region Coin Collection

        public void OnButtonInteractCoin(ProjectMER.Events.Arguments.ButtonInteractedEventArgs ev)
        {
            // O(1) statt O(n) Lookup
            if (!TrackedCoins.Contains(ev.Schematic))
                return;

            PointSystem.AddPoints(ev.Player, CachedConfig.PointsForCoin);
            
            TrackedCoins.Remove(ev.Schematic);
            ev.Schematic.Destroy();

            Log.Debug($"[Coin] Collected by {ev.Player.Nickname}");
        }

        #endregion
        
        #region Utility

        private static bool TryPurchase(Player player, int cost, string insufficientPointsMessage)
        {
            var currentPoints = PointSystem.GetPoints(player);
            
            if (currentPoints < cost)
            {
                player.Broadcast(BROADCAST_DURATION, insufficientPointsMessage);
                return false;
            }

            PointSystem.RemovePoints(player, cost);
            return true;
        }

        #endregion
    }
}
