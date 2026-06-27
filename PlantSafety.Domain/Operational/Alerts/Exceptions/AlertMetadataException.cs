// ============================================================================
//  File:           AlertMetadataException.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Exceptions;

public sealed class AlertMetadataException : AlertDomainException
{
    public AlertMetadataException(
        string reason,
        CorrelationId correlationId,
        CausationId causationId)
        : base(
            message: $"Metadata operation failed: {reason}.",
            errorCode: "ALERT.METADATA_ERROR",
            errorCategory: "Metadata",
            severity: "Error",
            correlationId: correlationId,
            causationId: causationId)
    { }
}
