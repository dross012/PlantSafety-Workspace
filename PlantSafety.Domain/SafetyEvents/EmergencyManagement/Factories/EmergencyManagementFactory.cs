// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Domain
//  File:           EmergencyManagementFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing EmergencyManagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.EmergencyManagement.Factories;

/// <summary>Default factory implementation for constructing <see cref="EmergencyManagement"/> aggregates.</summary>
public sealed class EmergencyManagementFactory : IEmergencyManagementFactory
{
    /// <inheritdoc/>
    public EmergencyManagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => EmergencyManagement.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
