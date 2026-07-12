// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Read-model item DTO for a single outbound email notification delivery record.
// SchemaVersion: 1
namespace PlantSafety.Application.Integrations.Notifications.Email.Models;

/// <summary>Represents a single outbound email notification delivery record.</summary>
public sealed record EmailItemDto
{
    /// <summary>Unique identifier of the email notification delivery record.</summary>
    public required Guid EmailDeliveryId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Email address the notification was delivered to.</summary>
    public required string RecipientAddress { get; init; }
    /// <summary>Subject line of the notification email.</summary>
    public required string Subject { get; init; }
    /// <summary>Name of the email template used to render the notification.</summary>
    public string? TemplateName { get; init; }

    /// <summary>Current delivery status (e.g., Queued/Sent/Delivered/Failed/Bounced).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the email was sent.</summary>
    public required DateTime SentAtUtc { get; init; }

    /// <summary>Identity of the user or process that triggered the notification.</summary>
    public required Guid SentBy { get; init; }
}