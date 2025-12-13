using System.Collections.Generic;
using System.Linq;
using AdminToys;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.API.Features.Spawn;
using Exiled.API.Features.Toys;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.Firearms.Attachments;
using MEC;
using UnityEngine;

namespace GockelsAIO_exiled.Items.Weapons.Rifles
{
    [CustomItem(ItemType.GunE11SR)]
    public class HumeBreaker : CustomWeapon
    {
        #region Configuration Properties
        public override uint Id { get; set; } = 100;
        public override string Name { get; set; } = "HumeBreaker v2.1";
        public override string Description { get; set; } = "An experimental weapon which destroys the complete hume shield. Usage could end up deadly.";
        public override float Damage { get; set; } = 1;
        public override byte ClipSize { get; set; } = 1;
        public override float Weight { get; set; } = 1.5f;
        public override SpawnProperties SpawnProperties { get; set; }
        public override AttachmentName[] Attachments { get; set; } = new[] { AttachmentName.ScopeSight };
        #endregion

        #region Effect Configuration
        private const float MaxAffectDistance = 10f;
        private const float FlashDuration = 0.5f;
        private const byte SlownessIntensity = 40;
        private const float SlownessDuration = 5f;
        private const float CoroutineCleanupDelay = 4f;
        private const int RaycastMask = ~(1 << 1 | 1 << 13 | 1 << 16 | 1 << 28);
        private const float MaxRaycastDistance = 100f;
        #endregion

        #region Visual Effect Configuration
        private const float BeamSegmentDistance = 0.05f;
        private const float BeamCubeSize = 0.05f;
        private const float SpiralCubeSize = 0.025f;
        private const float EffectFadeDuration = 2f;
        private const int DefaultBoltCount = 10;
        private const float BoltDuration = 0.2f;
        private const float BoltRandomOffset = 0.15f;
        private const int DefaultSpiralTurns = 10;
        private const float DefaultSpiralRadius = 0.1f;
        #endregion

        private readonly List<CoroutineHandle> _activeCoroutines = new List<CoroutineHandle>();

        #region Event Handlers
        protected override void OnReloading(ReloadingWeaponEventArgs ev)
        {
            ev.IsAllowed = false;
            base.OnReloading(ev);
        }

        protected override void OnShot(ShotEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem) || ev.Target == null)
            {
                ev.Firearm?.Destroy();
                return;
            }

            ev.CanHurt = false;

            HandleTargetDamage(ev);
            ApplyAreaEffects(ev.Player);
            CreateVisualEffects(ev.Player, ev.Position);
            
            ev.Firearm.Destroy();
            
            ScheduleCoroutineCleanup();
        }
        #endregion

        #region Damage Logic
        private void HandleTargetDamage(ShotEventArgs ev)
        {
            if (ev.Target.Role.Team == PlayerRoles.Team.SCPs)
            {
                DestroyHumeShield(ev.Target);
                SpawnGrenade(ev.Position, ev.Player, fuseTime: 0.01f, scpDamageMultiplier: 0f);
            }
            else
            {
                ev.Target.Vaporize();
                SpawnGrenade(ev.Position, ev.Player, fuseTime: 0.05f, maxRadius: 5f, scpDamageMultiplier: 0f);
            }
        }

        private void DestroyHumeShield(Player target)
        {
            target.HumeShield = 0;
            target.MaxHumeShield = 0;
        }

        private void SpawnGrenade(Vector3 position, Player owner, float fuseTime, float maxRadius = 0f, float scpDamageMultiplier = 0f)
        {
            ExplosiveGrenade grenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
            grenade.FuseTime = fuseTime;
            grenade.ScpDamageMultiplier = scpDamageMultiplier;
            
            if (maxRadius > 0)
                grenade.MaxRadius = maxRadius;
            
            grenade.ChangeItemOwner(Server.Host, owner);
            grenade.SpawnActive(position, owner: owner);
        }
        #endregion

        #region Area Effects
        private void ApplyAreaEffects(Player shooter)
        {
            List<Player> affectedPlayers = GetNearbyPlayers(shooter, MaxAffectDistance);
            
            foreach (Player player in affectedPlayers)
            {
                ApplyStatusEffects(player);
            }
            
            ApplyStatusEffects(shooter);
        }

        private List<Player> GetNearbyPlayers(Player centerPlayer, float maxDistance)
        {
            return Player.List
                .Where(p => p != centerPlayer && Vector3.Distance(centerPlayer.Position, p.Position) <= maxDistance)
                .ToList();
        }

        private void ApplyStatusEffects(Player player)
        {
            player.EnableEffect(EffectType.Flashed, FlashDuration);
            player.EnableEffect(EffectType.Slowness, SlownessIntensity, SlownessDuration);
        }
        #endregion

        #region Visual Effects
        private void CreateVisualEffects(Player player, Vector3 hitPosition)
        {
            if (!TryGetRaycastHit(player, out RaycastHit hit))
                return;

            Transform handBone = GetHandBone(player);
            if (handBone == null)
                return;

            SpawnBeamEffect(handBone.position, hit.point);
            SpawnElectricEffect(hit.point);
        }

        private bool TryGetRaycastHit(Player player, out RaycastHit hit)
        {
            return Physics.Raycast(
                player.CameraTransform.position,
                player.CameraTransform.forward,
                out hit,
                MaxRaycastDistance,
                RaycastMask
            ) && hit.collider != null;
        }

        private Transform GetHandBone(Player player)
        {
            Animator animator = player.GameObject.GetComponentInChildren<Animator>();
            return animator?.GetBoneTransform(HumanBodyBones.RightHand);
        }

        private void SpawnBeamEffect(Vector3 start, Vector3 end)
        {
            Vector3 direction = (end - start).normalized;
            float distance = Vector3.Distance(start, end);

            for (float i = 0; i <= distance; i += BeamSegmentDistance)
            {
                Vector3 position = start + direction * i;
                CreateBeamSegment(position);
            }

            SpawnSpiralEffect(start, end);
        }

        private void CreateBeamSegment(Vector3 position)
        {
            Primitive cube = CreatePrimitive(
                PrimitiveType.Cube,
                position,
                new Vector3(BeamCubeSize, BeamCubeSize, BeamCubeSize),
                new Color(24.1f, 25.7f, 50.0f, 0.1f)
            );

            StartFadeCoroutine(cube, EffectFadeDuration);
        }

        private void SpawnSpiralEffect(Vector3 start, Vector3 end, int turns = DefaultSpiralTurns, float radius = DefaultSpiralRadius)
        {
            Vector3 direction = (end - start).normalized;
            float distance = Vector3.Distance(start, end);
            int steps = Mathf.CeilToInt(distance / BeamSegmentDistance);
            Quaternion rotation = Quaternion.LookRotation(direction);

            for (int i = 0; i < steps; i++)
            {
                float t = i / (float)steps;
                Vector3 basePosition = Vector3.Lerp(start, end, t);
                Vector3 spiralPosition = CalculateSpiralPosition(basePosition, rotation, t, turns, radius);

                Primitive spiralCube = CreatePrimitive(
                    PrimitiveType.Cube,
                    spiralPosition,
                    Vector3.one * SpiralCubeSize,
                    new Color(50f, 0f, 0f, 0.1f)
                );

                StartFadeCoroutine(spiralCube, EffectFadeDuration);
            }
        }

        private Vector3 CalculateSpiralPosition(Vector3 basePosition, Quaternion rotation, float t, int turns, float radius)
        {
            float angle = t * turns * 360f * Mathf.Deg2Rad;
            float xOffset = Mathf.Cos(angle) * radius;
            float yOffset = Mathf.Sin(angle) * radius;
            return basePosition + rotation * new Vector3(xOffset, yOffset, 0);
        }

        public void SpawnElectricEffect(Vector3 position, int boltCount = DefaultBoltCount, float duration = BoltDuration)
        {
            for (int i = 0; i < boltCount; i++)
            {
                Vector3 randomOffset = Random.insideUnitSphere * BoltRandomOffset;
                Vector3 boltPosition = position + randomOffset;

                Primitive bolt = CreatePrimitive(
                    PrimitiveType.Cylinder,
                    boltPosition,
                    new Vector3(0.02f, Random.Range(0.3f, 0.6f), 0.02f),
                    new Color(0.3f, 0.8f, 1f, 1f)
                );

                bolt.Rotation = Quaternion.Euler(
                    Random.Range(-45, 45),
                    Random.Range(0, 360),
                    Random.Range(-45, 45)
                );

                float randomDuration = duration * Random.Range(0.7f, 1.2f);
                StartFadeCoroutine(bolt, randomDuration);
            }
        }

        private Primitive CreatePrimitive(PrimitiveType type, Vector3 position, Vector3 scale, Color color)
        {
            Primitive primitive = Primitive.Create(type);
            primitive.Flags = PrimitiveFlags.Visible;
            primitive.Position = position;
            primitive.Scale = scale;
            primitive.Color = color;
            return primitive;
        }
        #endregion

        #region Coroutine Management
        private void StartFadeCoroutine(Primitive primitive, float duration)
        {
            CoroutineHandle handle = Timing.RunCoroutine(FadeOutAndDestroy(primitive, duration));
            _activeCoroutines.Add(handle);
        }

        private IEnumerator<float> FadeOutAndDestroy(Primitive primitive, float duration)
        {
            float elapsed = 0f;
            Color initialColor = primitive.Color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(initialColor.a, 0f, elapsed / duration);
                primitive.Color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);

                yield return Timing.WaitForOneFrame;
            }

            primitive.Color = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
            primitive.Destroy();
        }

        private void ScheduleCoroutineCleanup()
        {
            Timing.CallDelayed(CoroutineCleanupDelay, StopAllCoroutines);
        }

        private void StopAllCoroutines()
        {
            foreach (CoroutineHandle coroutine in _activeCoroutines)
            {
                Timing.KillCoroutines(coroutine);
            }
            _activeCoroutines.Clear();
        }
        #endregion
    }
}