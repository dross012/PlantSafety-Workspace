// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Domain
//  File:           IncidentReportNotFoundException.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown when an IncidentReport aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Incidents.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Incidents.Exceptions;

/// <summary>Thrown when an <see cref="Aggregates.IncidentReport"/> aggregate lookup finds no match.</summary>
public sealed class IncidentReportNotFoundException : IncidentReportDomainException
{
    /// <summary>Creates a not-found exception for the given IncidentReport identity.</summary>
    public IncidentReportNotFoundException(IncidentReportId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"IncidentReport with id '{id}' was not found.",
            "IncidentReport.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
