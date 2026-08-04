// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.ControlChangeEvent
//  Layer:          Domain
//  File:           ControlChangeDomainEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for ControlChangeEvent domain events. Named
//                  "ControlChangeDomainEventId" (rather than "ControlChangeEventEventId") to avoid
//                  an awkward doubled "Event" in the type name.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for ControlChangeEvent domain events.</summary>
public readonly record struct ControlChangeDomainEventId(Guid Value)
{
    /// <summary>Creates a new ControlChangeDomainEventId using Uuid7.</summary>
    public static ControlChangeDomainEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
