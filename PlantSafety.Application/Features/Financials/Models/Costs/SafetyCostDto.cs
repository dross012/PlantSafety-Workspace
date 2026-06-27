// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side DTO for a Safety Cost record.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Financials.Models.Costs;

/// <summary>
/// Read-side projection of a SafetyCost record.
/// Tracks individual expenditure line items against a safety budget.
/// </summary>
public sealed record SafetyCostDto
{
    /// <summary>Unique identifier (UUIDv7).</summary>
    public required Guid CostId { get; init; }

    /// <summary>Tenant owning this record.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Budget this cost is charged against.</summary>
    public required Guid BudgetId { get; init; }

    /// <summary>Site this cost is associated with.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Cost description. Max 500 characters.</summary>
    public required string Description { get; init; }

    /// <summary>Cost category (mirrors budget category).</summary>
    public required string Category { get; init; }

    /// <summary>Amount of this expenditure.</summary>
    public required decimal Amount { get; init; }

    /// <summary>Currency code (ISO 4217).</summary>
    public required string CurrencyCode { get; init; }

    /// <summary>UTC timestamp when the cost was incurred.</summary>
    public required DateTimeOffset IncurredAtUtc { get; init; }

    /// <summary>Reference to the source event that triggered this cost (e.g., IncidentId).</summary>
    public Guid? SourceEventId { get; init; }

    /// <summary>Type of source event (e.g., Incident, Drill, Training).</summary>
    public string? SourceEventType { get; init; }

    /// <summary>UTC timestamp of last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }
}
