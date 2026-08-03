// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           AuditReportInvalidStateException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when an operation is attempted against an AuditReport that is not in
//                  a valid lifecycle state for it. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Auditing.AuditReport.ValueObjects.Ids;

namespace PlantSafety.Domain.Auditing.AuditReport.Exceptions;

/// <summary>Thrown when an AuditReport operation is attempted from an invalid lifecycle state.</summary>
public sealed class AuditReportInvalidStateException : AuditReportDomainException
{
    /// <summary>Creates an invalid-state exception for the given AuditReport identity.</summary>
    public AuditReportInvalidStateException(AuditReportId id, string currentStatus, string attemptedOperation,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot perform '{attemptedOperation}' on AuditReport {id} while status is '{currentStatus}'.",
            "AuditReport.InvalidState",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
