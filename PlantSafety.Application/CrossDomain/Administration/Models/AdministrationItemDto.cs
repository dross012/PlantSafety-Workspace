// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Administration record.
// SchemaVersion: 1

namespace PlantSafety.Application.CrossDomain.Administration.Models;

/// <summary>A single platform administration record.</summary>
public sealed record AdministrationItemDto
{
    /// <summary>Administration record identifier.</summary>
    public required Guid RecordId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Configuration setting key.</summary>
    public required string SettingKey { get; init; }

    /// <summary>Configuration setting value.</summary>
    public required string SettingValue { get; init; }

    /// <summary>Setting category (e.g., Security/Notifications/Tenancy).</summary>
    public required string Category { get; init; }

    /// <summary>Current record status (e.g., Active/PendingReview/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the record was last modified.</summary>
    public required DateTime LastModifiedAtUtc { get; init; }

    /// <summary>Identity of the user who last modified the record.</summary>
    public required Guid LastModifiedBy { get; init; }
}
