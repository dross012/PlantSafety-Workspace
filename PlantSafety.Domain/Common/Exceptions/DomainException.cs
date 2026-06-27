// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Common
//  Layer:          Domain
//  File:           DomainException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Abstract base for all domain exceptions across PlantSafety.Domain.
//      Module-specific exception bases (e.g. AlertDomainException) inherit
//      from this type to satisfy the canonical exception contract.
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Common.Exceptions;

/// <summary>Abstract base for domain exceptions. Carries SIEM-safe metadata.</summary>
public abstract class DomainException : Exception
{
    /// <summary>Unique identifier for this exception instance (Uuid7).</summary>
    public Guid ExceptionId { get; }

    /// <summary>Machine-readable error code (e.g. "Alert.NotFound").</summary>
    public string ErrorCode { get; }

    /// <summary>Broad category for routing/alerting (e.g. "NotFound", "Validation").</summary>
    public string ErrorCategory { get; }

    /// <summary>Operational severity: Critical, High, Medium, Low, Info.</summary>
    public string Severity { get; }

    /// <summary>Request correlation identifier for distributed tracing.</summary>
    public CorrelationId CorrelationId { get; }

    /// <summary>Command or event that caused this exception.</summary>
    public CausationId CausationId { get; }

    /// <summary>UTC instant at which this exception was created.</summary>
    public UtcTimestamp OccurredAtUtc { get; }

    /// <summary>Initialises the exception with all canonical SIEM fields.</summary>
    protected DomainException(
        string message,
        string errorCode,
        string errorCategory,
        string severity,
        CorrelationId correlationId,
        CausationId causationId)
        : base(message)
    {
        ExceptionId = Uuid7.New();
        ErrorCode = errorCode;
        ErrorCategory = errorCategory;
        Severity = severity;
        CorrelationId = correlationId;
        CausationId = causationId;
        OccurredAtUtc = UtcTimestamp.Now();
    }
}
