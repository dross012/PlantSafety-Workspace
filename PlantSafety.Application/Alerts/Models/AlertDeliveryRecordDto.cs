// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model DTO for a single Alert multi-channel delivery attempt.
// SchemaVersion: 1

namespace PlantSafety.Application.Alerts.Models;

/// <summary>Immutable read-model representation of a single Alert delivery attempt.</summary>
public sealed record AlertDeliveryRecordDto
{
    /// <summary>Delivery record identifier.</summary>
    public required Guid DeliveryRecordId { get; init; }

    /// <summary>Identifier of the parent Alert.</summary>
    public required Guid AlertId { get; init; }

    /// <summary>Delivery channel used (e.g. PaSpeaker, Sms, MobilePush).</summary>
    public required string Channel { get; init; }

    /// <summary>Whether the delivery attempt succeeded.</summary>
    public required bool Succeeded { get; init; }

    /// <summary>UTC instant the delivery attempt was made.</summary>
    public required DateTime AttemptedAtUtc { get; init; }

    /// <summary>Sanitized failure reason, when the attempt did not succeed.</summary>
    public string? FailureReason { get; init; }
}
