// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Domain
//  File:           InvalidIncidentReportDataException.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown when IncidentReport.Create is supplied with data that violates the
//                  aggregate's invariants (empty narrative, negative injury count, negative
//                  property damage estimate).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Incidents.Exceptions;

/// <summary>Thrown when the data supplied to <see cref="Aggregates.IncidentReport.Create"/> is invalid.</summary>
public sealed class InvalidIncidentReportDataException : IncidentReportDomainException
{
    /// <summary>Creates a validation exception describing why the supplied data is invalid.</summary>
    public InvalidIncidentReportDataException(string reason, CorrelationId correlationId, CausationId causationId)
        : base(
            $"Invalid IncidentReport data: {reason}",
            "IncidentReport.InvalidData",
            "Validation",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
