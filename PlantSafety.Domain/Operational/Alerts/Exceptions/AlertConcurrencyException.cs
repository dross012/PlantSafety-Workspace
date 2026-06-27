// ============================================================================
//  File:           AlertConcurrencyException.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Exceptions;

public sealed class AlertConcurrencyException : AlertDomainException
{
    public AlertConcurrencyException(
        string alertId,
        CorrelationId correlationId,
        CausationId causationId)
        : base(
            message: $"Concurrency conflict detected for alert '{alertId}'.",
            errorCode: "ALERT.CONCURRENCY_CONFLICT",
            errorCategory: "Concurrency",
            severity: "Error",
            correlationId: correlationId,
            causationId: causationId)
    { }
}
