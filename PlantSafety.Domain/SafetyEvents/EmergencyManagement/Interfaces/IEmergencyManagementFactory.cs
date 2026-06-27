// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Domain
//  File:           IEmergencyManagementFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing EmergencyManagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.EmergencyManagement.Interfaces;

/// <summary>Factory interface for constructing <see cref="EmergencyManagement"/> aggregates.</summary>
public interface IEmergencyManagementFactory
{
    /// <summary>Creates a new EmergencyManagement aggregate with the supplied parameters.</summary>
    EmergencyManagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
