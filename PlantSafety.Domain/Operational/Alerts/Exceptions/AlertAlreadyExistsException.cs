// ============================================================================
//  File:           AlertAlreadyExistsException.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Exceptions;

public sealed class AlertAlreadyExistsException : AlertDomainException
{
    public AlertAlreadyExistsException(
        string alertId,
        CorrelationId correlationId,
        CausationId causationId)
        : base(
            message: $"Alert '{alertId}' already exists.",
            errorCode: "ALERT.ALREADY_EXISTS",
            errorCategory: "Conflict",
            severity: "Error",
            correlationId: correlationId,
            causationId: causationId)
    { }
}
