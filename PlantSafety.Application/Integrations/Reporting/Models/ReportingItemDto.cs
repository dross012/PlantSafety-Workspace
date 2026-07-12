// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Read-model item DTO representing a report delivery record for integration-layer reporting activity to external stakeholders.
// SchemaVersion: 1

namespace PlantSafety.Application.Integrations.Reporting.Models;

/// <summary>Represents a report delivery record for integration-layer reporting activity to external stakeholders.</summary>
public sealed record ReportingItemDto
{
    /// <summary>Identifier of the report delivery.</summary>
    public required Guid DeliveryId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the report being delivered.</summary>
    public required string ReportName { get; init; }

    /// <summary>External stakeholder or system the report was delivered to.</summary>
    public required string RecipientSystem { get; init; }

    /// <summary>Delivery channel used (e.g., Email, API, SFTP).</summary>
    public required string DeliveryChannel { get; init; }

    /// <summary>Current delivery status (e.g., Queued/Sent/Failed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the report was delivered.</summary>
    public required DateTime DeliveredAtUtc { get; init; }

    /// <summary>Identity of the user or service principal that initiated the delivery.</summary>
    public required Guid InitiatedBy { get; init; }
}
