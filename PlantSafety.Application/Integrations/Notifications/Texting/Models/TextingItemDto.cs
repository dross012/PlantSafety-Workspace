// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Read-model item DTO for a single outbound SMS notification delivery record triggered by a platform notification.
// SchemaVersion: 1
namespace PlantSafety.Application.Integrations.Notifications.Texting.Models;

/// <summary>Represents a single outbound SMS notification delivery record triggered by a platform notification.</summary>
public sealed record TextingItemDto
{
    /// <summary>Unique identifier of the notification SMS delivery record.</summary>
    public required Guid SmsDeliveryId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Phone number the notification SMS was delivered to.</summary>
    public required string RecipientPhoneNumber { get; init; }
    /// <summary>Text content of the notification SMS.</summary>
    public required string MessageBody { get; init; }
    /// <summary>Name of the platform event that triggered the SMS notification.</summary>
    public string? TriggerEvent { get; init; }

    /// <summary>Current delivery status (e.g., Queued/Sent/Delivered/Failed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the SMS was sent.</summary>
    public required DateTime SentAtUtc { get; init; }

    /// <summary>Identity of the user or process that triggered the notification.</summary>
    public required Guid SentBy { get; init; }
}