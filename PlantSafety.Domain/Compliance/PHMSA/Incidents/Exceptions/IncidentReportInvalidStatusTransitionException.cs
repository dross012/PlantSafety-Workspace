// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Domain
//  File:           IncidentReportInvalidStatusTransitionException.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown when an IncidentReport lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Incidents.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Incidents.Exceptions;

/// <summary>Thrown when an <see cref="Aggregates.IncidentReport"/> lifecycle transition is not permitted.</summary>
public sealed class IncidentReportInvalidStatusTransitionException : IncidentReportDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public IncidentReportInvalidStatusTransitionException(
        IncidentReportId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition IncidentReport '{id}' from '{from}' to '{to}'.",
            "IncidentReport.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
