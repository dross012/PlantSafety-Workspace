// ============================================================================
//  File:           AlertNotFoundException.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Exceptions;

public sealed class AlertNotFoundException : AlertDomainException
{
    public AlertNotFoundException(
        string alertId,
        CorrelationId correlationId,
        CausationId causationId)
        : base(
            message: $"Alert '{alertId}' was not found.",
            errorCode: "ALERT.NOT_FOUND",
            errorCategory: "Lookup",
            severity: "Warning",
            correlationId: correlationId,
            causationId: causationId)
    { }
}
