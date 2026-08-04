// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single CommunityNotification record.
// SchemaVersion: 1

namespace PlantSafety.Application.CommunitySafety.CommunityNotification.Models;

/// <summary>A single CommunityNotification record (outreach/notification history).</summary>
public sealed record CommunityNotificationItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid CommunityNotificationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Delivery channel used.</summary>
    public required string Channel { get; init; }

    /// <summary>Weak reference to the recipient CommunityContact.</summary>
    public required Guid RecipientContactId { get; init; }

    /// <summary>Notification subject line.</summary>
    public required string Subject { get; init; }

    /// <summary>Weak reference to the related CommunityConcern, if any.</summary>
    public Guid? RelatedConcernId { get; init; }

    /// <summary>Weak reference to the related SafetyEvents incident, if any.</summary>
    public Guid? RelatedIncidentId { get; init; }

    /// <summary>Overall lifecycle status.</summary>
    public required string NotificationStatus { get; init; }

    /// <summary>UTC instant the notification was sent.</summary>
    public required DateTime SentAtUtc { get; init; }

    /// <summary>UTC instant the recipient acknowledged the notification, if acknowledged.</summary>
    public DateTime? AcknowledgedAtUtc { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
