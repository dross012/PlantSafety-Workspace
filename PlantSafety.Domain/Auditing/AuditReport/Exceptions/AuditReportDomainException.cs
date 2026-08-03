// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           AuditReportDomainException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised by the AuditReport
//                  submodule. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Common.Exceptions;

namespace PlantSafety.Domain.Auditing.AuditReport.Exceptions;

/// <summary>Abstract base for all domain exceptions raised by the AuditReport submodule.</summary>
public abstract class AuditReportDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected AuditReportDomainException(
        string message, string errorCode, string errorCategory, string severity,
        CorrelationId correlationId, CausationId causationId)
        : base(message, errorCode, errorCategory, severity, correlationId, causationId)
    {
    }
}
