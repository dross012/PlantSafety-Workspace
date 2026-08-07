// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Domain
//  File:           IncidentReportEventId.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for IncidentReport domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Incidents.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a raised IncidentReport domain event.</summary>
public readonly record struct IncidentReportEventId(Guid Value)
{
    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
