// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Domain
//  File:           IncidentReportFactory.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing IncidentReport aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Interfaces;

namespace PlantSafety.Domain.Compliance.PHMSA.Incidents.Factories;

/// <summary>Default factory implementation for constructing <see cref="Aggregates.IncidentReport"/> aggregates.</summary>
public sealed class IncidentReportFactory : IIncidentReportFactory
{
    /// <inheritdoc/>
    public Aggregates.IncidentReport Create(
        IncidentType incidentType,
        HazmatMaterialId? materialInvolvedId,
        string? materialInvolvedDescription,
        int injuries,
        bool fatalitiesInvolved,
        decimal? propertyDamageEstimateUsd,
        string narrative,
        DateTime incidentDateUtc,
        string location,
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => Aggregates.IncidentReport.Create(
            incidentType,
            materialInvolvedId,
            materialInvolvedDescription,
            injuries,
            fatalitiesInvolved,
            propertyDamageEstimateUsd,
            narrative,
            incidentDateUtc,
            location,
            createdBy,
            createdAtUtc,
            correlationId,
            causationId,
            actor);
}
