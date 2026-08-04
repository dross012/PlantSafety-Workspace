// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SabotageIndicator
//  Layer:          Domain
//  File:           SabotageIndicatorEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for SabotageIndicator domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for SabotageIndicator domain events.</summary>
public readonly record struct SabotageIndicatorEventId(Guid Value)
{
    /// <summary>Creates a new SabotageIndicatorEventId using Uuid7.</summary>
    public static SabotageIndicatorEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
