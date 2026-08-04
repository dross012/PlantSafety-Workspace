// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single PublicAlertBroadcast record. Backs the
// Broadcast History dashboard and the broadcast delivery success rate KPI.
// SchemaVersion: 1

namespace PlantSafety.Application.PublicSafetyCommunication.PublicAlertBroadcast.Models;

/// <summary>A single PublicAlertBroadcast read-model record.</summary>
public sealed record PublicAlertBroadcastItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PublicAlertBroadcastId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current delivery lifecycle status.</summary>
    public required string Status { get; init; }

    /// <summary>Identity of the originating incident, if any.</summary>
    public Guid? IncidentId { get; init; }

    /// <summary>Severity level of this broadcast.</summary>
    public required string SeverityCode { get; init; }

    /// <summary>Target broadcast channel.</summary>
    public required string ChannelCode { get; init; }

    /// <summary>Short headline for the alert.</summary>
    public required string Headline { get; init; }

    /// <summary>Full alert message content.</summary>
    public required string Message { get; init; }

    /// <summary>Description of the geographic area targeted by this broadcast.</summary>
    public required string TargetArea { get; init; }

    /// <summary>UTC timestamp the broadcast was sent, if any.</summary>
    public DateTime? IssuedAtUtc { get; init; }

    /// <summary>UTC timestamp delivery was confirmed, if any.</summary>
    public DateTime? DeliveryConfirmedAtUtc { get; init; }

    /// <summary>Reason delivery failed, if applicable.</summary>
    public string? FailureReason { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Optimistic-concurrency token (native RowVersion projected as a hex string).</summary>
    public string? ETag { get; init; }
}
