using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using UnityEngine;

namespace LilinsAdditions.Main.Items.Weapons.Pistols;

[CustomItem(ItemType.GunCOM15)]
public class ChainLightningGun : CustomWeapon
{
    public override uint Id { get; set; } = 204;
    public override string Name { get; set; } = "Mk.IV Arc Emitter";

    public override string Description { get; set; } =
        "A prototype pistol that turns its victim into a living conductor.";

    public override float Weight { get; set; } = 0.1f;
    public override SpawnProperties SpawnProperties { get; set; }

    public float ChainRange { get; set; } = 5f;
    public int MaxChains { get; set; } = 3;
    public float LightningDuration { get; set; } = 1f;
    public Color LightningColor { get; set; } = Color.cyan;

    public int LightningSegments { get; set; } = 8;
    public float LightningJaggedness { get; set; } = 0.3f;

    public float FirstChainDamageMultiplier { get; set; } = 0.5f;
    public float DamageReductionPerChain { get; set; } = 0.15f;

    protected override void OnShot(ShotEventArgs ev)
    {
        base.OnShot(ev);

        if (ev.Target is null || !(ev.Target is Player targetPlayer))
            return;

        var alreadyHit = new HashSet<Player> { targetPlayer };

        ChainLightning(targetPlayer, ev.Player, alreadyHit, 1, ev.Damage);
    }

    private void ChainLightning(Player currentTarget, Player originalAttacker, HashSet<Player> alreadyHit,
        int chainNumber, float baseShotDamage)
    {
        if (alreadyHit.Count >= MaxChains + 1)
            return;

        var nearbyPlayers = Player.List
            .Where(p => p != currentTarget &&
                        !alreadyHit.Contains(p) &&
                        Vector3.Distance(p.Position, currentTarget.Position) <= ChainRange)
            .OrderBy(p => Vector3.Distance(p.Position, currentTarget.Position))
            .FirstOrDefault();

        if (nearbyPlayers != null)
        {
            var chainDamage = CalculateChainDamage(chainNumber, baseShotDamage);

            DrawLightningBolt(currentTarget.Position, nearbyPlayers.Position);

            nearbyPlayers.Hurt(originalAttacker, chainDamage, DamageType.Tesla);

            alreadyHit.Add(nearbyPlayers);

            ChainLightning(nearbyPlayers, originalAttacker, alreadyHit, chainNumber + 1, baseShotDamage);
        }
    }

    private float CalculateChainDamage(int chainNumber, float baseShotDamage)
    {
        var damageMultiplier = FirstChainDamageMultiplier - DamageReductionPerChain * (chainNumber - 1);
        damageMultiplier = Mathf.Max(damageMultiplier, 0.1f);

        return baseShotDamage * damageMultiplier;
    }

    private void DrawLightningBolt(Vector3 start, Vector3 end)
    {
        var lightningPoints = GenerateLightningPath(start, end);

        Draw.Path(lightningPoints, LightningColor, LightningDuration, Player.List);

        if (Random.value < 0.3f)
        {
            var branchPoint = Random.Range(1, lightningPoints.Length - 1);
            var branchEnd = lightningPoints[branchPoint] + Random.insideUnitSphere * 3f;
            var branchPoints = GenerateLightningPath(lightningPoints[branchPoint], branchEnd, 4);
            Draw.Path(branchPoints, LightningColor * 0.7f, LightningDuration * 0.8f, Player.List);
        }
    }

    private Vector3[] GenerateLightningPath(Vector3 start, Vector3 end, int segments = -1)
    {
        if (segments == -1)
            segments = LightningSegments;

        var points = new Vector3[segments + 1];
        points[0] = start;
        points[segments] = end;

        var direction = (end - start).normalized;
        var distance = Vector3.Distance(start, end);

        var perp1 = Vector3.Cross(direction, Vector3.up).normalized;
        if (perp1.magnitude < 0.1f)
            perp1 = Vector3.Cross(direction, Vector3.forward).normalized;
        var perp2 = Vector3.Cross(direction, perp1).normalized;

        for (var i = 1; i < segments; i++)
        {
            var t = (float)i / segments;
            var basePoint = Vector3.Lerp(start, end, t);

            var offsetMagnitude = distance * LightningJaggedness * Random.Range(-0.5f, 0.5f);
            var angle = Random.Range(0f, 2f * Mathf.PI);

            var offset = perp1 * Mathf.Cos(angle) * offsetMagnitude +
                         perp2 * Mathf.Sin(angle) * offsetMagnitude;

            points[i] = basePoint + offset;
        }

        return points;
    }
}