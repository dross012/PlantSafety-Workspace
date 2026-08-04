// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single IncidentNotification dispatch record. Backs
// the notification dispatch latency and agency acknowledgement rate KPIs.
// SchemaVersion: 1

namespace PlantSafety.Application.PublicSafetyCommunication.IncidentNotificationDispatch.Models;

/// <summary>A single IncidentNotification dispatch read-model record.</summary>
public sealed record IncidentNotificationItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid IncidentNotificationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current dispatch lifecycle status.</summary>
    public required string Status { get; init; }

    /// <summary>Identity of the originating incident, if any.</summary>
    public Guid? IncidentId { get; init; }

    /// <summary>Identity of the receiving PublicSafetyAgency.</summary>
    public required Guid PublicSafetyAgencyId { get; init; }

    /// <summary>Channel used to deliver this notification.</summary>
    public required string ChannelCode { get; init; }

    /// <summary>Notification message content.</summary>
    public required string Message { get; init; }

    /// <summary>UTC timestamp the notification was dispatched, if any.</summary>
    public DateTime? DispatchedAtUtc { get; init; }

    /// <summary>UTC timestamp the agency acknowledged receipt, if any.</summary>
    public DateTime? AcknowledgedAtUtc { get; init; }

    /// <summary>Name/desk of the agency contact who acknowledged receipt, if any.</summary>
    public string? AcknowledgedByContactName { get; init; }

    /// <summary>UTC timestamp the notification lifecycle was closed, if any.</summary>
    public DateTime? ClosedAtUtc { get; init; }

    /// <summary>Reason delivery failed, if applicable.</summary>
    public string? FailureReason { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Optimistic-concurrency token (native RowVersion projected as a hex string).</summary>
    public string? ETag { get; init; }
}
