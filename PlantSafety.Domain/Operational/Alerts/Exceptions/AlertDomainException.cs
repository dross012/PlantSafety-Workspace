// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertDomainException.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Canonical base class for all Alerts domain exceptions.
//      Complies with:
//          - Exception Handling Architectural Rule (2.14)
//          - Canonical Error Envelope Rule (14.4)
//          - StrongId Enforcement Rule (5.5)
//          - Deterministic Time Rule (1.1)
//          - SIEM-Safe Requirements
//          - Additive-only evolution
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Exceptions;

public abstract class AlertDomainException : Exception
{
    public Guid ExceptionId { get; }
    public string ErrorCode { get; }
    public string ErrorCategory { get; }
    public string Severity { get; }
    public CorrelationId CorrelationId { get; }
    public CausationId CausationId { get; }
    public UtcTimestamp OccurredAtUtc { get; }

    protected AlertDomainException(
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
