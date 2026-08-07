// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Domain
//  File:           IIncidentReportFactory.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing IncidentReport aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Enums;

namespace PlantSafety.Domain.Compliance.PHMSA.Incidents.Interfaces;

/// <summary>Factory interface for constructing <see cref="Aggregates.IncidentReport"/> aggregates.</summary>
public interface IIncidentReportFactory
{
    /// <summary>Creates a new IncidentReport aggregate with the supplied parameters.</summary>
    Aggregates.IncidentReport Create(
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
        PerformedByActor actor);
}
