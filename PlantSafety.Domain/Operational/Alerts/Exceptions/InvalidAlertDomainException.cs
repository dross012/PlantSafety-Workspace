// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           InvalidAlertDomainException.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Thrown when a domain operation on an Alert violates domain rules,
//      invariants, lifecycle constraints, or business-model fidelity.
//      Examples:
//          - Attempting an illegal lifecycle transition
//          - Performing an action in an invalid state
//          - Violating a domain invariant (e.g., missing required fields)
//          - Attempting to modify immutable domain data
//
//      Complies with:
//          - Exception Handling Architectural Rule (2.14)
//          - Canonical Error Envelope Rule (14.4)
//          - StrongId Enforcement Rule (5.5)
//          - Deterministic Time Rule (1.1)
//          - SIEM-Safe Requirements
//          - Additive-only evolution
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Exceptions;

public sealed class InvalidAlertDomainException : AlertDomainException
{
    public InvalidAlertDomainException(
        string reason,
        CorrelationId correlationId,
        CausationId causationId)
        : base(
            message: $"Invalid alert domain operation: {reason}.",
            errorCode: "ALERT.INVALID_OPERATION",
            errorCategory: "Domain",
            severity: "Error",
            correlationId: correlationId,
            causationId: causationId)
    { }

    // Add this constructor to match the usage in AlertValidationService
    public InvalidAlertDomainException(
        string message,
        string errorCode,
        string errorCategory,
        string severity,
        CorrelationId correlationId,
        CausationId causationId)
        : base(message, errorCode, errorCategory, severity, correlationId, causationId)
    {
    }
}

