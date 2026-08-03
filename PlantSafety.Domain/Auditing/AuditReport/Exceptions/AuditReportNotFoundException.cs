// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           AuditReportNotFoundException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when an AuditReport aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Auditing.AuditReport.ValueObjects.Ids;

namespace PlantSafety.Domain.Auditing.AuditReport.Exceptions;

/// <summary>Thrown when an AuditReport aggregate lookup finds no match.</summary>
public sealed class AuditReportNotFoundException : AuditReportDomainException
{
    /// <summary>Creates a not-found exception for the given AuditReport identity.</summary>
    public AuditReportNotFoundException(AuditReportId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"AuditReport with id '{id}' was not found.",
            "AuditReport.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
