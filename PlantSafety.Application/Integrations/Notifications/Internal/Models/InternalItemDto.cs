// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Read-model item DTO for a single in-app internal platform notification delivered to a user.
// SchemaVersion: 1
namespace PlantSafety.Application.Integrations.Notifications.Internal.Models;

/// <summary>Represents a single in-app internal platform notification delivered to a user.</summary>
public sealed record InternalItemDto
{
    /// <summary>Unique identifier of the internal notification delivery record.</summary>
    public required Guid NotificationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>User the internal notification was delivered to.</summary>
    public required Guid RecipientUserId { get; init; }
    /// <summary>Notification message body.</summary>
    public required string Message { get; init; }
    /// <summary>Notification category (e.g., Alert/Reminder/Info).</summary>
    public string? Category { get; init; }

    /// <summary>Current notification status (e.g., Unread/Read/Dismissed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the notification was delivered.</summary>
    public required DateTime DeliveredAtUtc { get; init; }

    /// <summary>Identity of the user or process that triggered the notification.</summary>
    public required Guid TriggeredBy { get; init; }
}