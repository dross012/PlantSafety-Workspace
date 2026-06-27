// ============================================================================
//  File:           AlertInvalidStatusTransitionException.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Exceptions;

public sealed class AlertInvalidStatusTransitionException : AlertDomainException
{
    public AlertInvalidStatusTransitionException(
        AlertStatus current,
        AlertStatus attempted,
        CorrelationId correlationId,
        CausationId causationId)
        : base(
            message: $"Invalid lifecycle transition: {current} → {attempted}.",
            errorCode: "ALERT.INVALID_TRANSITION",
            errorCategory: "Lifecycle",
            severity: "Error",
            correlationId: correlationId,
            causationId: causationId)
    { }
}
