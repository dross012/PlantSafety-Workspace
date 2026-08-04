// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single CommunityContact record.
// SchemaVersion: 1

namespace PlantSafety.Application.CommunitySafety.CommunityContact.Models;

/// <summary>
/// A single CommunityContact read-model row. Deliberately PII-minimal per the CommunitySafety
/// data-governance mandate: only display name, classification, coarse community area, and
/// outreach contact channels are exposed.
/// </summary>
public sealed record CommunityContactItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid CommunityContactId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Display name of the contact.</summary>
    public required string DisplayName { get; init; }

    /// <summary>Classification of the community relationship (Resident, LocalOfficial, EmergencyServices, Media, CommunityGroup, RegulatoryAgency).</summary>
    public required string ContactType { get; init; }

    /// <summary>Organization/affiliation, if any.</summary>
    public string? Organization { get; init; }

    /// <summary>Coarse-grained community area label.</summary>
    public required string CommunityArea { get; init; }

    /// <summary>Whether this contact is opted in to receive proactive community notifications.</summary>
    public required bool OptedInForNotifications { get; init; }

    /// <summary>Lifecycle status (Active, Inactive, Archived).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
