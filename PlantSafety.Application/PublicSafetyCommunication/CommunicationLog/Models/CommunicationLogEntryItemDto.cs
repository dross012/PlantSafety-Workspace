// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single CommunicationLogEntry record. Backs the
// After-Action Communication Timeline dashboard.
// SchemaVersion: 1

namespace PlantSafety.Application.PublicSafetyCommunication.CommunicationLog.Models;

/// <summary>A single CommunicationLogEntry read-model record.</summary>
public sealed record CommunicationLogEntryItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid CommunicationLogEntryId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the originating incident, if any.</summary>
    public Guid? IncidentId { get; init; }

    /// <summary>Identity of the related IncidentNotification, if any.</summary>
    public Guid? RelatedIncidentNotificationId { get; init; }

    /// <summary>Identity of the related PublicAlertBroadcast, if any.</summary>
    public Guid? RelatedPublicAlertBroadcastId { get; init; }

    /// <summary>Direction of the communication.</summary>
    public required string Direction { get; init; }

    /// <summary>Medium used for the communication.</summary>
    public required string Medium { get; init; }

    /// <summary>Name of the external counterparty involved.</summary>
    public required string CounterpartyName { get; init; }

    /// <summary>Free-text summary of the communication content.</summary>
    public required string Summary { get; init; }

    /// <summary>UTC timestamp the communication actually occurred.</summary>
    public required DateTime OccurredAtUtc { get; init; }

    /// <summary>UTC timestamp this record was logged.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}
