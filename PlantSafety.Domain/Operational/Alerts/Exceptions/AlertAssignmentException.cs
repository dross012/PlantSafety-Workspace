// ============================================================================
//  File:           AlertAssignmentException.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Exceptions;

public sealed class AlertAssignmentException : AlertDomainException
{
    public AlertAssignmentException(
        string reason,
        CorrelationId correlationId,
        CausationId causationId)
        : base(
            message: $"Assignment failed: {reason}.",
            errorCode: "ALERT.ASSIGNMENT_FAILED",
            errorCategory: "Assignment",
            severity: "Error",
            correlationId: correlationId,
            causationId: causationId)
    { }
}
