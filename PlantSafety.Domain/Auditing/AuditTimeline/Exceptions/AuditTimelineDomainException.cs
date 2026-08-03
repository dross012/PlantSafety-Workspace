// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Domain
//  File:           AuditTimelineDomainException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised by the AuditTimeline
//                  submodule. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Common.Exceptions;

namespace PlantSafety.Domain.Auditing.AuditTimeline.Exceptions;

/// <summary>Abstract base for all domain exceptions raised by the AuditTimeline submodule.</summary>
public abstract class AuditTimelineDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected AuditTimelineDomainException(
        string message, string errorCode, string errorCategory, string severity,
        CorrelationId correlationId, CausationId causationId)
        : base(message, errorCode, errorCategory, severity, correlationId, causationId)
    {
    }
}
