// ============================================================================
//  File:           AlertSlaViolationException.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Exceptions;

public sealed class AlertSlaViolationException : AlertDomainException
{
    public AlertSlaViolationException(
        string alertId,
        string reason,
        CorrelationId correlationId,
        CausationId causationId)
        : base(
            message: $"SLA violation for alert '{alertId}': {reason}.",
            errorCode: "ALERT.SLA_VIOLATION",
            errorCategory: "SLA",
            severity: "Critical",
            correlationId: correlationId,
            causationId: causationId)
    { }
}
