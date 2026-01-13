using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using MEC;
using UnityEngine;

namespace GockelsAIO_exiled.Items.SCPs
{
    [CustomItem(ItemType.SCP268)]
    public class Gasmask : GogglesItem
    {
        private const int SPAWN_CHANCE = 25;
        private const float INITIAL_DAMAGE = 0f;
        private const float MAX_DAMAGE = 10f;
        private const float DAMAGE_INCREMENT = 0.2f;
        private const float DAMAGE_INTERVAL = 1f;

        // SCP-106 room teleport coordinates
        private static readonly Vector3 DimensionPosition = new(5.66f, 10.233f, -10.88f);
        private static readonly Vector3 DimensionRotation = Vector3.zero;

        private readonly Dictionary<Player, PlayerDimensionState> _activePlayers = new();

        public override uint Id { get; set; } = 999;
        public override string Name { get; set; } = "SCP-1499";
        public override string Description { get; set; } = "Sends you to another dimension.";
        public override float Weight { get; set; } = 0.5f;
        public override SpawnProperties SpawnProperties { get; set; } = new()
        {
            Limit = 1,
            LockerSpawnPoints = new List<LockerSpawnPoint>
            {
                new()
                {
                    Chance = SPAWN_CHANCE,
                    Zone = ZoneType.LightContainment,
                    UseChamber = false,
                    Type = LockerType.Misc,
                },
                new()
                {
                    Chance = SPAWN_CHANCE,
                    Zone = ZoneType.HeavyContainment,
                    UseChamber = false,
                    Type = LockerType.Misc,
                },
                new()
                {
                    Chance = SPAWN_CHANCE,
                    Zone = ZoneType.Entrance,
                    UseChamber = false,
                    Type = LockerType.Misc,
                }
            },
        };

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Left += OnPlayerLeft;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeft;
            base.UnsubscribeEvents();
        }

        protected override void EquipGoggles(Player player, bool showMessage = true)
        {
            base.EquipGoggles(player, showMessage);

            if (_activePlayers.ContainsKey(player))
            {
                Log.Warn($"[SCP-1499] {player.Nickname} already in dimension");
                return;
            }

            var state = new PlayerDimensionState
            {
                OriginalPosition = player.Position,
                CurrentDamage = INITIAL_DAMAGE
            };

            var larryRoom = Room.Get(RoomType.Hcz106);
            if (larryRoom == null)
            {
                Log.Error($"[SCP-1499] Failed to find SCP-106 room");
                return;
            }

            TeleportPlayerToRoom(player, larryRoom, DimensionPosition, DimensionRotation);
            state.DamageCoroutine = Timing.RunCoroutine(ApplyDamageOverTime(player));
            
            _activePlayers[player] = state;
            Log.Debug($"[SCP-1499] {player.Nickname} entered dimension");
        }

        protected override void RemoveGoggles(Player player, bool showMessage = true)
        {
            base.RemoveGoggles(player, showMessage);

            if (!_activePlayers.TryGetValue(player, out var state))
                return;

            player.Teleport(state.OriginalPosition);

            if (state.DamageCoroutine.IsRunning)
                Timing.KillCoroutines(state.DamageCoroutine);

            _activePlayers.Remove(player);
            Log.Debug($"[SCP-1499] {player.Nickname} exited dimension");
        }

        private void OnPlayerLeft(Exiled.Events.EventArgs.Player.LeftEventArgs ev)
        {
            if (_activePlayers.TryGetValue(ev.Player, out var state))
            {
                if (state.DamageCoroutine.IsRunning)
                    Timing.KillCoroutines(state.DamageCoroutine);

                _activePlayers.Remove(ev.Player);
                Log.Debug($"[SCP-1499] {ev.Player.Nickname} disconnected from dimension");
            }
        }

        private IEnumerator<float> ApplyDamageOverTime(Player player)
        {
            if (!_activePlayers.TryGetValue(player, out var state))
                yield break;

            while (true)
            {
                if (player == null || !player.IsAlive || !_activePlayers.ContainsKey(player))
                    yield break;

                if (state.CurrentDamage > 0)
                    player.Health -= state.CurrentDamage;

                state.CurrentDamage = Mathf.Min(state.CurrentDamage + DAMAGE_INCREMENT, MAX_DAMAGE);

                yield return Timing.WaitForSeconds(DAMAGE_INTERVAL);
            }
        }

        private static void TeleportPlayerToRoom(Player player, Room room, Vector3 localPos, Vector3 localRot)
        {
            var globalPosition = room.transform.localToWorldMatrix * new Vector4(localPos.x, localPos.y, localPos.z, 1);
            var globalRotation = room.transform.rotation * Quaternion.Euler(localRot);

            player.Position = globalPosition;
            player.Rotation = globalRotation;
        }

        private class PlayerDimensionState
        {
            public Vector3 OriginalPosition { get; set; }
            public float CurrentDamage { get; set; }
            public CoroutineHandle DamageCoroutine { get; set; }
        }
    }
}
